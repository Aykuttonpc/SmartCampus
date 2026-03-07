# SmartCampus – Kapsamlı Sistem Dokümantasyonu

> **Hedef kitle:** Sistemi hiç görmemiş, teknik veya teknik olmayan herkes  
> **Uygulama:** ASP.NET Core MVC · SQL Server · Localhost:5000

---

## 1. Sistem Nedir?

**SmartCampus**, bir üniversite kampüsündeki mekanların (kütüphane, laboratuvar, spor salonu, toplantı odası vb.) **gerçek zamanlı doluluk durumunu** izlemek ve **rezervasyon yönetimini** merkezi olarak yapmak için tasarlanmış bir web uygulamasıdır.

### Temel Özellikler

| Özellik | Açıklama |
|---------|----------|
| 🏠 Dashboard | Kampüsteki tüm mekanların anlık doluluk yüzdelerini kart görünümünde gösterir |
| 📅 Takvim | Bugünkü tüm rezervasyonları liste halinde görüntüler |
| 🕐 Rezervasyon | Seçilen mekan ve tarihe göre boş saatleri listeler, tek tıkla yer ayırtılır |
| 👤 Profil | Kişisel rezervasyon geçmişi, katılım oranı ve aktif ceza puanları |
| ⚙️ Admin Paneli | No-show raporu ve mekan yönetimi (sadece Admin rolü) |

---

## 2. Kullanıcı Rolleri

Sistemde **üç farklı rol** bulunmaktadır:

### 👨‍🎓 Student (Öğrenci) — Varsayılan rol

**Yapabilecekleri:**
- Dashboard'u görüntüleme (herkese açık)
- Rezervasyon yapma (boş slot seçme)
- **Kendi** rezervasyonlarını iptal etme (Takvim veya Profil sayfasından)
- Profil istatistiklerini görme

**Yapamayacakları:**
- Başka birinin rezervasyonunu iptal etme (backend engeller)
- Rezervasyon onaylama (`Onayla` butonu görünmez)
- Admin paneline erişim (`/Admin` → Login sayfasına atar)

---

### 👨‍💼 Staff (Personel)

Student yetkilerine ek olarak:

**Yapabilecekleri:**
- **Herhangi** bir rezervasyonu iptal etme (Takvim sayfasında tüm `İptal` butonları görünür)
- **Pending** durumundaki rezervasyonları **Onaylama** (`Onayla` butonu görünür)

**Yapamayacakları:**
- Admin paneline erişim (`/Admin` → Login sayfasına atar)
- Mekan ekleme/silme

---

### 🔑 Admin (Yönetici)

Tüm yetkilere sahiptir:

**Yapabilecekleri:**
- Staff'ın tüm yetkileri
- `/Admin` sayfası: No-Show raporu (en çok rezervasyonu iptal eden kullanıcılar)
- `/Admin/Facilities` sayfası: Mekan listesi, mekan silme

---

## 3. Giriş Yapma

**URL:** `http://localhost:5000`  
**Sayfa:** `/Account/Login`

| Alan | Açıklama |
|------|----------|
| E-posta | Üniversite email adresi (örn: `kaan1@uni.edu`) |
| Şifre | Kullanıcı şifresi |

> 💡 **Seed data şifreleri:** Veritabanında test verileri oluştururken tüm kullanıcılara `hash` şifre atanmıştır (literal `hash` kelimesi).

**Giriş başarısız olursa:** "E-posta veya şifre hatalı." mesajı gösterilir. Sayfa değişmez.

**Güvenlik:**
- Boş alan gönderimi HTML5 validation ile engellenir
- SQL injection denemeleri EF Core parametrize sorguları sayesinde etkisizleşir
- 300 karakterlik girişler veritabanı sütun uzunluğu sınırında reddedilir

---

## 4. Dashboard (Ana Sayfa)

**URL:** `/Home/Index` (varsayılan sayfa)  
**Erişim:** Giriş gerekmez (herkese açık)

Kampüsteki her mekan için şu bilgiler gösterilir:
- **Mekan adı ve tipi** (Lounge, Library, Lab vb.)
- **Anlık kişi sayısı / Kapasite** (örn: `47 / 79 kişi`)
- **Doluluk yüzdesi** ve renkli progress bar
- **Kalabalık seviyesi:** Empty / Low / Moderate / High / Over Capacity
- **Son güncelleme zamanı** (sensör verisi)
- `Rezervasyon Yap` ve `Alternatif Bul` butonları

---

## 5. Takvim (Bugünkü Rezervasyonlar)

