# Huzur Mobilya

![Huzur Mobilya Logo](logo/logo.png)

Modern, hafif ve hızlı bir masaüstü stok & satış yönetimi uygulaması — .NET 8 WinForms ile geliştirilmiştir. Bu repo, demo amaçlı yerel bir Supabase backend'i düşünerek tasarlanmıştır (REST + Storage).

Özellikler
- Ürün, stok, sipariş, müşteri, tedarikçi ve personel yönetimi
- Stok hareketleri ve sipariş oluşturma ekranları
- Ürün resim yükleme (Supabase Storage entegrasyonu)
- Hızlı yükleme için paralel HTTP çağrıları (Task.WhenAll)
- Modern özel başlık çubuğu (FormBorderStyle.None) ve logo
- Tek dosya (self-contained) Windows x64 publish desteği

Gereksinimler
- .NET 8 SDK (net8.0-windows)
- Windows 10/11 (GUI uygulaması)
- (Opsiyonel) Supabase projesi — storage ve REST API anahtarları kullanmak istiyorsanız ortam değişkenleri (.env) ayarlanmalıdır.

Hızlı Başlangıç
1. Ortam değişkenleri: .env dosyasını projenin köküne koyun ve Supabase URL / anon key gibi değerleri ekleyin.
2. Derleme (geliştirme):

```
dotnet build HuzurMobilya/HuzurMobilya.csproj
```

3. Uygulamayı çalıştırma (Debug/IDE): Visual Studio veya `dotnet run` ile çalıştırabilirsiniz.

Self-contained single-file publish (Windows x64)

Bu repo için önceden bir single-file publish çıktısı oluşturuldu. Kendiniz tekrar üretmek isterseniz:

```
dotnet publish HuzurMobilya/HuzurMobilya.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o HuzurMobilya/publish/win-x64/release
```

Çıktı: `HuzurMobilya/publish/win-x64/release/HuzurMobilya.exe` (tek .exe, .NET runtime dahil)

Not: Publish sırasında HuzurMobilya.exe çalışıyorsa build dosyaları kilitlenir. Uygulamayı kapatıp yeniden publish edin.

Simge (Icon)
- Projeye logo/logo.ico eklendi ve uygulamaya gömüldü. Masaüstü uygulaması .exe'nin simgesi bu .ico'dan alınmaktadır.
- PNG → ICO dönüştürme aracı: `tools/makeico` içinde basit bir küçük araç bulabilirsiniz.

Veritabanı ve Supabase
- PostgreSQL şeması: `Database/schema.sql` (Postgres sözdizimi içerir)
- Supabase Storage kurulumu: `Database/setup_storage.sql` (product-images bucket + RLS örnekleri)

Dikkat
- Visual Studio'daki SQL linter bazı PostgreSQL sözdizimi öğelerini (ör. CREATE POLICY, PL/pgSQL trigger fonksiyonları) hata olarak gösterebilir — bu sadece linter farkından kaynaklanır, C# derlemesini etkilemez.

Katkıda bulunma
- Yeni özellikler veya hata düzeltmeleri için pull request açın.
- Kod stili: mevcut WinForms düzenine (Form + Theme.cs stilizasyonu) uyun.

Sık Karşılaşılan Sorunlar
- "Kaydet" butonları görünmüyor: WinForms Z-order kuralı nedeniyle düzeltilmiştir (panel Controls.Add önce, header sonra). Eğer benzer UI sorunları görürseniz ilgili Form constructor içindeki Controls.Add sırasını kontrol edin.
- .exe kilitlenmesi: Uygulamayı kapatın veya görev yöneticisinden HuzurMobilya.exe sürecini sonlandırın, sonra tekrar publish/derleyin.

İletişim & Lisans
- README ve proje açıklamaları güncellenebilir; isterseniz ben bir MIT lisans dosyası ekleyebilirim.

Görseller
- Logo: `logo/logo.png` (projede kullanıldı)

Teşekkürler — başka bir belge, kurulum paketi (Inno Setup script) veya GitHub Release notları hazırlamamı ister misiniz?