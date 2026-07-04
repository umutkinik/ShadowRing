#!/usr/bin/env python3
"""
Gölgehalka — Meshy TOPLU üretim hattı.

Birden çok prompt dosyasındaki tüm asset'leri paralel işçilerle üretir:
preview -> refine(PBR) -> GLB + thumbnail indir.

Kullanım:
  python3 meshy_batch.py prompts/heroes.json prompts/enemies.json ...
  python3 meshy_batch.py --workers 3 prompts/*.json

Özellikler:
  - output/<key>/<key>.glb zaten varsa ATLAR (kaldığı yerden devam).
  - 429 / eşzamanlılık limitinde bekleyip yeniden dener.
  - Her asset bağımsız: biri hata verirse diğerleri devam eder.
  - Durum: output/batch_status.json  +  stdout log.
Python 3.9 uyumlu, sadece stdlib.
"""
import argparse
import json
import os
import sys
import threading
import time
import urllib.error
import urllib.request
from concurrent.futures import ThreadPoolExecutor, as_completed

BASE = "https://api.meshy.ai"
HERE = os.path.dirname(os.path.abspath(__file__))
POLL_SECONDS = 20
TIMEOUT_MINUTES = 60
RATE_LIMIT_WAIT = 60

_print_lock = threading.Lock()
_status_lock = threading.Lock()
_status = {}


def log(msg):
    with _print_lock:
        print(f"[{time.strftime('%H:%M:%S')}] {msg}", flush=True)


def set_status(key, stage):
    with _status_lock:
        _status[key] = stage
        path = os.path.join(HERE, "output", "batch_status.json")
        with open(path, "w") as f:
            json.dump(_status, f, indent=2, ensure_ascii=False)


def load_key():
    key = os.environ.get("MESHY_API_KEY", "")
    env_path = os.path.join(HERE, ".env")
    if not key and os.path.exists(env_path):
        for line in open(env_path):
            if line.strip().startswith("MESHY_API_KEY="):
                key = line.strip().split("=", 1)[1]
    if not key:
        sys.exit("HATA: MESHY_API_KEY yok.")
    return key


KEY = load_key()


class RateLimited(Exception):
    pass


def api(method, path, payload=None):
    req = urllib.request.Request(BASE + path, method=method)
    req.add_header("Authorization", "Bearer " + KEY)
    data = None
    if payload is not None:
        data = json.dumps(payload).encode()
        req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, data, timeout=60) as r:
            return json.loads(r.read())
    except urllib.error.HTTPError as e:
        body = e.read().decode(errors="replace")
        if e.code == 429 or "concurrent" in body.lower() or "limit" in body.lower():
            raise RateLimited(body)
        raise RuntimeError("API %d: %s" % (e.code, body))


def api_retry(method, path, payload=None, label=""):
    """Hız/eşzamanlılık limitinde bekleyerek dener — toplu işte kritik."""
    while True:
        try:
            return api(method, path, payload)
        except RateLimited:
            log("%s: limit — %ds bekleniyor..." % (label, RATE_LIMIT_WAIT))
            time.sleep(RATE_LIMIT_WAIT)


def poll(task_id, key, stage):
    deadline = time.time() + TIMEOUT_MINUTES * 60
    last = -1
    while time.time() < deadline:
        t = api_retry("GET", "/openapi/v2/text-to-3d/" + task_id, label=key)
        s, p = t.get("status"), t.get("progress", 0)
        if p != last:
            set_status(key, "%s %%%d" % (stage, p))
            last = p
        if s == "SUCCEEDED":
            return t
        if s in ("FAILED", "CANCELED"):
            raise RuntimeError("%s %s: %s" % (stage, s, t.get("task_error")))
        time.sleep(POLL_SECONDS)
    raise RuntimeError(stage + " zaman aşımı")


def download(url, dest):
    with urllib.request.urlopen(urllib.request.Request(url), timeout=300) as r, open(dest, "wb") as f:
        f.write(r.read())


def produce(key, cfg):
    out_dir = os.path.join(HERE, "output", key)
    glb = os.path.join(out_dir, key + ".glb")
    if os.path.exists(glb):
        log("%s: zaten var, atlanıyor ✓" % key)
        set_status(key, "SKIP (mevcut)")
        return "skip"
    os.makedirs(out_dir, exist_ok=True)
    log("%s (%s): başlıyor" % (key, cfg["display_name"]))

    set_status(key, "preview gönderildi")
    pid = api_retry("POST", "/openapi/v2/text-to-3d", {
        "mode": "preview",
        "prompt": cfg["prompt"],
        "art_style": cfg.get("art_style", "realistic"),
        "should_remesh": True,
        "topology": "triangle",
        "target_polycount": cfg.get("target_polycount", 12000),
    }, label=key)["result"]
    poll(pid, key, "preview")
    log("%s: preview ✓ — refine başlıyor" % key)

    rid = api_retry("POST", "/openapi/v2/text-to-3d", {
        "mode": "refine", "preview_task_id": pid, "enable_pbr": True,
    }, label=key)["result"]
    final = poll(rid, key, "refine")

    urls = final.get("model_urls", {})
    if urls.get("glb"):
        download(urls["glb"], glb)
    if final.get("thumbnail_url"):
        download(final["thumbnail_url"], os.path.join(out_dir, key + "_thumb.png"))
    with open(os.path.join(out_dir, key + "_meta.json"), "w") as f:
        json.dump(final, f, indent=2)
    set_status(key, "TAMAM")
    log("%s: TAMAM ✓ (%.1f MB)" % (key, os.path.getsize(glb) / 1e6))
    return "ok"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+", help="prompt JSON dosyaları")
    ap.add_argument("--workers", type=int, default=3)
    args = ap.parse_args()

    assets = {}
    for fp in args.files:
        assets.update(json.load(open(fp)))
    log("Toplam %d asset, %d paralel işçi" % (len(assets), args.workers))
    log("Bakiye: %d kredi" % api("GET", "/openapi/v1/balance")["balance"])

    results = {"ok": 0, "skip": 0, "fail": 0}
    failed = []
    with ThreadPoolExecutor(max_workers=args.workers) as ex:
        futures = {ex.submit(produce, k, c): k for k, c in assets.items()}
        for fut in as_completed(futures):
            k = futures[fut]
            try:
                results[fut.result()] += 1
            except Exception as e:
                results["fail"] += 1
                failed.append(k)
                set_status(k, "HATA: %s" % e)
                log("%s: HATA — %s" % (k, e))

    log("=" * 50)
    log("BİTTİ — üretilen: %d, atlanan: %d, hata: %d" % (results["ok"], results["skip"], results["fail"]))
    if failed:
        log("Hatalılar (tekrar çalıştırınca sadece bunlar üretilir): " + ", ".join(failed))
    log("Kalan bakiye: %d kredi" % api("GET", "/openapi/v1/balance")["balance"])


if __name__ == "__main__":
    main()
