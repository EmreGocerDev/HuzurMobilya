# Huzur Mobilya

![Huzur Mobilya Logo](HuzurMobilya/logo/logo.png)

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
