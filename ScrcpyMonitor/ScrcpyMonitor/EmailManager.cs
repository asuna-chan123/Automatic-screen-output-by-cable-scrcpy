using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ScrcpyMonitor
{
    public static class EmailManager
    {
        private const string ServiceId = "";
        private const string TemplateId = "";
        private const string PublicKey = "";
        private const string ApiUrl = "";
        
        public const string TailscaleLink = "https://login.tailscale.com/admin/invite/5dhW6Fi9N7XBcMECXPW711";

        private static string GetStatsFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "ScrcpyMonitor");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            return Path.Combine(folder, "email_stats.txt");
        }

        private static (DateTime Date, int Count, DateTime LastSent) ReadStats()
        {
            try
            {
                string path = GetStatsFilePath();
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path).Trim();
                    string[] parts = content.Split('|');
                    if (parts.Length == 3)
                    {
                        DateTime date = DateTime.Parse(parts[0]);
                        int count = int.Parse(parts[1]);
                        DateTime lastSent = new DateTime(long.Parse(parts[2]));
                        return (date, count, lastSent);
                    }
                }
            }
            catch { }

            return (DateTime.Today, 0, DateTime.MinValue);
        }

        private static void WriteStats(DateTime date, int count, DateTime lastSent)
        {
            try
            {
                string path = GetStatsFilePath();
                string content = $"{date:yyyy-MM-dd}|{count}|{lastSent.Ticks}";
                File.WriteAllText(path, content);
            }
            catch { }
        }

        public static async Task<(bool Success, string Message)> SendTailscaleInviteAsync(string toEmail)
        {
            var stats = ReadStats();

            // Reset daily count if it's a new day
            if (stats.Date < DateTime.Today)
            {
                stats.Date = DateTime.Today;
                stats.Count = 0;
            }

            // Check rate limits
            if (stats.Count >= 10)
            {
                return (false, "Daily limit reached (10 emails/day). Please try again tomorrow.");
            }

            if ((DateTime.Now - stats.LastSent).TotalSeconds < 30)
            {
                return (false, "Please wait 30 seconds before sending another email.");
            }

            // Send Email using EmailJS
            try
            {
                // 1. Tạo một object chứa dữ liệu giống hệt cấu trúc EmailJS yêu cầu
                var emailPayload = new
                {
                    service_id = ServiceId,
                    template_id = TemplateId,
                    user_id = PublicKey,
                    template_params = new
                    {
                        email = toEmail,
                        reusable_link = TailscaleLink
                    }
                };

                // 2. Tự động chuyển Object thành chuỗi JSON chuẩn
                string jsonString = JsonSerializer.Serialize(emailPayload);

                using (var client = new HttpClient())
                {
                    // 3. Gửi chuỗi JSON đi
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(ApiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Update stats
                        WriteStats(DateTime.Today, stats.Count + 1, DateTime.Now);
                        return (true, "Email sent successfully!");
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        return (false, $"Failed to send email. API returned: {response.StatusCode} - {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error sending email: {ex.Message}");
            }
        }
    }
}
