using System;
using System.Drawing;
using System.Windows.Forms;

namespace HuzurMobilya.Forms
{
    public class HelpPage : UserControl
    {
        public HelpPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var header = Theme.CreateModernHeader("❓", "Yardım Merkezi");

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Padding = new System.Drawing.Point(16, 6)
            };

            tabs.TabPages.Add(MakeTab("🏠 Genel Bakış", HelpGenel()));
            tabs.TabPages.Add(MakeTab("📦 Ürünler & Stok", HelpUrunler()));
            tabs.TabPages.Add(MakeTab("🛒 Siparişler", HelpSiparisler()));
            tabs.TabPages.Add(MakeTab("👥 Müşteriler", HelpMusteriler()));
            tabs.TabPages.Add(MakeTab("👨‍💼 Personel", HelpPersonel()));
            tabs.TabPages.Add(MakeTab("📅 Ajanda", HelpAjanda()));
            tabs.TabPages.Add(MakeTab("⚙️ İpuçları", HelpIpuclari()));

            // Add header first so tabs fill the remaining space and do not get overlapped
            Controls.Add(header);
            Controls.Add(tabs);
        }

        private TabPage MakeTab(string title, string content)
        {
            var tp = new TabPage(title) { BackColor = Theme.Background, Padding = new Padding(0) };
            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10),
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Padding = new Padding(16)
            };
            rtb.Rtf = BuildRtf(content);
            tp.Controls.Add(rtb);
            return tp;
        }

        private string BuildRtf(string text)
        {
            // Basit RTF dönüştürme — ## başlık, ** bold, normal metin
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(@"{\rtf1\ansi\deff0{\fonttbl{\f0 Segoe UI;}}");
            sb.AppendLine(@"{\colortbl;\red17\green24\blue39;\red99\green102\blue241;\red107\green114\blue128;\red16\green185\blue129;\red239\green68\blue68;}");
            sb.AppendLine(@"\f0\fs22\cf1");

            foreach (var rawLine in text.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');
                if (line.StartsWith("## "))
                {
                    sb.Append(@"\fs28\b\cf2 ");
                    sb.Append(EscapeRtf(line.Substring(3)));
                    sb.AppendLine(@"\b0\fs22\cf1\par\par");
                }
                else if (line.StartsWith("### "))
                {
                    sb.Append(@"\fs24\b\cf1 ");
                    sb.Append(EscapeRtf(line.Substring(4)));
                    sb.AppendLine(@"\b0\fs22\par");
                }
                else if (line.StartsWith("• ") || line.StartsWith("- "))
                {
                    sb.Append(@"\li200\bullet\tab ");
                    sb.Append(EscapeRtf(line.Substring(2)));
                    sb.AppendLine(@"\li0\par");
                }
                else if (line.StartsWith("✅ ") || line.StartsWith("⚠️ ") || line.StartsWith("💡 "))
                {
                    sb.Append(@"\cf4\b ");
                    sb.Append(EscapeRtf(line));
                    sb.AppendLine(@"\b0\cf1\par");
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine(@"\par");
                }
                else
                {
                    sb.Append(EscapeRtf(line));
                    sb.AppendLine(@"\par");
                }
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string EscapeRtf(string s)
        {
            return s.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");
        }

        private string HelpGenel() => @"
## Huzur Mobilya - Yönetim Paneli

Bu program, Huzur Mobilya işletmenizin tüm operasyonlarını tek bir yerden yönetmenizi sağlar.

### Programın Bölümleri

• Dashboard - Genel istatistikler ve hızlı özet
• Ürünler - Ürün kataloğu yönetimi
• Stok Takibi - Depo bazlı stok durumu
• Stok Hareketleri - Giriş/çıkış kayıtları
• Müşteriler - Müşteri bilgileri ve geçmiş
• Siparişler - Sipariş oluşturma ve takip
• Personel - Çalışan yönetimi ve izin takibi
• Kategoriler - Ürün kategori yönetimi
• Tedarikçiler - Tedarikçi bilgileri
• Bildirimler - Sistem bildirimleri
• Ajanda - Takvim ve not sistemi
• Yardım - Bu ekran

### Giriş Yapma

- Programı açtığınızda e-posta ve şifrenizi girin.
- İlk kez kullanıyorsanız kayıt olun (Kayıt Ol butonu).
- Şifrenizi güvende tutun, başkasıyla paylaşmayın.

### Genel Kullanım İpuçları

• Tüm listelerde satıra çift tıklayarak detay/düzenleme ekranını açabilirsiniz.
• Tablolarda sütun başlıklarına tıklayarak sıralama yapabilirsiniz.
• Üst sağdaki kullanıcı bilgisi oturum açan kişiyi gösterir.
• Çıkış Yap butonuyla güvenli oturum kapatabilirsiniz.
";

        private string HelpUrunler() => @"
## Ürünler & Stok Yönetimi

### Ürün Ekleme

- Sol menüden Ürünler'e tıklayın.
- '+ Yeni Ürün' butonuna tıklayın.
- SKU (stok kodu), ürün adı, fiyat ve KDV oranını girin.
- Kategori ve tedarikçi seçin.
- Resim ekleyebilirsiniz (PNG/JPG destekli).

### Ürün Düzenleme

- Listede ürüne çift tıklayın.
- Bilgileri güncelleyin ve Kaydet'e basın.

### Stok Takibi

- Stok Takibi sayfası, depolar bazında anlık stok durumunu gösterir.
- Kırmızı renkli satırlar minimum stok seviyesinin altındaki ürünleri gösterir.

### Stok Hareketleri

- Giriş: Tedarikçiden mal geldiğinde kullanın.
- Çıkış: Hasarlı veya iade ürünler için kullanın.
- Transfer: Depolar arası ürün transferi.
- Sipariş verdiğinizde stok otomatik düşer.

✅ Her stok hareketinde not ekleyebilirsiniz (referans no, fatura no vb.)
";

        private string HelpSiparisler() => @"
## Siparişler

### Yeni Sipariş Oluşturma

1. Siparişler menüsüne gidin.
2. '+ Yeni Sipariş' butonuna tıklayın.
3. Müşteri seçin (açılır listeden).
4. Ürün ve adet girerek '+ Ekle' ile sepete ekleyin.
5. Not ekleyebilirsiniz (isteğe bağlı).
6. 'Sipariş Oluştur' ile kaydedin.

### Sipariş Durumunu Güncelleme

- Siparişler listesinde bir satıra SAĞ TIKLAYARAK menüyü açın.
- Sipariş Durumu: Beklemede → Hazırlanıyor → Kargoda → Teslim Edildi
- Ödeme Durumu: Ödenmedi → Kısmi Ödendi → Ödendi

✅ Hızlı Onayla seçeneği: Durumu 'Hazırlanıyor' ve ödemeyi 'Ödendi' olarak tek seferde ayarlar.

### Sipariş Detayı

- Satıra ÇİFT TIKLAYARAK detay ekranını açın.
- Bu ekrandan hem sipariş hem ödeme durumunu güncelleyebilirsiniz.

### Renk Kodları

• Yeşil zemin = Ödendi
• Sarı zemin = Ödenmedi (bekliyor)
• Kırmızı zemin = İptal edildi
";

        private string HelpMusteriler() => @"
## Müşteri Yönetimi

### Müşteri Ekleme

1. Müşteriler menüsüne gidin.
2. '+ Yeni Müşteri' butonuna tıklayın.
3. Ad Soyad, telefon, e-posta ve adres bilgilerini girin.
4. Vergi no (kurumsal müşteriler için) ekleyebilirsiniz.

### Müşteri Düzenleme

- Listede müşteriye çift tıklayın.
- Bilgileri güncelleyin ve Kaydet'e basın.

### Müşteri İstatistikleri

- Her müşterinin kaç sipariş verdiği ve toplam harcaması görüntülenir.
- Bu bilgi müşteri listesinde 'Toplam Sipariş' ve 'Toplam Harcama' sütunlarında gösterilir.

💡 İpucu: Müşteri eklemeden sipariş oluşturamazsınız. Önce müşteri ekleyin.
";

        private string HelpPersonel() => @"
## Personel Yönetimi

### Personel Ekleme

1. Personel menüsüne gidin.
2. '+ Yeni Personel' butonuna tıklayın.
3. Kişisel bilgileri, departman ve pozisyonu girin.
4. Giriş tarihi ve maaş bilgilerini ekleyin.

### İzin Yönetimi

- Personel listesinde personele sağ tıklayın veya 'İzin Ekle' butonunu kullanın.
- İzin türü: Yıllık, Mazeret, Hastalık, Ücretsiz
- Başlangıç ve bitiş tarihi girin.
- İzin onay durumları: Beklemede, Onaylandı, Reddedildi

### Personel Durumları

• Aktif - Çalışıyor
• İzinli - İzinde
• Ayrıldı - İşten ayrıldı

💡 İpucu: Personel oluştururken sistem otomatik olarak e-posta ve giriş şifresi oluşturur.
Varsayılan şifre: 123456 (çalışana bildirip değiştirmesini isteyin)
";

        private string HelpAjanda() => @"
## Ajanda & Takvim

### Ajanda Nedir?

Ajanda, iş randevularınızı, önemli tarihleri ve notlarınızı takvim üzerinde yönetmenizi sağlar.

### Not Ekleme

1. Ajanda menüsüne gidin.
2. Sol taraftaki takvimden bir gün seçin.
3. '+ Not Ekle' butonuna tıklayın.
4. Başlık ve içerik girin.
5. Etiket seçin: Genel, Toplantı, Hatırlatma, Önemli.
6. Kaydet'e basın.

### Not Düzenleme

- Sağ taraftaki listede nota çift tıklayın.
- Değişikliklerinizi yapın ve Kaydet'e basın.

### Not Silme

- Listeden notu seçin.
- '🗑 Sil' butonuna tıklayın ve onaylayın.
- Ya da notu açarak silme seçeneğini kullanın.

### Renk Kodları (Etiketler)

• Mor (Genel) - Genel notlar
• Yeşil (Toplantı) - Toplantı ve görüşmeler
• Turuncu (Hatırlatma) - Hatırlatmalar
• Kırmızı (Önemli) - Acil ve önemli işler

✅ Notlar bilgisayarınıza yerel olarak kaydedilir. Internet kesintisinde de erişebilirsiniz.
";

        private string HelpIpuclari() => @"
## İpuçları & Sık Sorulan Sorular

### Genel İpuçları

💡 Tablo sütunlarını genişletebilir, sürükleyerek yeniden sıralayabilirsiniz.
💡 Listelerde arama yapmak için tablonun üstündeki arama kutusunu kullanın.
💡 Yenile butonu verileri sunucudan tekrar çeker.
💡 Program her zaman internet bağlantısı gerektirir (Supabase kullanır).

### Sık Sorulan Sorular

### Sipariş oluştururken ürün göremiyorum?
- Önce Ürünler sayfasından ürün ekleyin.

### Stok azalmıyor?
- Sipariş oluşturulunca stok rezervasyona alınır.
- Stok Hareketleri'nden manuel 'çıkış' hareketi de ekleyebilirsiniz.

### Bildirim gelmiyor?
- Bildirimler sayfasını kontrol edin.
- Bildirimler sistemi tarafından otomatik oluşturulur (düşük stok, yeni sipariş vb.)

### Şifremi unuttum?
- Şu an program içinde şifre sıfırlama özelliği yoktur.
- Veritabanından (Supabase Dashboard) şifreyi manuel sıfırlayabilirsiniz.

### Program açılmıyor?
- .env dosyasının program klasöründe olduğundan emin olun.
- SUPABASE_URL ve SUPABASE_KEY değerlerinin doğru olduğunu kontrol edin.
- İnternet bağlantınızı kontrol edin.

⚠️ Önemli: Veritabanı bağlantısı olmadan program çalışmaz.
Lütfen .env dosyasını program klasöründe muhafaza edin.
";
    }
}
