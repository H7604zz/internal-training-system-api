using InternalTrainingSystem.Core.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace InternalTrainingSystem.Core.Helper
{
    public class ZoomHelper
    {
        private static string _clientId;
        private static string _clientSecret;
        private static string _accountId;

        public static void Configure(string clientId, string clientSecret, string accountId)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _accountId = accountId;
        }

        public static async Task<string> CreateRecurringMeetingAndGetJoinUrlAsync()
        {
            var token = await GetAccessTokenAsync(_clientId, _clientSecret, _accountId);

            var joinUrl = await CreateRecurringMeetingAsync(token);
            return joinUrl;
        }

        private static async Task<string> GetAccessTokenAsync(string clientId, string clientSecret, string accountId)
        {
            using var client = new HttpClient();

            var url = $"https://zoom.us/oauth/token?grant_type=account_credentials&account_id={accountId}";
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authHeader}");

            var response = await client.PostAsync(url, null);
            var body = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("access_token", out var tokenElement))
                throw new Exception("Không lấy được access_token: " + body);

            return tokenElement.GetString()!;
        }

        private static async Task<string> CreateRecurringMeetingAsync(string token)
        {
            using var client = new HttpClient();

            var meetingData = new
            {
                topic = "Phòng học online",
                type = 3,
                settings = new
                {
                    join_before_host = true,
                    waiting_room = false
                }
            };

            var json = JsonSerializer.Serialize(meetingData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await client.PostAsync("https://api.zoom.us/v2/users/me/meetings", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Zoom API Error ({response.StatusCode}): {body}");

            using var doc = JsonDocument.Parse(body);
            string joinUrl = doc.RootElement.GetProperty("join_url").GetString()!;

            return joinUrl;
        }
    }
}
