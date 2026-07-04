#!/usr/bin/env python3
"""Rigli karakterlere Meshy animasyon kütüphanesinden klip üretir.
Varsayılan: 9 kahramana Idle(0) + Attack(4). Çıktı: output/<char>/rig/<isim>.glb"""
import json
import os
import sys
import time
import urllib.error
import urllib.request

HERE = os.path.dirname(os.path.abspath(__file__))
KEY = [l.split("=", 1)[1].strip() for l in open(os.path.join(HERE, ".env"))
       if l.startswith("MESHY_API_KEY=")][0]

HEROES = ["kael", "faelyn", "elwin", "baldric", "milo", "pip", "sylwen", "ravox", "borin"]
ACTIONS = {0: "idle", 4: "attack"}  # katalog indeksi: dosya adı


def req(method, path, payload=None):
    r = urllib.request.Request("https://api.meshy.ai" + path, method=method)
    r.add_header("Authorization", "Bearer " + KEY)
    data = json.dumps(payload).encode() if payload else None
    if payload:
        r.add_header("Content-Type", "application/json")
    with urllib.request.urlopen(r, data, timeout=60) as resp:
        return json.loads(resp.read())


def download(url, dest):
    for attempt in range(4):
        try:
            with urllib.request.urlopen(urllib.request.Request(url), timeout=300) as r, open(dest, "wb") as f:
                f.write(r.read())
            return
        except Exception as e:
            print("  indirme tekrar:", e, flush=True)
            time.sleep(4)
    raise RuntimeError("indirme başarısız: " + dest)


failed = []
for char in HEROES:
    rig_dir = os.path.join(HERE, "output", char, "rig")
    rig_id_path = os.path.join(rig_dir, "rig_id.txt")
    if not os.path.exists(rig_id_path):
        print(char, ": rig yok, atlandı", flush=True)
        continue
    rig_id = open(rig_id_path).read().strip()

    for action_id, name in ACTIONS.items():
        dest = os.path.join(rig_dir, name + ".glb")
        if os.path.exists(dest):
            print(char, name, ": var, atlandı", flush=True)
            continue
        try:
            t = req("POST", "/openapi/v1/animations",
                    {"rig_task_id": rig_id, "action_id": action_id})
            tid = t["result"]
            status = None
            for _ in range(40):
                st = req("GET", "/openapi/v1/animations/" + tid)
                status = st.get("status")
                if status in ("SUCCEEDED", "FAILED", "CANCELED"):
                    break
                time.sleep(7)
            if status != "SUCCEEDED":
                raise RuntimeError(str(st.get("task_error")))
            download(st["result"]["animation_glb_url"], dest)
            print(char, name, ": TAMAM ✓", flush=True)
        except Exception as e:
            print(char, name, ": HATA ->", e, flush=True)
            failed.append(char + "/" + name)

print("=" * 40, flush=True)
print("BİTTİ — hata:", failed, flush=True)
print("Bakiye:", req("GET", "/openapi/v1/balance")["balance"], flush=True)
