# 🚀 UniFlow: Ultra-Architected Master Implementation Plan 

Bu doküman, UniFlow (Akademik İşletim Sistemi) projesinin MVP geliştirme yol haritasıdır. Proje, "Önce Backend" (Backend-First) yaklaşımıyla, servisler arası tam izolasyon (Decoupling) prensibine göre kurgulanmıştır.

## 🛠 Teknik Stack
- **Backend:** .NET 8.0, EF Core, SQL Server, N-Tier Architecture.
- **Frontend:** .NET MAUI (MVVM), CommunityToolkit.Mvvm.
- **AI:** Gemini API (Avatar: "Sarkastik Dahi"), Azure AI Document Intelligence (OCR).

---

## 📂 FAZ 1: Workspace Setup & Foundation (Kurulum)
*Amacı: Backend ve Frontend servislerinin birbirinden tamamen izole şekilde başlatılması.*

- [x] **Adım 1.1: Root Orchestration:**
    - `/src/backend` ve `/src/frontend` dizinlerini oluştur.
    - Kök dizine global `.gitignore` ekle.
- [x] **Adım 1.2: Backend Solution Setup:**
    - `UniFlow.sln` oluştur ve 4 katmanı fiziksel projeler olarak ekle:
        - `UniFlow.API` (Web API)
        - `UniFlow.Business` (Logic & AI Services)
        - `UniFlow.DataAccess` (EF Core & DB)
        - `UniFlow.Entity` (Core Domain)
    - Katman referanslarını bağla: `API -> Business -> DataAccess -> Entity`.
- [x] **Adım 1.3: Frontend Shell:**
    - `/src/frontend` içinde `UniFlow.Mobile` (MAUI) projesini başlat.
    - Temel klasör yapısını kur: `Views`, `ViewModels`, `Services`, `Models`.

---

## 🗄 FAZ 2: Backend - Data Sovereignty (Veri Katmanı)
*Amacı: Veri modellerinin ve veritabanı erişim standartlarının inşası.*

- [x] **Adım 2.1: Domain Entities:**
    - `BaseEntity` (Id, CreatedDate, UpdatedDate) oluştur.
    - `User`, `Course`, `Syllabus`, `TaskItem` (Syllabus'tan çıkan her bir görev) sınıflarını yaz.
- [x] **Adım 2.2: Data Access & EF Core:**
    - `UniFlowDbContext` tanımla.
    - `AuditInterceptor` ile `CreatedDate` alanlarını otomatize et.
    - Generic `IRepository<T>` ve `IUnitOfWork` yapılarını kur.
- [x] **Adım 2.3: Result Pattern:**
    - Tüm metodların döneceği `Result<T>` ve `IResult` yapılarını `Entity` veya `Core` katmanına ekle.

---

## 🧠 FAZ 3: Backend - The "Brain" (AI & Logic)
*Amacı: Projenin kalbi olan OCR ve planlama algoritmalarının bitirilmesi.*

- [x] **Adım 3.1: OCR & External Services:**
    - `IOCRService` oluştur ve Azure/AWS/Tesseract entegrasyonunu yap.
    - `IGeminiService` ile Gemini API bağlantısını kur.
- [x] **Adım 3.2: Syllabus Parsing Logic:**
    - OCR metnini alıp Gemini üzerinden `List<TaskItem>` (Vize, Final, Ödev tarihleri) üreten Business logic'i yaz.
- [x] **Adım 3.3: Adaptive Scheduling Algorithm:**
    - Kalan gün, sınav ağırlığı ve zorluk seviyesine göre görevlere 1-100 arası `PriorityScore` veren algoritmayı implemente et.

---

## 🌐 FAZ 4: Backend - API & Hardening (Bitiş)
*Amacı: Backend'i dokümante edilmiş ve test edilmiş bir ürün olarak finalize etmek.*

- [x] **Adım 4.1: Controller Development:**
    - `AuthController`, `SyllabusController`, `TaskController`, `ChatController` endpoint'lerini yaz.
- [x] **Adım 4.2: Security & Validation:**
    - JWT Auth Middleware ekle.
    - `FluentValidation` ile tüm giriş verilerini doğrula.
- [x] **Adım 4.3: Global Exception & Swagger:**
    - Merkezi hata yakalama (Global Exception Middleware) sistemini kur.
    - Swagger UI üzerinden tüm servisleri test et.

---

## 📱 FAZ 5: Frontend - Mobile UI & Integration
*Amacı: Bitmiş backend servislerini şık bir arayüzle kullanıcıya sunmak.*

- [x] **Adım 5.1: API Service Client:**
    - Backend ile JSON üzerinden haberleşen `ApiService` sınıfını yaz.
    - DTO'ları Mobil taraftaki `Models` klasörüne taşı.
- [x] **Adım 5.2: Auth & Dashboard UI:**
    - Login/Register ekranlarını tasarla.
    - Backend'den gelen "Öncelikli Görevler" listesini Dashboard'da göster.
- [x] **Adım 5.3: AI Chat & Camera UX:**
    - "Sarkastik Dahi" için mesajlaşma balonları UI'ını yap.
    - Kamera ile syllabus fotoğrafı çekip API'ye gönderen akışı tamamla.

---

## 🛡 Teknik Standartlar & Kurallar
1. **Separation:** Frontend, Backend projelerine asla referans vermez. İletişim sadece REST API (JSON) ile sağlanır.
2. **DTO Mandatory:** Entity sınıfları asla Controller'dan dışarı sızmaz. Daima DTO kullanılır.
3. **Naming:** Kod içi isimlendirmeler (metot, değişken) İngilizce; UI metinleri ve AI cevapları Türkçe olacaktır.
4. **Error Handling:** Backend asla `null` dönmez, daima `Result` objesi döner.