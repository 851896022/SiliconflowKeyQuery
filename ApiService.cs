using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KeyQuery
{
    public class ApiService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfig _config;

        public ApiService(ApiConfig config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "KeyQuery/1.0");
        }

        public async Task<string> QueryBalanceAsync(string apiKey)
        {
            var retryCount = 0;
            Exception? lastException = null;

            while (retryCount <= _config.MaxRetries)
            {
                try
                {
                    var url = $"{_config.BaseUrl}/user/info";
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<ApiResponse>(content);

                        if (result?.Code == 20000 && result.Data != null)
                        {
                            return result.Data.Balance ?? "0";
                        }
                        else
                        {
                            throw new Exception($"API返回错误: {result?.Message ?? "未知错误"}");
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"HTTP错误: {response.StatusCode} - {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retryCount++;
                    if (retryCount <= _config.MaxRetries)
                    {
                        await Task.Delay(_config.RetryDelayMs * retryCount);
                    }
                }
            }

            throw new Exception($"查询失败，已重试{_config.MaxRetries}次: {lastException?.Message}");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_config.BaseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ApiResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("data")]
        public UserData? Data { get; set; }
    }

    public class UserData
    {
        [JsonPropertyName("balance")]
        public string? Balance { get; set; }

        [JsonPropertyName("chargeBalance")]
        public string? ChargeBalance { get; set; }

        [JsonPropertyName("totalBalance")]
        public string? TotalBalance { get; set; }
    }
} 