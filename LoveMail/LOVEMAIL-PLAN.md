# LoveMail — Çoklu Domain E-posta Servisi: A'dan Z'ye Proje Planı

> Domainler: `lovemail.cool` · `lovemail.fun` · `lovemail.live` · `lovemail.lol` · `lovemail.wtf`
> Durum: VPS hazır, Cloudflare DNS hazır. Hedef lansman: **14 Şubat 2027** (Sevgililer Günü).
> İlke: **Kendi başına çalışabilen, en düşük sabit maliyetli altyapı.** Ücretli API yalnızca giden posta için, o da kullandıkça öde.

---

## 0. Özet Karar Tablosu

| Katman | Seçim | Neden / Maliyet |
|---|---|---|
| Mail sunucusu (gelen, depolama, IMAP, JMAP) | **Stalwart Mail Server** (self-host, Docker) | Tek binary, Rust, çoklu domain yerleşik, JMAP + IMAP + SMTP, yerleşik spam filtresi, DKIM/ARC/DMARC, Let's Encrypt (Cloudflare DNS-01). **Ücretsiz.** |
| Giden posta relay | **Amazon SES** (SMTP relay) | 1.000 mail = **0,10 $**. Aylık sabit ücret yok. Dünyanın en ucuz relay'i. VPS'lerde 25. port genelde kapalı; SES 587 üzerinden çalışır. |
| Yedek relay | Resend (ücretsiz 3.000/ay) veya Brevo (ücretsiz 300/gün) | AWS hesabı gecikirse ilk haftalarda köprü olarak. |
| Reverse proxy + TLS | **Caddy** | Otomatik sertifika, sıfır konfigürasyon. Ücretsiz. |
| Uygulama (kayıt, viral özellikler, API) | **Next.js (TypeScript) + Postgres + Drizzle ORM** | Tek repo, tek dil, tek deploy. Ücretsiz. |
| Web posta istemcisi | Faz 1: **SnappyMail** (hazır, hafif) · Faz 2: Kendi JMAP istemcimiz | Lansmana hızlı ulaşmak için hazır istemci, sonra markalı deneyim. |
| Veritabanı | **PostgreSQL 16** (Docker) | Hem Stalwart hem uygulama kullanır. Ücretsiz. |
| Obje depolama / yedek | VPS diski + **Backblaze B2** yedek (6 $/TB) veya Hetzner Storage Box (~4 €/ay 1 TB) | Mail gövdeleri önce diskte; büyüyünce Stalwart S3 backend'e geçer (B2 S3 uyumlu). |
| Bot koruması | **Cloudflare Turnstile** | Ücretsiz. SMS doğrulama YOK (pahalı). |
| İçerik moderasyonu | OpenAI Moderation API (ücretsiz) + yerel kelime listesi | Anonim mesaj özellikleri için şart. |
| Antivirüs | ClamAV (Docker) | Ücretsiz. Faz 2'de eklenir. |
| İzleme | Uptime Kuma (self-host) + Stalwart dahili metrikler | Ücretsiz. |
| Mobil uygulama | **Expo (React Native)** + JMAP, tek kod tabanı iOS + Android | Web istemciyle aynı TypeScript JMAP katmanı paylaşılır. Expo ücretsiz; EAS Build ücretsiz kotası yetmezse yerel build. Push: Expo Push (ücretsiz) → FCM/APNs. |
| Ödeme (Faz 3) | Lemon Squeezy veya Paddle (Merchant of Record) | Türkiye'den global tahsilat, vergi onlar halleder. Satış başına komisyon, sabit ücret yok. |

**Tahmini aylık sabit maliyet: VPS ücreti + ~5-10 $** (yedek + SES). 10.000 kullanıcı günde 1 mail atsa SES = ~30 $/ay.

---

## 1. Mimari

