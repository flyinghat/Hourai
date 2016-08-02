using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using Newtonsoft.Json;

namespace DrumBot {
    [Serializable]
    public class Config {

        public const string ConfigFilePath = "config.json";

        readonly Dictionary<ulong, ServerConfig> _serversConfigs;

        public Config() {
            _serversConfigs = new Dictionary<ulong, ServerConfig>();
        }

        public static Config Load() {
            string fullPath = Path.Combine(Bot.ExecutionDirectory,
                ConfigFilePath);
            Log.Info($"Loading DrumBot config from {fullPath}...");
            var config = JsonConvert.DeserializeObject<Config>(
                            File.ReadAllText(ConfigFilePath));
            Log.Info($"Setting log directory to: { config.LogDirectory }");
            Log.Info($"Setting config directory to: { config.ConfigDirectory }");
            Log.Info("Config loaded.");
            return config;
        }

        public ServerConfig GetServerConfig(Server server) {
            if(!_serversConfigs.ContainsKey(server.Id))
                _serversConfigs[server.Id] = new ServerConfig(server);
            return _serversConfigs[server.Id];
        }

        public string Token { get; set; }
        public string BotName { get; set; }
        public ulong Owner { get; set; }
        public string LogDirectory { get; set; } = "logs";
        public string ConfigDirectory { get; set; } = "config";
        public char CommandPrefix { get; set; } = '~';
        public string SuccessResponse { get; set; } = ":thumbsup:";
        public int PruneLimit { get; set; } = 100;
    }
}
