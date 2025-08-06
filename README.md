# Cognify Commerce

**Gemini AI ile Güçlendirilmiş Akıllı E-Ticaret Platformu**

[![.NET 8](https://img.shields.io/badge/.NET-8-blueviolet.svg)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![Angular](https://img.shields.io/badge/Angular-DD0031?logo=angular&logoColor=white)](https://angular.io/)
[![Google Gemini](https://img.shields.io/badge/Google-Gemini_AI-4285F4?logo=google&logoColor=white)](https://ai.google.dev/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 🌐 Canlı Demo

**Projenin yeteneklerini canlı olarak deneyimlemek için aşağıdaki bağlantıya tıklayabilirsiniz:**

### **[➡️ Canlı Demoyu Görüntüle](https://ui.onurbahtiyar.dev/presentation)**

---

## 🚀 Proje Hakkında

**Cognify Commerce**, e-ticareti yeniden şekillendirmek amacıyla geliştirilmiş, Google'ın gelişmiş Gemini yapay zeka servisleri ile güçlendirilmiş akıllı bir platformdur. Bu proje, .NET 8 ve Katmanlı Mimari'nin sağlam temelleri üzerinde yükselirken, dinamik ve modern kullanıcı arayüzünü Angular ile sunar. Cloudflare ile korunan altyapısı, global ölçekte yüksek performans ve güvenlik sağlar.

Platformun en devrimci özelliği, kullanıcıların veri tabanıyla doğal dilde sohbet ederek anlık ve akıllı çıktılar alabilmesidir. "Geçen ayki kâr-zarar durumunu grafik olarak göster" gibi bir komut, Gemini entegrasyonu sayesinde anında işlenerek karmaşık veri analizini herkes için erişilebilir kılar.

## ✨ Öne Çıkan Özellikler

### 🤖 Gemini Destekli Doğal Dil Sorgulama
Kullanıcıların teknik bilgiye ihtiyaç duymadan, konuşarak veri tabanından grafik ve tablo formatında raporlar almasını sağlar.

### ⚙️ AI ile Operasyonel Otomasyon
- **Otomatik Yanıt:** Müşteri yorumlarına otomatik ve bağlama uygun yanıtlar oluşturur.
- **Görsel Optimizasyonu:** Ürün görsellerini e-ticaret standartlarına uygun hale getirir.
- **Dinamik İndirim:** Satılmayan ürünler için otomatik indirim kampanyaları planlar ve uygular.

### 📊 Akıllı Analiz ve Öngörü
- **Fiyat Analisti:** Rakip platformlardaki fiyatları anlık analiz ederek rekabetçi fiyatlandırma önerileri sunar.
- **Proaktif Tedarik Tespiti:** Satış hızına göre stok erime süresini hesaplayarak proaktif tedarik uyarısı üretir.
- **Kök Neden Analizi:** Olumsuz müşteri yorumlarını analiz ederek işletme süreçlerindeki temel sorunları tespit eder.

## 🛠️ Kullanılan Teknolojiler

- **Backend:** .NET 8, ASP.NET Core Web API, Entity Framework Core
- **Frontend:** Angular
- **AI Servisleri:** Google Gemini
- **Mimari:** Katmanlı Mimari (Onion Architecture)
- **Veritabanı:** SQL Server (veya herhangi bir EF Core destekli veritabanı)
- **Güvenlik:** JWT (JSON Web Tokens), AES Şifreleme
- **Altyapı:** Cloudflare

## 🏁 Kurulum ve Başlangıç

Bu bölüm, projeyi yerel makinenizde kurmanız ve çalıştırmanız için gereken adımları içerir.

### Gereksinimler
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js ve npm](https://nodejs.org/en/)
- [Angular CLI](https://angular.io/cli): `npm install -g @angular/cli`
- Bir SQL Server veritabanı veya EF Core uyumlu başka bir veritabanı.

### 1. Backend Kurulumu (.NET 8 API)

Backend'in kurulumu, hassas verilerin otomatik olarak şifrelenmesi için özel bir araç olan **StarkPortal** projesi ile başlar.

1.  **Repo'yu Klonlayın:**
    ```bash
    git clone https://github.com/kullanici/cognify-commerce.git
    cd cognify-commerce/backend
    ```

2.  **Otomatik Şifreleme ve Konfigürasyon (`StarkPortal`):**
    -   Solution dosyasını Visual Studio'da açın.
    -   `StarkPortal` projesini **Başlangıç Projesi (Startup Project)** olarak ayarlayın ve çalıştırın.
    -   Konsol uygulaması sizden **veritabanı bağlantı dizenizi (Connection String)** girmenizi isteyecektir. Ham (şifrelenmemiş) bağlantı dizenizi buraya yapıştırıp Enter'a basın.
    -   `StarkPortal`, bu girdiyi kullanarak `appsettings.json` dosyasındaki `ConnectionStrings`, `JwtSettings` ve `AesSettings` alanlarını **otomatik olarak şifreleyerek güncelleyecektir.**

3.  **Manuel Konfigürasyon (`appsettings.json`):**
    -   `WebApi` projesindeki `appsettings.json` dosyasını açın.
    -   **Gemini API Anahtarını Girin:** Kendi Google Gemini API anahtarınızı `Gemini:ApiKey` alanına girin.
        ```json
        "Gemini": {
          "ApiKey": "BURAYA_KENDI_GEMINI_API_ANAHTARINIZI_GIRIN"
        }
        ```
    -   **CORS Ayarlarını Yapılandırın:** Frontend uygulamanızın çalışacağı URL'leri `CorsOrigins` dizisine ekleyin.
        ```json
        "CorsOrigins": [
          "https://sizin-frontend-adresiniz.com",
          "http://localhost:4200"
        ]
        ```

4.  **Veritabanını Oluşturma:**
    -   Şimdi, `WebApi` projesini **Başlangıç Projesi** olarak ayarlayın.
    -   Projeyi çalıştırın (F5 veya `dotnet run`).
    -   Entity Framework Core migrations, uygulama ilk kez başladığında **otomatik olarak veritabanı tablolarını oluşturacak** ve gerekli başlangıç verilerini (seed data) ekleyecektir.

### 2. Frontend Kurulumu (Angular)

1.  **Dizine Gidin ve Paketleri Yükleyin:**
    ```bash
    cd ../frontend 
    npm install
    ```

2.  **Environment Dosyasını Yapılandırın:**
    -   `src/environments/environment.ts` (geliştirme için) ve `src/environments/environment.prod.ts` (canlı ortam için) dosyalarını açın.
    -   `apiUrl` değişkenini, çalışan backend API'nizin adresine göre güncelleyin.
        ```typescript
        // src/environments/environment.prod.ts
        export const environment = {
          production: true,
          apiUrl: 'https://api.sizin-domaininiz.com' // Backend API adresiniz
        };
        ```

3.  **Geliştirme Sunucusunu Başlatın:**
    ```bash
    ng serve
    ```
    Uygulama artık `http://localhost:4200/` adresinde çalışıyor olacaktır.

## 🚀 Canlı Ortama Taşıma (Deployment)

### Backend Dağıtımı

`WebApi` katmanını canlı sunucuda yayınlamak için aşağıdaki .NET CLI komutunu kullanın. Bu komut, projeyi `Release` modunda derler ve belirtilen klasöre bağımlılıklarıyla birlikte yayınlar.

```bash
# Projenizin ana dizinindeyken bu komutu çalıştırın
dotnet publish src/WebApi/WebApi.csproj -c Release -o ./publish/backend
```

-   `-c Release`: Projeyi canlı ortam için optimize edilmiş şekilde derler.
-   `-o ./publish/backend`: Çıktı dosyalarının oluşturulacağı klasörü belirtir.

Bu komut sonrası oluşan `./publish/backend` klasörünün içeriğini sunucunuza yükleyip bir web sunucusu (IIS, Nginx, Apache) arkasında çalıştırabilirsiniz.

### Frontend Dağıtımı

Angular projesini canlı ortam için derlemek ve sunucuya yüklemek için:

1.  **Build Alın:**
    ```bash
    # Frontend projesinin ana dizinindeyken bu komutu çalıştırın
    ng build --configuration production --output-hashing=all
    ```

2.  **Dosyaları Sunucuya Yükleyin:**
    -   Build işlemi tamamlandığında, projenizin altında `dist/cognify-commerce-frontend` (proje isminize göre değişebilir) adında bir klasör oluşacaktır.
    -   Bu klasörün **içerisindeki tüm dosyaları** web sunucunuzun ilgili kök dizinine yükleyin.