```
                      ┌──────────────────────────── Cloudflare (DNS, Turnstile, Pages) ────────────────────────────┐
                      │   MX (5 domain) ──► mail.lovemail.cool (A kaydı, GRİ bulut / DNS-only)                     │
                      │   app.lovemail.cool, lovemail.live (TURUNCU bulut / proxy) ──► Caddy                        │
                      └───────────────────────────────────────────────────────────────────────────────────────────┘
                                                        │
   ┌────────────────────────────────────────────────────┼─────────────────────────────── VPS ─────────────────────┐
   │                                                    ▼                                                          │
   │   :25 :465 :587 :993 :443(jmap)            :80/:443 Caddy                                                     │
   │   ┌──────────────────────┐                 ┌────────────────────┐    ┌────────────────────┐                   │
   │   │   Stalwart Mail      │◄── JMAP/REST ───│  Next.js App       │    │  SnappyMail        │                   │
   │   │   SMTP/IMAP/JMAP     │                 │  kayıt, kimlikler, │    │  webmail (Faz 1)   │                   │
   │   │   spam filtre, DKIM  │                 │  itiraf, public    │    │  IMAP → Stalwart   │                   │
   │   │   kota, fail2ban     │                 │  sayfa, davet,     │    └────────────────────┘                   │
   │   └──────────┬───────────┘                 │  zaman kapsülü     │                                             │
   │              │ giden                       └─────────┬──────────┘                                             │
   │              ▼                                       ▼                                                        │
   │   Amazon SES (587, relay)                  PostgreSQL 16 (stalwart_db + lovemail_db)                          │
   │                                                                                                               │
   │   Disk: /srv/mail (gövdeler + indeks)  ──► gece yedeği ──► Backblaze B2                                       │
   └───────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Domain rolleri

| Domain | Rol | Kullanıcı kimliği tonu |
|---|---|---|
| `lovemail.cool` | Ana site, `app.` (uygulama), `mail.` (MX host), `api.`, `webmail.` | Varsayılan, "ciddi ama tatlı" |
| `lovemail.live` | Herkese açık profil ve anonim mesaj sayfaları: `lovemail.live/u/<kullanici>` | Yayıncı, link-in-bio |
| `lovemail.fun` | Oyun, etkinlik, zaman kapsülü vitrini | Eğlence |
| `lovemail.lol` | Meme ve şaka modu | Komik |
| `lovemail.wtf` | Burner (tek kullanımlık) adresler + sarkastik kimlik | Sert |

**Tek MX hostname** (`mail.lovemail.cool`) beş domaine de hizmet verir. Tek sertifika, tek PTR, tek itibar yönetimi.

### Hesap modeli

- Kullanıcı `ahmet` adını alır → otomatik **5 adres**: `ahmet@lovemail.{cool,fun,live,lol,wtf}`.
- Stalwart'ta tek "principal", 5 e-posta alias. Tek gelen kutusu.
- Gönderirken "kimlik seç" (From domaini). Her kimliğin kendi imzası ve rengi.
- `*.lovemail.wtf` altında rastgele burner alias: `kayit-3f9a2c@lovemail.wtf` → ana kutuya düşer, tek tıkla kapatılır.

---

## 2. Bugün: VPS Hazırlığı (Gün 1)

### 2.1 Ön kontroller (önce bunlar, 10 dakika)

```bash
# 25. port gelen açık mı? (başka bir makineden) — mail almak için ŞART
nc -zv <VPS_IP> 25

# 25. port giden açık mı? (VPS içinden) — kapalıysa sorun değil, SES 587 kullanacağız
nc -zv smtp.gmail.com 25

# PTR (reverse DNS) — VPS sağlayıcı panelinden ayarlanır:
#   <VPS_IP>  →  mail.lovemail.cool
dig -x <VPS_IP> +short
```

> Hetzner, DigitalOcean, Oracle, Contabo gibi sağlayıcılarda giden 25 kapalı olabilir; bazıları gelen 25'i de ticket ile açar. Gelen 25 kapalıysa sağlayıcıya ticket aç, bu olmadan mail alamazsın.

### 2.2 Sunucu sertleştirme

```bash
apt update && apt -y upgrade
apt -y install ufw fail2ban unattended-upgrades curl git jq

# SSH: sadece anahtar
sed -i 's/^#\?PasswordAuthentication.*/PasswordAuthentication no/' /etc/ssh/sshd_config
systemctl restart ssh

# Güvenlik duvarı
ufw default deny incoming
ufw default allow outgoing
ufw allow 22/tcp
ufw allow 80,443/tcp          # web + ACME
ufw allow 25,465,587/tcp      # SMTP, SMTPS, Submission
ufw allow 993/tcp             # IMAPS
ufw allow 4190/tcp            # ManageSieve (opsiyonel)
ufw enable

dpkg-reconfigure -plow unattended-upgrades
```

### 2.3 Docker

```bash
curl -fsSL https://get.docker.com | sh
mkdir -p /srv/lovemail/{stalwart,postgres,caddy,app,snappymail,backup}
```

### 2.4 Hostname

```bash
hostnamectl set-hostname mail.lovemail.cool
echo "<VPS_IP> mail.lovemail.cool mail" >> /etc/hosts
```

---

## 3. DNS (Cloudflare) — Her 5 Domain İçin

### 3.1 Cloudflare'de kapatılması gerekenler

- **Email Routing**: 5 zone'da da **kapalı** olmalı (kendi MX'imizle çakışır).
- `mail.lovemail.cool` A/AAAA kaydı **DNS-only (gri bulut)**. Proxy'li mail çalışmaz.
- Web hostları (`app.`, `lovemail.live`, kök domainler) proxy'li (turuncu) olabilir.

### 3.2 Sadece `lovemail.cool`

```
A      mail           <VPS_IPv4>        DNS only
AAAA   mail           <VPS_IPv6>        DNS only   (varsa)
A      @              <VPS_IPv4>        Proxied
CNAME  app            lovemail.cool     Proxied
CNAME  webmail        lovemail.cool     Proxied
CNAME  api            lovemail.cool     Proxied
CNAME  mta-sts        lovemail.cool     Proxied
```

### 3.3 Her domain için ortak şablon (`cool`, `fun`, `live`, `lol`, `wtf`)

```
MX     @              10 mail.lovemail.cool.
TXT    @              "v=spf1 mx include:amazonses.com -all"
TXT    _dmarc         "v=DMARC1; p=none; rua=mailto:dmarc@lovemail.cool; ruf=mailto:dmarc@lovemail.cool; adkim=r; aspf=r"
                      # 2 hafta sonra p=quarantine, 2 ay sonra p=reject
