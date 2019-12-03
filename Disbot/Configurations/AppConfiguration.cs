using Disbot.Classes;
using DSharpPlus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Disbot.Configurations
{
    public class AppConfiguration
    {
        public DiscordConfigTemplate Discord { get; set; }
        public string CommandPrefix { get; set; }
        public LogLevel LogLevel { get; set; }
        public string SQLiteConnectionString { get; set; }
        public static AppConfiguration Content { get; }
        static AppConfiguration()
        {
            var configPath = "appsettings.json";
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
                    CommandPrefix = "!",
                    LogLevel = LogLevel.Info
                };
                Content = content;
                File.WriteAllText(configPath, JsonConvert.SerializeObject(content));
            }

        }
    }
}