**URL:** `/Reservations`  
**Erişim:** 🔒 Giriş gerektirir

Bugün için tüm aktif rezervasyonları tablo halinde gösterir:

| Sütun | Açıklama |
|-------|----------|
| # | Rezervasyon ID |
| Mekan | Yer adı |
| Organizatör | Rezervasyonu yapan kişi |
| Başlangıç / Bitiş | Saatler |
| Süre | Dakika cinsinden |
| Katılımcı | Davetli kişi sayısı |
| Durum | Pending / Approved / Cancelled / Completed |
| İşlem | Rol bazlı butonlar |

### Durum Renkleri

| Durum | Renk | Anlam |
|-------|------|-------|
| `Pending` | 🟡 Sarı | Onay bekliyor |
| `Approved` | 🟢 Yeşil | Onaylandı |
| `Cancelled` | 🔴 Kırmızı | İptal edildi |
| `Completed` | ⚫ Gri | Tamamlandı |

### Buton Görünürlüğü (Rol Bazlı)

| | Onayla | İptal |
|---|--------|-------|
| **Student** | ❌ | ✅ Sadece kendi rezervasyonları |
| **Staff** | ✅ Pending olanlar | ✅ Tüm rezervasyonlar |
| **Admin** | ✅ Pending olanlar | ✅ Tüm rezervasyonlar |

---

## 6. Rezervasyon Yapma

### Adım 1: Dashboard'dan Mekan Seçimi
Dashboard'da herhangi bir mekan kartındaki **`Rezervasyon Yap`** butonuna tıklanır.

### Adım 2: Boş Zaman Dilimlerini Görüntüleme
**URL:** `/Reservations/Book?facilityId=X&date=YYYY-MM-DD`

- Sistem, seçilen mekan ve tarihe göre **08:00–22:00 arası boş 1 saatlik dilimleri** listeler
- Başlık: `Mekan: [Mekan Adı] · Tarih: [Tarih]`
- Her slot: `08:00 – 09:00 ✓ Uygun` şeklinde gösterilir

> ⚠️ **Kural:** Geçmiş tarih girilirse otomatik olarak bugüne yönlendirilir.  
> ⚠️ **Kural:** Geçersiz mekan ID girilirse `404 Not Found` döner.

### Adım 3: Slot Seçimi
İstenen saat dilimine tıklanır. POST isteği `/Reservations/Confirm`'e gider.

### Adım 4: Sonuç
- **Başarılı:** Takvim sayfasına (`/Reservations`) yönlendirilir, rezervasyon `Pending` statüsüyle görünür
- **Başarısız:** Hata mesajı gösterilir (örn: çakışma, askıya alınmış kullanıcı)

---

## 7. Rezervasyon İptal Etme

### Takvim Sayfasından (Sadece kendi veya Staff/Admin)
1. `/Reservations` → İlgili satırdaki `İptal` butonuna tıkla
2. `"İptal edilsin mi?"` onay iletişim kutusu çıkar
3. `OK` → Rezervasyon `Cancelled` statüsüne geçer, sayfa yenilenir

### Profil Sayfasından
1. `/Account/Profile` → `Yaklaşan Rezervasyonlarım` tablosundaki `İptal` butonu
2. Onay iletişim kutusu
3. `OK` → İptal işlemi gerçekleşir, **profil sayfasına geri döner** (Takvime değil)

---

## 8. Alternatif Mekan Bulma

**URL:** `/Reservations/Alternatives`  
**Erişim:** 🔒 Giriş gerektirir

Dashboard'daki **`Alternatif Bul`** butonu, aynı tipteki en boş diğer mekanları listeler.

---

## 9. Profil Sayfası

**URL:** `/Account/Profile`  
**Erişim:** 🔒 Giriş gerektirir

Üç istatistik kartı gösterilir:

| Kart | Açıklama |
|------|----------|
| Toplam Rezervasyon | Yapılan tüm rezervasyon sayısı |
| Katılım Oranı | Rezervasyonlara gerçekte katılım yüzdesi |
| Aktif Ceza Puanı | Geç iptal veya no-show ceza puanları |

**Yaklaşan Rezervasyonlarım:** Gelecekteki Pending/Approved rezervasyonlar listelenir. Her biri için İptal butonu mevcuttur.

**Şu An En Boş Mekanlar:** Kampüsteki en düşük doluluk oranına sahip 2 mekan anlık olarak gösterilir.

---

## 10. Admin Paneli