TXT    _smtp._tls     "v=TLSRPTv1; rua=mailto:tlsrpt@lovemail.cool"
TXT    _mta-sts       "v=STSv1; id=20261108"
CNAME  mta-sts        mta-sts.lovemail.cool       Proxied
SRV    _imaps._tcp    0 1 993 mail.lovemail.cool.
SRV    _submission._tcp 0 1 587 mail.lovemail.cool.
SRV    _jmap._tcp     0 1 443 mail.lovemail.cool.
CAA    @              0 issue "letsencrypt.org"
CAA    @              0 issue "pki.goog"            # Cloudflare proxy sertifikaları için
```

**DKIM kayıtları:** Stalwart admin panelinde her domain eklenince DKIM anahtarı üretir ve kopyalanacak TXT'yi gösterir (`<selector>._domainkey`). Ayrıca SES her domain için 3 adet CNAME (Easy DKIM) verir. İkisini de ekle.

### 3.4 MTA-STS policy dosyası

`https://mta-sts.lovemail.<tld>/.well-known/mta-sts.txt` adresinden sunulur (Caddy veya Cloudflare Pages):

```
version: STSv1
mode: testing
mx: mail.lovemail.cool
max_age: 604800
```

---

## 4. Stalwart Kurulumu

### 4.1 `/srv/lovemail/docker-compose.yml`

```yaml
services:
  postgres:
    image: postgres:16-alpine
    restart: unless-stopped
    environment:
      POSTGRES_USER: lovemail
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: lovemail
    volumes:
      - ./postgres:/var/lib/postgresql/data
      - ./init-db.sql:/docker-entrypoint-initdb.d/init.sql:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U lovemail"]
      interval: 10s

  stalwart:
    image: stalwartlabs/stalwart:latest
    restart: unless-stopped
    depends_on: [postgres]
    ports:
      - "25:25"
      - "465:465"
      - "587:587"
      - "993:993"
      - "4190:4190"
      - "127.0.0.1:8080:8080"     # admin + JMAP, Caddy arkasında
    volumes:
      - ./stalwart:/opt/stalwart
    environment:
      - STALWART_DB_URL=postgres://lovemail:${POSTGRES_PASSWORD}@postgres/stalwart

  caddy:
    image: caddy:2
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./caddy/Caddyfile:/etc/caddy/Caddyfile:ro
      - ./caddy/data:/data
      - ./caddy/config:/config

  snappymail:
    image: djmaze/snappymail:latest
    restart: unless-stopped
    volumes:
      - ./snappymail:/var/lib/snappymail

  app:
    build: ./app
    restart: unless-stopped
    env_file: ./app/.env
    depends_on: [postgres, stalwart]
```

`init-db.sql`:
```sql
CREATE DATABASE stalwart OWNER lovemail;
```

### 4.2 `/srv/lovemail/caddy/Caddyfile`

```
mail.lovemail.cool {
    # JMAP + autoconfig + admin (admin'i IP ile kısıtla)
    @admin path /admin* /api/*
    handle @admin {
        @allowed remote_ip <SENIN_EV_IP>
        handle @allowed { reverse_proxy stalwart:8080 }
        respond 403
    }
    reverse_proxy stalwart:8080
}

webmail.lovemail.cool {
    reverse_proxy snappymail:8888
}

app.lovemail.cool, lovemail.cool, www.lovemail.cool {
    reverse_proxy app:3000
}

lovemail.live, www.lovemail.live {
    reverse_proxy app:3000
}

lovemail.fun, lovemail.lol, lovemail.wtf, www.lovemail.fun, www.lovemail.lol, www.lovemail.wtf {
    redir https://lovemail.cool{uri} permanent
}

mta-sts.lovemail.cool, mta-sts.lovemail.fun, mta-sts.lovemail.live, mta-sts.lovemail.lol, mta-sts.lovemail.wtf {
    respond /.well-known/mta-sts.txt "version: STSv1
mode: testing
mx: mail.lovemail.cool
max_age: 604800
" 200
}
```

> Cloudflare proxy arkasındaki hostlar için Caddy'nin kendi sertifika alması gerekiyorsa SSL modunu **Full (strict)** yap; Caddy HTTP-01 ile proxy arkasında da sertifika alabilir.

### 4.3 İlk açılış

```bash
cd /srv/lovemail
echo "POSTGRES_PASSWORD=$(openssl rand -hex 24)" > .env
docker compose up -d postgres
sleep 10
docker compose up -d stalwart
docker compose logs stalwart | grep -i "admin"   # admin şifresi loglarda bir kez görünür
docker compose up -d caddy snappymail
```

Admin panel: `https://mail.lovemail.cool/admin`

### 4.4 Admin panelde yapılacaklar (sırayla)

