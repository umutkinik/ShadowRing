#!/usr/bin/env python3
"""
Meshy Auto-Rigging toplu işi: her karakter için rig + yürüme/koşma animasyonu.
Çıktı: output/<char>/rig/ altına rigged.glb, walking.glb, running.glb (+fbx).
Mevcut çıktıyı atlar; poz hatası verenleri listeler (ör. gövdeyi çaprazlayan silah).
"""
import json
import os
import sys
import time
import urllib.error
import urllib.request

HERE = os.path.dirname(os.path.abspath(__file__))
KEY = [l.split("=", 1)[1].strip() for l in open(os.path.join(HERE, ".env"))
       if l.startswith("MESHY_API_KEY=")][0]

CHARS = {  # karakter: tahmini boy (pose estimation'a yardım)
    "kael": 1.8, "faelyn": 1.8, "elwin": 1.8, "baldric": 1.85, "milo": 1.2,
    "pip": 1.2, "sylwen": 1.85, "ravox": 1.8, "borin": 1.4,
    "bloodclaw": 2.0, "shroud_king": 2.0, "stone_behemoth": 2.5,
    "malketh": 1.9, "molgroth": 2.4, "zarok": 2.2,
}


def req(method, path, payload=None):
    r = urllib.request.Request("https://api.meshy.ai" + path, method=method)
    r.add_header("Authorization", "Bearer " + KEY)
    data = None
    if payload is not None:
        data = json.dumps(payload).encode()
        r.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(r, data, timeout=60) as resp:
            return resp.status, json.loads(resp.read())
    except urllib.error.HTTPError as e:
        try:
            return e.code, json.loads(e.read())
        except Exception:
            return e.code, {}


def download(url, dest):
    for attempt in range(3):
        try:
            with urllib.request.urlopen(urllib.request.Request(url), timeout=300) as r, open(dest, "wb") as f:
                f.write(r.read())
            return
        except Exception as e:
            print("  indirme tekrar (%d/3): %s" % (attempt + 1, e), flush=True)
            time.sleep(4)
    raise RuntimeError("indirme 3 denemede başarısız: " + dest)


failed, done = [], []
for char, height in CHARS.items():
    rig_dir = os.path.join(HERE, "output", char, "rig")
    if os.path.exists(os.path.join(rig_dir, "rigged.glb")):
        print(char, ": zaten var, atlandı", flush=True)
        done.append(char)
        continue

    meta_path = os.path.join(HERE, "output", char, char + "_meta.json")
    if not os.path.exists(meta_path):
        print(char, ": meta yok, atlandı", flush=True)
        continue
    task_id = json.load(open(meta_path))["id"]

    os.makedirs(rig_dir, exist_ok=True)
    rig_id_path = os.path.join(rig_dir, "rig_id.txt")
    if os.path.exists(rig_id_path):
        rig_id = open(rig_id_path).read().strip()
        print(char, ": mevcut rig görevi kullanılıyor", rig_id, flush=True)
    else:
        code, body = req("POST", "/openapi/v1/rigging",
                         {"input_task_id": task_id, "height_meters": height})
        if code not in (200, 201, 202):
            print(char, ": RIG REDDEDILDI ->", code, body.get("message"), flush=True)
            failed.append(char)
            continue
        rig_id = body.get("result") or body.get("id")
        open(rig_id_path, "w").write(rig_id)
        print(char, ": rig başladı", rig_id, flush=True)

    status = None
    for _ in range(60):
        _, t = req("GET", "/openapi/v1/rigging/" + rig_id)
        status = t.get("status")
        if status in ("SUCCEEDED", "FAILED", "CANCELED"):
            break
        time.sleep(12)

    if status != "SUCCEEDED":
        print(char, ": RIG BAŞARISIZ ->", (t.get("task_error") or {}), flush=True)
        failed.append(char)
        continue

    try:
        res = t["result"]
        download(res["rigged_character_glb_url"], os.path.join(rig_dir, "rigged.glb"))
        download(res["rigged_character_fbx_url"], os.path.join(rig_dir, "rigged.fbx"))
        ba = res.get("basic_animations", {})
        if ba.get("walking_glb_url"):
            download(ba["walking_glb_url"], os.path.join(rig_dir, "walking.glb"))
        if ba.get("running_glb_url"):
            download(ba["running_glb_url"], os.path.join(rig_dir, "running.glb"))
        with open(os.path.join(rig_dir, "rig_meta.json"), "w") as f:
            json.dump(t, f, indent=2)
        print(char, ": TAMAM ✓", flush=True)
        done.append(char)
    except Exception as e:
        print(char, ": İNDİRME HATASI ->", e, flush=True)
        failed.append(char)

print("=" * 50, flush=True)
print("BİTTİ — başarılı:", len(done), "hata:", failed, flush=True)
_, b = req("GET", "/openapi/v1/balance")
print("Kalan bakiye:", b.get("balance"), flush=True)
