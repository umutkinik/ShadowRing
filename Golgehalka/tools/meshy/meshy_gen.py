#!/usr/bin/env python3
"""
Gölgehalka — Meshy text-to-3D üretim hattı.

Akış: preview (geometri) -> refine (PBR doku) -> GLB indir.
Kullanım:
  python3 meshy_gen.py --char borin                  # prompts/heroes.json içinden
  python3 meshy_gen.py --char borin --preview-only   # sadece geometri (daha az kredi)
  python3 meshy_gen.py --list                        # tanımlı karakterleri listele

API anahtarı .env dosyasından (MESHY_API_KEY) veya ortam değişkeninden okunur.
Sadece stdlib kullanır (urllib) — ek paket gerekmez.
"""
import argparse
import json
import os
import sys
import time
import urllib.error
import urllib.request

BASE = "https://api.meshy.ai"
HERE = os.path.dirname(os.path.abspath(__file__))
POLL_SECONDS = 20
TIMEOUT_MINUTES = 45


def load_key() -> str:
    key = os.environ.get("MESHY_API_KEY", "")
    env_path = os.path.join(HERE, ".env")
    if not key and os.path.exists(env_path):
        for line in open(env_path):
            line = line.strip()
            if line.startswith("MESHY_API_KEY="):
                key = line.split("=", 1)[1].strip()
    if not key:
        sys.exit("HATA: MESHY_API_KEY bulunamadı (.env veya ortam değişkeni).")
    return key


KEY = load_key()


def api(method: str, path: str, payload: dict = None) -> dict:
    req = urllib.request.Request(BASE + path, method=method)
    req.add_header("Authorization", f"Bearer {KEY}")
    data = None
    if payload is not None:
        data = json.dumps(payload).encode()
        req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, data, timeout=60) as r:
            return json.loads(r.read())
    except urllib.error.HTTPError as e:
        body = e.read().decode(errors="replace")
        sys.exit(f"HATA: API {e.code} — {body}")


def poll(task_id: str, label: str) -> dict:
    deadline = time.time() + TIMEOUT_MINUTES * 60
    last_progress = -1
    while time.time() < deadline:
        t = api("GET", f"/openapi/v2/text-to-3d/{task_id}")
        status = t.get("status")
        progress = t.get("progress", 0)
        if progress != last_progress:
            print(f"[{label}] {status} %{progress}", flush=True)
            last_progress = progress
        if status == "SUCCEEDED":
            return t
        if status in ("FAILED", "CANCELED"):
            sys.exit(f"HATA: {label} görevi {status}: {t.get('task_error')}")
        time.sleep(POLL_SECONDS)
    sys.exit(f"HATA: {label} zaman aşımı ({TIMEOUT_MINUTES} dk).")


def download(url: str, dest: str) -> None:
    req = urllib.request.Request(url)
    with urllib.request.urlopen(req, timeout=300) as r, open(dest, "wb") as f:
        f.write(r.read())
    size_mb = os.path.getsize(dest) / 1e6
    print(f"  indirildi: {dest} ({size_mb:.1f} MB)", flush=True)


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--char", help="prompts/heroes.json içindeki karakter anahtarı")
    ap.add_argument("--preview-only", action="store_true", help="refine (doku) adımını atla")
    ap.add_argument("--list", action="store_true", help="tanımlı karakterleri listele")
    args = ap.parse_args()

    chars = json.load(open(os.path.join(HERE, "prompts", "heroes.json")))
    if args.list or not args.char:
        print("Tanımlı karakterler:")
        for k, v in chars.items():
            print(f"  {k:10s} — {v['display_name']}")
        return
    if args.char not in chars:
        sys.exit(f"HATA: '{args.char}' bulunamadı. --list ile bakın.")

    c = chars[args.char]
    out_dir = os.path.join(HERE, "output", args.char)
    os.makedirs(out_dir, exist_ok=True)

    bal = api("GET", "/openapi/v1/balance")
    print(f"Bakiye: {bal['balance']} kredi", flush=True)

    # 1) PREVIEW — geometri üretimi
    print(f"== {c['display_name']} — preview başlıyor ==", flush=True)
    preview_id = api("POST", "/openapi/v2/text-to-3d", {
        "mode": "preview",
        "prompt": c["prompt"],
        "art_style": c.get("art_style", "realistic"),
        "should_remesh": True,
        "topology": "triangle",
        "target_polycount": c.get("target_polycount", 15000),
    })["result"]
    print(f"preview task: {preview_id}", flush=True)
    preview = poll(preview_id, "preview")

    final_task = preview
    if not args.preview_only:
        # 2) REFINE — PBR doku
        print("== refine (doku) başlıyor ==", flush=True)
        refine_id = api("POST", "/openapi/v2/text-to-3d", {
            "mode": "refine",
            "preview_task_id": preview_id,
            "enable_pbr": True,
        })["result"]
        print(f"refine task: {refine_id}", flush=True)
        final_task = poll(refine_id, "refine")

    # 3) İNDİR
    print("== indiriliyor ==", flush=True)
    urls = final_task.get("model_urls", {})
    if urls.get("glb"):
        download(urls["glb"], os.path.join(out_dir, f"{args.char}.glb"))
    if final_task.get("thumbnail_url"):
        download(final_task["thumbnail_url"], os.path.join(out_dir, f"{args.char}_thumb.png"))
    with open(os.path.join(out_dir, f"{args.char}_meta.json"), "w") as f:
        json.dump(final_task, f, indent=2)

    bal = api("GET", "/openapi/v1/balance")
    print(f"TAMAM ✓  Kalan bakiye: {bal['balance']} kredi", flush=True)
    print(f"Sonraki adım: {args.char}.glb → Blender kontrol → Mixamo rig → Unity import", flush=True)


if __name__ == "__main__":
    main()