1. **Settings → Server → Hostname**: `mail.lovemail.cool`
2. **Settings → TLS → ACME**: Let's Encrypt, challenge **DNS-01 (Cloudflare)**, Cloudflare API token (Zone:DNS:Edit, 5 zone). Böylece 465/993 için de sertifika gelir.
3. **Management → Domains**: 5 domaini ekle. Her biri için **DKIM Create** → verilen TXT'yi Cloudflare'e yapıştır.
4. **Settings → SMTP → Outbound → Routing**: Varsayılan relay = SES
   - Host: `email-smtp.eu-central-1.amazonaws.com`, Port 587, STARTTLS
   - Auth: SES SMTP kullanıcı adı / şifre (IAM'dan üretilir)
5. **Settings → Spam filter**: açık (varsayılan). Bayes öğrenmeyi aktif et.
6. **Settings → Authentication → Fail2ban**: açık (varsayılan).
7. **Settings → Storage → Quota**: varsayılan hesap kotası 1 GB (`1073741824`).
8. **Management → Accounts**: test hesabı `test` → 5 adres.
9. **Settings → Listeners**: JMAP ve HTTP için `https://mail.lovemail.cool` URL'sini doğrula.

### 4.5 Doğrulama

```bash
# Dışarıdan test
swaks --to test@lovemail.lol --server mail.lovemail.cool --tls
# Gmail'e gönder, "Orijinali göster" ile SPF/DKIM/DMARC = PASS kontrol et
# https://www.mail-tester.com  → hedef 10/10
# https://mxtoolbox.com/SuperTool.aspx → blacklist, PTR, MX kontrolü
```

---

## 5. Amazon SES (Giden Posta)

1. AWS hesabı → SES → Bölge: **eu-central-1 (Frankfurt)** (Türkiye'ye en yakın, GDPR içi).
2. **Verified identities → Create → Domain**: 5 domaini ayrı ayrı ekle. Easy DKIM CNAME'lerini Cloudflare'e yapıştır.
3. Her domain için **Custom MAIL FROM domain**: `bounce.lovemail.<tld>` (DMARC SPF hizası için). SES verdiği MX + TXT'yi ekle.
4. **Request production access** (sandbox'tan çıkış): Formda şunu yaz: "Consumer email service, users send personal correspondence, bounce/complaint handling via SNS, rate limiting per user, Stalwart-based inbound". Genelde 24 saat içinde onaylanır.
5. **SMTP credentials** oluştur → Stalwart relay'e gir.
6. **Configuration set + SNS → HTTPS endpoint**: bounce ve complaint bildirimleri `https://api.lovemail.cool/webhooks/ses` adresine düşsün; uygulama o kullanıcıyı uyarsın ve sert bounce'ları bastırsın. Complaint oranı %0,1'i geçerse SES hesabı askıya alır; bu yüzden bu webhook **zorunlu**.

**Fiyat:** 0,10 $/1.000 mail + giden veri. Ek ücret yok, aylık taban yok. Aylık 1 milyon mail = 100 $.

**Geçici köprü:** AWS onayı gecikirse Resend hesabı aç (3.000 mail/ay ücretsiz, SMTP relay destekler), aynı Stalwart ayarına Resend SMTP bilgilerini gir.

---

## 6. Uygulama (Next.js)

### 6.1 Yeni repo yapısı (`lovemail`)

```
lovemail/
├── infra/
│   ├── docker-compose.yml
│   ├── caddy/Caddyfile
│   ├── dns/                     # 5 domain için kayıt şablonları (.txt)
│   └── backup/backup.sh
├── app/                         # Next.js 15 (App Router), TypeScript
│   ├── src/app/
│   │   ├── (marketing)/         # lovemail.cool ana sayfa
│   │   ├── (auth)/signup, login
│   │   ├── (app)/inbox, identities, burners, capsule, invites, settings
│   │   ├── u/[handle]/          # lovemail.live/u/<handle> herkese açık sayfa
│   │   └── api/                 # route handlers
│   ├── src/lib/stalwart.ts      # Stalwart yönetim API istemcisi
│   ├── src/lib/jmap.ts          # JMAP istemcisi (Faz 2)
│   ├── src/db/schema.ts         # Drizzle şeması
│   └── Dockerfile
└── docs/
```

### 6.2 Veri modeli (Postgres, `lovemail` DB)

```sql
users           (id, handle UNIQUE, password_hash, primary_domain, plan, quota_bytes, created_at, verified_at, banned_at)
identities      (id, user_id, domain, display_name, signature, color)          -- 5 satır / kullanıcı
burner_aliases  (id, user_id, local_part UNIQUE, label, active, created_at)    -- @lovemail.wtf
invites         (code UNIQUE, inviter_id, used_by_id, used_at)
short_handle_claims (user_id, handle, unlocked_at)                             -- 3-4 harf, 3 davet ile
confessions     (id, sender_id, target_email_norm, created_at, matched_at)     -- karşılıklı itiraf
public_messages (id, recipient_id, body, sender_ip_hash, moderation_score, status, created_at)
scheduled_mails (id, user_id, from_identity, to_email, subject, body, deliver_at, sent_at)
reports         (id, reporter_id, target_type, target_id, reason, created_at)
blocks          (user_id, blocked_email_norm)
ses_events      (id, type, email, payload, created_at)                         -- bounce/complaint
```

### 6.3 Stalwart hesap sağlama (`src/lib/stalwart.ts`)

Stalwart REST yönetim API'si (`/api/principal`) ile kayıt anında hesap açılır. Uygulama şifreyi Argon2 ile hash'ler ve **aynı hash'i** Stalwart'a verir; Stalwart `$argon2id$...` formatını doğrudan kabul eder. Böylece kullanıcı IMAP/JMAP'e aynı şifreyle girer, iki ayrı şifre deposu olmaz.

```ts
const DOMAINS = ["lovemail.cool", "lovemail.fun", "lovemail.live", "lovemail.lol", "lovemail.wtf"];

export async function provisionAccount(handle: string, argon2Hash: string, quotaBytes: number) {
  const res = await fetch(`${process.env.STALWART_URL}/api/principal`, {
    method: "POST",
    headers: {
      "Authorization": "Basic " + Buffer.from(`admin:${process.env.STALWART_ADMIN_PASSWORD}`).toString("base64"),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      type: "individual",
      name: handle,
      secrets: [argon2Hash],
      emails: DOMAINS.map(d => `${handle}@${d}`),
      quota: quotaBytes,
      roles: ["user"],
    }),
  });
  if (!res.ok) throw new Error(`Stalwart provision failed: ${res.status} ${await res.text()}`);
}

export async function addAlias(handle: string, alias: string) {
  // PATCH /api/principal/{handle} → emails listesine ekle
}
```

Admin API'ye yalnızca Docker ağından erişim verilir (Caddy'de dışa kapalı).

### 6.4 Kayıt akışı

1. Handle seç → canlı uygunluk kontrolü (rezerve liste: admin, postmaster, abuse, dmarc, noreply, root, support, help…). **`postmaster@` ve `abuse@` her domainde sana yönlenmeli** (RFC zorunluluğu, SES de ister).
2. Cloudflare Turnstile.
3. Mevcut bir e-posta ile doğrulama (SMS yok). Doğrulanmadan gönderim kapalı, alım açık.
4. Şifre → Argon2id → Stalwart'a sağla.
5. İlk 7 gün: günde 20 giden mail. 30. günden sonra 200. Spam itibarını korur.

### 6.5 Viral özellikler (uygulama tarafı)

**A. Karşılıklı itiraf (`/crush`)**
- Kullanıcı hedef e-posta girer (herhangi bir domain olabilir, lovemail şart değil).
- `target_email_norm` = küçük harf, nokta/plus temizliği (gmail için).
- Kayıt: `confessions(sender_id, target_email_norm)`.
- Eşleşme kontrolü: hedef adres bir lovemail kullanıcısına aitse ve o kullanıcı gönderenin **herhangi bir** adresine itiraf yapmışsa → iki tarafa da "💘 Eşleştiniz" maili. Aksi halde **hiç kimseye hiçbir şey** gitmez.
- Hedef lovemail'de değilse: "Birisi sana lovemail'de itirafta bulundu, hesap açarsan öğrenirsin" maili (haftada en fazla 1, rate-limit). Bu, kayıt dönüşümünün ana motoru.
- Kötüye kullanım: kullanıcı başına günde 5 itiraf, aynı hedefe ömür boyu 1.

**B. Herkese açık sayfa (`lovemail.live/u/<handle>`)**
- "Bana anonim bir şey söyle" formu. Turnstile zorunlu.
- Mesaj Moderation API'den geçer; skor yüksekse `status=held`, kullanıcı görmeden silinir.
- Kabul edilen mesaj kullanıcının gelen kutusuna `anon@lovemail.live` göndereni ile düşer.
- Kullanıcı mesajı **paylaşılabilir kart** olarak dışa aktarır (OG görsel, `/api/og`). Kartın altında `lovemail.live/u/<handle>` yazar → döngü.
- Göndericinin IP hash'i saklanır (yasal talep ve engelleme için), kullanıcıya gösterilmez.

**C. Zaman kapsülü (`/capsule`)**
- Gelecek tarihe mail planla. Cron (her dakika) `deliver_at <= now()` olanları SES ile gönderir.
- "Bir yıl sonraki kendime" şablonu, yılbaşı kampanyası.

**D. Davet ve kısa handle**
- Her kullanıcıya 5 davet kodu. 3 davet kullanıldığında 3-4 harfli handle talep hakkı.
- Davetle gelen kullanıcı da +1 GB alır.

**E. Burner adresler (`@lovemail.wtf`)**
- Tek tık: `addAlias()` ile rastgele alias. Listeden kapat → alias silinir, o adrese gelen mail reddedilir.
- Ücretsiz plan: 5 aktif burner.

**F. Kimlik seçici**
- Gönderirken From listesi 5 kimlik + aktif burner'lar. SnappyMail'de "identities" olarak otomatik tanımlanır; Faz 2 kendi istemcide tek tıkla.

### 6.6 Moderasyon ve güvenlik (ilk günden)

- Rate limit: Redis'siz, Postgres `rate_limits` tablosu veya Next.js içi `@upstash/ratelimit` yerine basit sayaç. Trafik büyürse Redis eklenir.
- Engelleme: kullanıcı bir adresi engellerse `blocks` tablosu + Stalwart Sieve kuralı ile reddedilir.
- Raporlama: her anonim mesajda "Bildir" düğmesi. 3 rapor → otomatik `held`.
- `abuse@lovemail.cool` ve `postmaster@` → senin kutuna.
- Admin paneli: raporlar, bekleyen mesajlar, kullanıcı yasaklama, SES bounce listesi.
- Yaş sınırı 16+ (KVKK/GDPR rıza yaşı). Kayıtta beyan.

### 6.7 SES webhook (`/api/webhooks/ses`)

- SNS imzasını doğrula.
- `Bounce (Permanent)` → o adrese bir daha gönderme (suppression).
- `Complaint` → kullanıcıyı uyar, 3 complaint → gönderim kapat.

### 6.8 Mobil Uygulama (iOS + Android)

**Neden:** Gelen kutusu alışkanlığı telefonda kurulur. Anlık "💘 Eşleştiniz" ve "Sana anonim mesaj geldi" bildirimleri, paylaşım kartını doğrudan Instagram story'ye atma ve kişi listesinden crush seçme, mobil olmadan yarım kalır. Viral döngünün en güçlü halkası bildirimdir.

**Teknoloji (maliyet en düşük, tek ekip):**

| Katman | Seçim | Not |
|---|---|---|
| Çatı | **Expo SDK (React Native) + TypeScript** | Tek kod tabanı, iOS + Android. Web istemciyle `packages/jmap-client` ve `packages/ui` paketlerini **monorepo'da paylaşır**. |
| Mail erişimi | **JMAP** (Stalwart) | IMAP'ten çok daha az pil ve veri tüketir; push ile değişiklik bildirimi (JMAP push / EventSource). |
| Bildirim | **Expo Push Service → APNs / FCM** | Ücretsiz. Sunucu tarafında Stalwart "yeni mail" olayı → uygulama API'si → Expo push. |
| Kimlik | Aynı şifre, cihazda SecureStore'da JMAP token | Biometrik kilit (Face ID / parmak izi) opsiyonel. |
| Paylaşım | `expo-sharing` + `react-native-view-shot` | Kartı ekran görüntüsü yerine canvas'tan üretip doğrudan Instagram/WhatsApp'a gönderir. |
| Kişiler | `expo-contacts` (izinle) | Crush seçmek için rehberden e-posta çekme; rehber sunucuya **yüklenmez**, eşleşme hash ile cihazda yapılır. |
| Derin link | Universal Links (`lovemail.live/u/<handle>` uygulamayı açar) | Story'den gelen tıklama direkt uygulamaya düşer. |
| Build / dağıtım | EAS Build (ücretsiz kota, aylık sınırlı) veya yerel `expo run:ios/android` | Ücretsiz kota aşılırsa Mac'te yerel build; EAS Update ile mağazasız JS güncellemesi. |
| Analitik | PostHog (self-host, ücretsiz) | Huni: indirme → kayıt → ilk itiraf → paylaşım. |

**Sabit maliyet:** Apple Developer **99 $/yıl**, Google Play **25 $ tek sefer**. Başka zorunlu ücret yok.

**Monorepo yapısı (güncel):**

```
lovemail/
├── apps/
│   ├── web/          # Next.js (kayıt, viral sayfalar, admin, Faz 2 webmail)
│   └── mobile/       # Expo (iOS + Android)
├── packages/
│   ├── jmap-client/  # Stalwart JMAP istemcisi (web + mobil ortak)
│   ├── ui/           # Tasarım sistemi (Tamagui veya NativeWind ile web+native)
│   ├── api-types/    # Zod şemaları, API sözleşmesi
│   └── config/       # ESLint, TS ayarları
└── infra/
```

**MVP ekranları (öncelik sırasıyla):**
1. Giriş / kayıt (Turnstile yerine App Attest + Play Integrity, ücretsiz)
2. Gelen kutusu (JMAP, çekerek yenile, sonsuz kaydırma, okundu/yıldız/arşiv)
3. Mail oku + hızlı emoji tepki
4. Yeni mail + kimlik seçici (5 domain + burner'lar)
5. Crush: rehberden veya elle e-posta → itiraf gönder → eşleşme ekranı (konfeti)
6. Anonim mesajlar sekmesi → kart oluştur → paylaş
7. Burner adres oluştur / kapat
8. Zaman kapsülü
9. Bildirim ayarları, hesap, kimlik renkleri

**Mağaza riskleri ve önlemler:**
- Apple **Guideline 1.2 (kullanıcı üretimi içerik)**: raporla, engelle, 24 saat içinde kaldırma, moderasyon **zorunlu**. Bunlar uygulamada görünür olmalı, yoksa red.
- Anonim mesajlaşma uygulamaları (NGL, Sarahah) geçmişte kaldırıldı. Ayrım: lovemail bir **e-posta istemcisi**; anonim mesaj yan özellik. Mağaza açıklamasında ana kategori "Productivity / E-posta".
- 16+ yaş derecelendirmesi. Apple'da 17+ gerekebilir; kabul et.
- Hesap silme uygulama içinden tek tıkla (Apple zorunluluğu).
- Google Play: Data Safety formu, rehber izni gerekçesi açıkça yazılır.

**Yayın stratejisi:**
- Lansmanda (Hafta 8) **PWA**: ana ekrana ekle + Web Push. Mağaza beklemeden mobil deneyim.
- Hafta 9-12 web JMAP istemcisi ile **aynı anda** mobil geliştirilir (ortak paket sayesinde).
- Hafta 13: TestFlight (iOS) + Internal Testing (Android), 100 beta kullanıcı.
- Hafta 14: mağaza gönderimi. Apple inceleme 1-3 gün; red ihtimaline karşı **14 Şubat'tan en az 3 hafta önce** gönder.
- 14 Şubat: her iki mağazada canlı, web ile aynı anda kampanya.


---

## 7. Yedekleme ve İşletim

```bash
# /srv/lovemail/infra/backup/backup.sh  (cron: her gece 03:00)
#!/bin/bash
set -e
DATE=$(date +%F)
docker compose -f /srv/lovemail/docker-compose.yml exec -T postgres pg_dumpall -U lovemail | gzip > /srv/lovemail/backup/pg-$DATE.sql.gz
tar czf /srv/lovemail/backup/stalwart-$DATE.tgz -C /srv/lovemail stalwart
rclone copy /srv/lovemail/backup b2:lovemail-backup --min-age 0 --transfers 4
find /srv/lovemail/backup -mtime +7 -delete
```

- `rclone` + Backblaze B2 (ilk 10 GB ücretsiz, sonra 6 $/TB).
- Uptime Kuma: SMTP 25, IMAPS 993, HTTPS app, SES kota sayfası.
- Haftalık: mail-tester skoru, DMARC raporları (ücretsiz: dmarc-report görüntüleyici veya `parsedmarc` self-host).
- Disk %80 → S3 backend'e geçiş (Stalwart blob store → B2).

---

## 8. Yasal ve Uyumluluk (Türkiye + AB)

- **KVKK aydınlatma metni** ve **açık rıza** (anonim mesaj IP hash'i için).
- **VERBİS** kaydı: yıllık çalışan < 50 ve bilanço < 100 M TL ise muaf olabilir, ancak "ana faaliyeti hassas veri işleme" değilsen genelde muafsın. Mali müşavire teyit ettir.
- **Gizlilik politikası, Kullanım şartları, Çerez politikası** (İngilizce + Türkçe).
- **Veri silme**: hesap sil → Stalwart principal sil + DB satırları anonimleştir (30 gün içinde).
- **Yasal talep süreci**: `legal@lovemail.cool`, IP hash + zaman damgası saklama süresi 1 yıl (5651 sayılı kanun trafik kaydı 1-2 yıl).
- **DMCA/istismar**: 24 saat içinde içerik kaldırma akışı.
- Faz 3 ödeme için Lemon Squeezy/Paddle Merchant of Record olduğundan KDV/VAT onlar üzerinden.

---

## 9. Monetizasyon (Faz 3)

| Plan | Fiyat | İçerik |
|---|---|---|
| Free | 0 | 1 GB, 5 kimlik, 5 burner, günde 200 mail, lovemail imzası |
| Plus | 2,99 $/ay veya 79 ₺/ay | 15 GB, sınırsız burner, kısa handle önceliği, imza yok, özel kart temaları |
| Pro | 6,99 $/ay | 50 GB, kendi domainini bağla, IMAP dışa aktarma, öncelikli destek |

Kullanıcı başına maliyet: depolama ~0,006 $/GB/ay (B2), SES ~0,003 $/ay. Plus abonesinin brüt marjı %95+.

---

## 10. Zaman Çizelgesi

| Hafta | Çıktı |
|---|---|
| **1 (bu hafta)** | VPS sertleştirme, Docker, Stalwart + Caddy ayakta, `lovemail.cool` DNS tam, SES hesabı ve production erişim talebi, mail-tester 10/10, SnappyMail çalışıyor |
| **2** | Kalan 4 domain Stalwart + SES'te, DKIM/DMARC/MTA-STS hepsi PASS, yedek cron, Uptime Kuma |
| **3-4** | Next.js: kayıt, handle rezervasyonu, Turnstile, Stalwart sağlama, 5 kimlik, burner alias, ayarlar, hesap silme |
| **5** | Karşılıklı itiraf + "lovemail'de olmayanlara" davet maili, SES webhook |
| **6** | Herkese açık sayfa, moderasyon, raporlama, engelleme, OG kart üretimi |
| **7** | Zaman kapsülü, davet sistemi, kısa handle, admin paneli |
| **8** | Kapalı beta (200 kişi), DMARC `p=quarantine`, hata düzeltme, landing sayfası, App Store'suz PWA |
| **8** | (ek) PWA: ana ekrana ekle + Web Push → mağaza öncesi mobil deneyim |
| **9-12** | Monorepo: ortak `jmap-client` + `ui` paketleri; web JMAP istemcisi (inbox, compose, kimlik seçici, emoji tepki) ve **Expo mobil uygulama MVP** paralel geliştirilir; Plus planı + Lemon Squeezy |
| **13** | TestFlight + Google Internal Testing (100 beta), push bildirimleri, Universal Links, kart paylaşımı testi |
| **14** | App Store + Google Play gönderimi (red payı için 3 hafta önce), mağaza görselleri, Data Safety formu |
| **15-16** | Mağaza geri bildirimi düzeltmeleri, ASO (anahtar kelimeler: anonim mesaj, itiraf, e-posta), yumuşak lansman TR |
| **→ 14 Şubat** | Web + iOS + Android aynı anda: "Crush'ına itiraf et, karşılıklıysa ikiniz de öğrenin" |

---

## 11. Kontrol Listeleri

### Lansman öncesi teknik
- [ ] 5 domainde SPF, DKIM (Stalwart + SES), DMARC PASS
- [ ] PTR = `mail.lovemail.cool`
- [ ] MTA-STS `mode: enforce`, TLS-RPT raporları geliyor
- [ ] mail-tester ≥ 9,5; Gmail, Outlook, Yahoo gelen kutusuna düşüyor (spam değil)
- [ ] `postmaster@`, `abuse@`, `dmarc@` her domainde çalışıyor
- [ ] SES production, bounce/complaint webhook test edildi
- [ ] Yedek geri yükleme bir kez **gerçekten** denendi
- [ ] Admin panel IP kısıtlı, SSH anahtarlı, ufw aktif
- [ ] Hız sınırları ve yeni hesap kotaları aktif
- [ ] Hesap silme uçtan uca çalışıyor

### Lansman öncesi ürün ve hukuk
- [ ] KVKK aydınlatma, gizlilik, kullanım şartları yayında
- [ ] Raporla / engelle / sessize al her anonim yüzeyde
- [ ] Moderasyon eşikleri test edildi (küfür, tehdit, cinsel içerik)
- [ ] Yaş beyanı 16+
- [ ] Paylaşım kartları Instagram story (1080×1920) ve X (1200×675) boyutlarında

### Mobil mağaza gönderimi öncesi
- [ ] Apple Developer ve Google Play hesapları açık (Apple onayı 1-2 gün sürebilir, erken aç)
- [ ] Uygulama içi hesap silme çalışıyor
- [ ] Raporla / engelle her anonim yüzeyde, 24 saat kaldırma taahhüdü mağaza metninde
- [ ] Rehber izni gerekçesi ve Data Safety formu doğru
- [ ] Push bildirimleri iOS + Android gerçek cihazda test edildi
- [ ] Universal Links (`lovemail.live/u/<handle>`) uygulamayı açıyor
- [ ] Mağaza görselleri 5 domain tonuyla, 17+ derecelendirme kabul edildi
- [ ] TestFlight / Internal Testing'de en az 100 kullanıcı, çökme oranı < %1

---

## 12. Maliyet Özeti (ilk 6 ay, aylık)

| Kalem | Tutar |
|---|---|
| VPS (mevcut) | zaten var |
| Amazon SES | 0 $ başlangıç; 100 bin mail/ay = 10 $ |
| Backblaze B2 yedek | 0-2 $ |
| Cloudflare (DNS, Turnstile, proxy) | 0 $ |
| Stalwart, Caddy, Postgres, SnappyMail, Uptime Kuma | 0 $ |
| Moderation API | 0 $ |
| Domain yenilemeleri (5 adet) | ~8-12 $ (yıllığı 12'ye bölünmüş) |
| Apple Developer (99 $/yıl) + Google Play (25 $ tek sefer) | ~10 $ (aylığa bölünmüş) |
| Expo / EAS, PostHog (self-host), Expo Push | 0 $ |
| **Toplam** | **~20-35 $/ay + VPS** |

---

## 13. Hemen Şimdi Yapılacak İlk 10 Adım

1. Başka bir makineden `nc -zv <VPS_IP> 25` → açık değilse sağlayıcıya ticket.
2. VPS panelinden PTR'ı `mail.lovemail.cool` yap.
3. Cloudflare: 5 zone'da Email Routing kapat; `mail.lovemail.cool` A kaydını **gri bulut** ekle.
4. Bölüm 2.2-2.4: sertleştirme + Docker.
5. Bölüm 4.1-4.3: compose dosyalarını oluştur, `docker compose up -d`.
6. Admin panelden hostname, ACME (Cloudflare DNS-01), `lovemail.cool` domaini + DKIM.
7. Bölüm 3.3 DNS kayıtlarını `lovemail.cool` için gir (DKIM dahil).
8. AWS SES: hesap aç, `lovemail.cool` doğrula, production erişim iste, SMTP credential al, Stalwart relay'e gir.
9. `test@lovemail.cool` oluştur, Gmail'e mail at, SPF/DKIM/DMARC PASS gör, mail-tester skoru al.
10. Kalan 4 domaini aynı şekilde ekle. Hafta 1 tamam.
