using System;
using System.IO;
using System.Text.Json;

namespace KeyQuery
{
    public class ApiConfig
    {
        private static readonly string ConfigFile = "api_config.json";
        
        public string BaseUrl { get; set; } = "https://api.siliconflow.cn/v1";
        public string ApiKey { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;

        public static ApiConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    var json = File.ReadAllText(ConfigFile);
                    var config = JsonSerializer.Deserialize<ApiConfig>(json);
                    return config ?? new ApiConfig();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载配置文件失败: {ex.Message}");
            }

            return new ApiConfig();
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }
    }
} 