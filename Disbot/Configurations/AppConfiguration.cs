using Disbot.Classes;
using DSharpPlus;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Disbot.Configurations
{
    public class AppConfiguration
    {
        public DiscordConfigTemplate Discord { get; set; }
        public Authorization Authorization { get; set; }
        public string CommandPrefix { get; set; }
        public LogLevel LogLevel { get; set; }
        public string SQLiteConnectionString { get; set; }
        public float MultiplyRate { get; set; }
        public static AppConfiguration Content { get; }
        static AppConfiguration()
        {
            var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "appsettings.json");
            if (File.Exists(configPath))
            {
                var content = File.ReadAllText(configPath);
                Content = JsonConvert.DeserializeObject<AppConfiguration>(content);
            }
            else
            {
                var content = new AppConfiguration()
                {
                    Discord = new DiscordConfigTemplate(),
                    Authorization = new Authorization(),
                    CommandPrefix = "!",
                    LogLevel = LogLevel.Info,
                    MultiplyRate = 100
                };
                Content = content;
                File.WriteAllText(configPath, JsonConvert.SerializeObject(content));
            }

        }
        public float GetUpdatedMultiplyRate()
        {
            var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "appsettings.json");
            var content = File.ReadAllText(configPath);
            var instance = JsonConvert.DeserializeObject<AppConfiguration>(content);
            return instance.MultiplyRate / 100;
        }
    }
}
