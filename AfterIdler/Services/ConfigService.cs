using AfterIdler.Models;
using System;
using System.IO;
using System.Text.Json;

namespace AfterIdler.Services
{

    /// 設定ファイルの読み書き
    /// </summary>
    public static class ConfigService
    {
    
        /// 設定ファイルパス
        /// </summary>
        private static readonly string ConfigPath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "config.json");

    
        /// 設定を読み込む
        /// </summary>
        public static AppConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    return new AppConfig();
                }

                string json = File.ReadAllText(ConfigPath);

                AppConfig? config =
                    JsonSerializer.Deserialize<AppConfig>(json);

                return config ?? new AppConfig();
            }
            catch
            {
                // 読み込み失敗時は初期値を返す
                return new AppConfig();
            }
        }

    
        /// 設定を保存する
        /// </summary>
        public static void Save(AppConfig config)
        {
            try
            {
                string json =
                    JsonSerializer.Serialize(
                        config,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // 保存失敗時は何もしない
            }
        }
    }
}