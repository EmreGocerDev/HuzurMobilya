using HuzurMobilya.Forms;
using HuzurMobilya.Services;

namespace HuzurMobilya;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            SupabaseService.InitializeAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var r = MessageBox.Show(
                ".env dosyasini kontrol edin!\nSUPABASE_URL ve SUPABASE_KEY dolu olmali.\nHata: " + ex.Message + "\nDevam?",
                "Baglanti Hatasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r == DialogResult.No) return;
        }
        Application.Run(new LoginForm());
    }
}