**URL:** `/Admin`  
**Erişim:** 🔒 Yalnızca Admin rolü (diğerleri Login'e yönlendirilir)

### No-Show Raporu
Sistemdeki kullanıcıların rezervasyon katılım/devamsızlık istatistiklerini gösterir:

| Sütun | Açıklama |
|-------|----------|
| Kullanıcı | Ad Soyad |
| Toplam Rez. | Toplam rezervasyon sayısı |
| Katıldı | Gerçekten gelen sayısı |
| Kaçırdı | Gelmeden geçen sayısı |
| No-Show Oranı | Devamsızlık yüzdesi |
| Risk | Good Standing / Moderate Risk / High Risk |

### Mekan Yönetimi

**URL:** `/Admin/Facilities`

Tüm kampüs mekanlarını listeler. Her mekan için:
- **Sil** butonu: Mekanı veritabanından kaldırır
- FK kısıtı varsa (aktif rezervasyonlar) silme işlemi başarısız olabilir

---

## 11. Güvenlik Mekanizmaları

### Kimlik Doğrulama (Authentication)
- **Oturum bazlı** (`Session`): Login sonrası sunucu tarafında session oluşturulur
- Korunan sayfalar (`/Reservations`, `/Admin` vb.) session yoksa `/Account/Login`'e yönlendirir

### Yetkilendirme (Authorization)

| Güvenlik Katmanı | Koruduğu Şey |
|-----------------|--------------|
| Session kontrolü | Giriş yapmadan sayfa erişimi |
| Rol kontrolü | Admin/Staff gerektiren işlemler |
| Sahiplik filtresi | Başkasının rezervasyonunu iptal etme (IDOR koruması) |

### IDOR Koruması
Bir öğrenci kendi rezervasyonu dışındaki bir rezervasyonu iptal etmeye çalışırsa backend EF Core sorgusu `OrganizerUserID == userId` koşulu nedeniyle eşleşme bulamaz; işlem sessizce başarısız olur, hata fırlatmaz.

---

## 12. Veritabanı Mimarisi

```
Roles ──────────────── UserRoles ─────────── Users
                                               │
FacilityTypes ─── Facilities ─── Sensors ─── OccupancyLogs
                       │
                  Reservations ──── Statuses
                       │
              ReservationAttendees, UserPenalties, ReservationAuditLogs
```

### Önemli Tablolar

| Tablo | Açıklama |
|-------|----------|
| `Users` | Kullanıcılar (auto `FullName` computed column) |
| `Roles` | Student, Staff, Admin |
| `Facilities` | Rezervasyon yapılabilen mekanlar |
| `Reservations` | Tüm rezervasyon kayıtları |
| `Statuses` | Pending, Approved, Cancelled, Completed |
| `OccupancyLogs` | Sensör doluluk verileri (big data, BIGINT) |

### Önemli View'lar (SQL)

| View | Açıklama |
|------|----------|
| `vw_LiveOccupancy` | Her mekan için en son sensör verisi + doluluk yüzdesi |
| `vw_TodayReservations` | Bugünün rezervasyonları (organizatör, durum, OrganizerUserID dahil) |

### Önemli Stored Procedure'lar

| SP | Kullanım |
|----|---------|
| `sp_FindFreeSlots` | Bir mekanın gün içindeki boş 1 saatlik dilimler |
| `sp_GetUserDashboard` | Kullanıcı profil verilerini tek sorguda getirir |
| `sp_SuggestAlternatives` | Aynı tipteki alternatif mekanları önerir |
| `sp_GetNoShowReport` | Devamsızlık raporu (Admin için) |

---

## 13. URL Referansı

| URL | Yöntem | Açıklama | Yetki |
|-----|--------|----------|-------|
| `/` | GET | Dashboard | Herkese açık |
| `/Account/Login` | GET/POST | Giriş | Herkese açık |
| `/Account/Profile` | GET | Profil | 🔒 Giriş gerekli |
| `/Account/Logout` | POST | Çıkış | 🔒 Giriş gerekli |
| `/Reservations` | GET | Bugünkü takvim | 🔒 Giriş gerekli |
| `/Reservations/Book` | GET | Slot listesi | 🔒 Giriş gerekli |
| `/Reservations/Confirm` | POST | Rezervasyon oluştur | 🔒 Giriş gerekli |
| `/Reservations/Cancel/{id}` | POST | İptal et | 🔒 Sahip veya Staff/Admin |
| `/Reservations/Approve/{id}` | POST | Onayla | 🔒 Staff/Admin |
| `/Reservations/Alternatives` | GET | Alternatif mekanlar | 🔒 Giriş gerekli |
| `/Admin` | GET | No-show raporu | 🔒 Admin |
| `/Admin/Facilities` | GET | Mekan listesi | 🔒 Admin |
| `/Admin/DeleteFacility/{id}` | POST | Mekan sil | 🔒 Admin |

---

## 14. QA Test Özeti — Bulunan ve Düzeltilen Buglar

Tüm testler `http://localhost:5000` üzerinde canlı uygulama ile yapılmıştır.

### Düzeltilen Buglar

| Bug ID | Kategori | Sorun | Çözüm |
|--------|----------|-------|-------|
| BUG-1 | 🔴 Güvenlik | IDOR: Herkes herkesin rezervasyonunu iptal edebiliyordu | `CancelAsync`'e `userId` ownership filtresi eklendi |
| BUG-2 | 🟡 Erişim | Giriş yapmadan `/Reservations`, `/Book`, `/Alternatives`'e erişiliyordu | Auth guard eklendi (session check) |
| BUG-3 | 🟡 UX | Profilden iptal → Takvime atıyordu | `returnUrl` parametresi ile profil sayfasına geri dönüş |
| BUG-4 | 🟢 UI | Book sayfası mekan adı yerine `ID: 1` gösteriyordu | `GetFacilityNameAsync` + `ViewBag.FacilityName` |
| BUG-5 | 🟢 UI (Kaldırıldı) | Admin mekan silerken FK hatası ekrana geliyordu | Kullanıcı isteğiyle revert edildi |
| BUG-6 | 🟢 UI (Kaldırıldı) | `FirstAsync` null reference riski | Kullanıcı isteğiyle revert edildi |
| BUG-7 | 🟡 UI | Takvimde diğer kullanıcıların rezervasyonlarında da İptal butonu görünüyordu | `vw_TodayReservations`'a `OrganizerUserID` eklendi, Index.cshtml'de sahiplik kontrolü |
| EC4 | 🟡 UX | Geçmiş tarih için URL manuel girilince slot gösteriyordu | Book aksiyonunda `date < today → redirect to today` |
| EC9 | 🔴 Güvenlik | Geçersiz `facilityId` ile Book URL'ye girilince FK hatası ekrana geliyordu | `GetFacilityNameAsync` null dönerse `NotFound()` |

### Geçen Edge Case'ler

| Case | Test | Sonuç |
|------|------|-------|
| EC1 | Login boş alan gönderim | ✅ HTML5 validation engelledi |
| EC2 | SQL injection login | ✅ EF Core parametrize sorgu engelledi |
| EC3 | 300+ karakter girişi | ✅ Graceful reject |
| EC5 | Gelecek tarih rezervasyonu | ✅ Normal slot listesi |
| EC6 | Zaten iptal rezervasyonu tekrar iptal | ✅ Sessizce başarısız (hata yok) |
| EC7 | Geçersiz rezervasyon ID iptal denemesi | ✅ Sessizce başarısız |
| EC8 | Admin geçersiz mekan ID sil | ✅ Graceful redirect |
| EC10 | Alternatifler sayfası boş sonuç | ✅ "Sonuç bulunamadı" gösterildi |
| EC11 | Session cookie silinince erişim | ✅ Login sayfasına yönlendirildi |
| EC12 | Rezervasyonu olmayan kullanıcı profili | ✅ Sıfır istatistikle çalıştı |

---

## 15. Teknik Stack

| Katman | Teknoloji |
|--------|-----------|
| Web Framework | ASP.NET Core MVC (.NET 8) |
| ORM | Entity Framework Core |
| Veritabanı | SQL Server (MONSTER\SQLEXPRESS) |
| Kimlik Doğrulama | Session-based (AddSession middleware) |
| UI | Razor Pages (.cshtml) + Vanilla CSS |
| Port | 5000 (HTTP) |

---

## 16. Sistemi Sıfırdan Kurma

1. SQL Server'ı başlatın
2. `SmartCampusDB.sql` dosyasını SSMS'de çalıştırın (veritabanı + seed data oluşturur)
3. `appsettings.json`'daki connection string'i kontrol edin:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=MONSTER\\SQLEXPRESS;Database=SmartCampusDB;..."
   }
   ```
4. Proje klasöründe çalıştırın:
   ```bash
   dotnet run --project SmartCampus/SmartCampus.csproj
   ```
5. Tarayıcıdan açın: `http://localhost:5000`
6. Giriş: `kaan1@uni.edu` / `hash`

---

*Dokümantasyon SmartCampus QA sürecinde otomatik oluşturulmuştur. Son güncelleme: 2026-03-07*
