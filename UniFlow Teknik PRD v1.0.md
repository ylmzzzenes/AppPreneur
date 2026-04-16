# UniFlow: Yapay Zeka Destekli Akademik İşletim Sistemi \- Teknik PRD (v1.0)

## 1\. Ürün Vizyonu ve Stratejik Hedefler

**UniFlow**, öğrencilerin akademik hayatındaki karmaşayı (syllabus, sınavlar, ödevler) yapay zeka ile otomatize eden ve "Sarkastik Dahi" personası ile disiplin sağlayan bir mobil akademik işletim sistemidir.

* **Kuzey Yıldızı Metriği:** Sistem tarafından başarıyla planlanan ve kullanıcı tarafından tamamlanan toplam görev sayısı.  
* **Viralite Metriği:** Kullanıcı başına paylaşılan "AI Roast" (Yapay Zeka Eleştirisi) ekran görüntüsü oranı.

## 2\. Teknoloji Yığını (Tech Stack) & Mimari

Ürün, yüksek ölçeklenebilirlik ve performans için **N-Tier (Çok Katmanlı) Mimari** üzerine inşa edilecektir.

| Bileşen | Teknoloji | Görevi |
| :---- | :---- | :---- |
| **Mobil Frontend** | .NET MAUI (C\#) | iOS ve Android için cross-platform native performans. |
| **Backend API** | ASP.NET Core 8.0 (Minimal APIs) | İş mantığı, AI entegrasyonu ve veri yönetimi. |
| **Yapay Zeka** | Gemini 1.5 Pro / GPT-4o Vision | Syllabus analizi ve sarkastik içerik üretimi. |
| **Veritabanı** | PostgreSQL | İlişkisel akademik verilerin ve kullanıcı profillerinin saklanması. |
| **Yerel Veritabanı** | SQLite | Offline erişim ve cihaz içi hızlı veri senkronizasyonu. |

## 3\. Veritabanı Şeması (PostgreSQL)

### 3.1. Ana Tablolar ve İlişkiler

| Tablo Adı | Sütun | Veri Tipi | Kısıtlamalar | Açıklama |
| :---- | :---- | :---- | :---- | :---- |
| **users** | id | UUID | Primary Key | Kullanıcı eşsiz kimliği. |
|  | username | VARCHAR | Unique, Not Null | Kullanıcı adı. |
|  | personality\_vibe | INT | Default: 5 | "Dahi"nin sertlik seviyesi (1-10). |
|  | created\_at | TIMESTAMPTZ | Default: NOW() | Kayıt tarihi. |
| **courses** | id | UUID | Primary Key | Ders kimliği. |
|  | user\_id | UUID | Foreign Key | Hangi kullanıcıya ait olduğu. |
|  | name | VARCHAR | Not Null | Dersin adı (Örn: Kalkülüs). |
| **tasks** | id | UUID | Primary Key | Görev kimliği. |
|  | course\_id | UUID | Foreign Key | Bağlı olduğu ders. |
|  | type | VARCHAR | Exam, Assignment, Reading | Görev tipi. |
|  | deadline | TIMESTAMPTZ | Not Null | Teslim veya sınav tarihi. |
|  | status | VARCHAR | Pending, Done, Missed | Görev durumu. |
| **daily\_logs** | id | SERIAL | Primary Key | Günlük kayıt ID. |
|  | user\_id | UUID | Foreign Key | İlgili kullanıcı. |
|  | ai\_message | TEXT | Nullable | Dahinin o günkü özel mesajı. |

## 4\. API Endpoint Dokümantasyonu

### 4.1. Kimlik Doğrulama ve Profil

* **POST `/api/v1/auth/setup`**  
  * **Payload:** `{ "username": "string", "vibe": 7, "major": "string" }`  
  * **İşlem:** Kullanıcı profili ve "Dahi" personası oluşturulur.

### 4.2. Syllabus Analiz Motoru

* **POST `/api/v1/syllabus/scan`**  
  * **Payload:** `{ "image_base64": "string", "user_id": "uuid" }`  
  * **İşlem:** Görüntü AI Vision modeline gönderilir, akademik takvim çıkarılır ve `tasks` tablosuna işlenir.

### 4.3. Dashboard ve Görev Yönetimi

* **GET `/api/v1/dashboard/today`**  
  * **İşlem:** Bugünün en kritik görevlerini ve avatarın güncel durumunu döner.  
* **PATCH `/api/v1/tasks/{id}/status`**  
  * **Payload:** `{ "status": "Done" }`  
  * **İşlem:** Görev durumunu günceller ve puan sistemini tetikler.

## 5\. UI/UX Akışı ve Ekran Tasarımları

### 5.1. Onboarding & Persona Seçimi

Kullanıcı 3 farklı "Sarkastik Dahi" tipinden birini seçer. Seçim anında haptic feedback (titreşim) ve karakterin ilk "roast" cümlesi görünür.

### 5.2. Scan-to-Plan (Sihirli An)

* **Kamera Vizörü:** Belge sınırlarını otomatik algılayan native overlay.  
* **Analiz Animasyonu:** Avatarın belgeyi incelerken göz devirdiği veya şaşırdığı interaktif yükleme ekranı.  
* **Hızlı Onay:** AI tarafından çıkarılan listenin kullanıcı tarafından saniyeler içinde doğrulandığı özet ekranı.

### 5.3. Reality Dashboard

* **Avatar Widget:** Ekranın üst kısmında performansa göre (Mutlu, Kızgın, Alaycı) değişen animasyonlu karakter.  
* **The Big 3 Kartları:** O gün bitirilmesi gereken en önemli 3 işin odaklandığı minimalist liste.

## 6\. Kullanıcı Hikayeleri ve Kabul Kriterleri (User Stories)

### EPIC 1: Akıllı Planlama

* **US 1.1:** Bir öğrenci olarak, karmaşık syllabus belgelerimi saniyeler içinde takvime dönüştürmek istiyorum.  
* **Kabul Kriterleri:** AI Vision motoru tarihleri ve görev tiplerini en az %90 doğrulukla ayıklamalıdır.

### EPIC 2: Dahi Etkileşimi

* **US 2.1:** Ertelemeye meyilli bir öğrenci olarak, AI'nın beni dürüst bir dille uyarmasını istiyorum.  
* **Kabul Kriterleri:** Görev kaçırıldığında sistem, kullanıcının geçmiş performansına dayalı sarkastik bir bildirim (push notification) tetiklemelidir.

## 7\. Kapsam Dışı (Out of Scope \- Faz 1\)

* İkinci ebeveyn/arkadaş hesap eşleştirmesi.  
* Gelişmiş PDF düzenleme özellikleri.  
* Uygulama içi ödeme sistemleri (V1 tamamen ücretsiz veya tekil satın alma odaklı).

## 8\. Teknik Riskler ve Çözüm Planları

* **AI Halüsinasyonu:** Kullanıcıya her zaman "Onay Ekranı" sunularak verinin doğruluğu son aşamada insana bırakılır.  
* **Offline Senkronizasyon:** .NET MAUI Connectivity API kullanılarak veriler yerel SQLite'da tutulur ve internet geldiğinde asenkron olarak buluta yedeklenir.

