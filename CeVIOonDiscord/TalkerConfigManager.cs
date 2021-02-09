using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace CeVIOonDiscord
{
    [JsonObject]
    class TalkerConfig
    {
        // guildid -> userid -> config
        [JsonProperty]
        public Dictionary<ulong, Dictionary<ulong, ConfigEntry>> Entries { get; set; } = new Dictionary<ulong, Dictionary<ulong, ConfigEntry>>();
    }

    [JsonObject]
    class ConfigEntry
    {
        [JsonProperty("volume")]
        public uint Volume { get; set; }
        [JsonProperty("cast")]
        public string Cast { get; set; }
        public uint Speed { get; set; }
        [JsonProperty("Tone")]
        public uint Tone { get; set; }
        [JsonProperty("alpha")]
        public uint Alpha { get; set; }
        [JsonProperty("tonescale")]
        public uint Tonescale { get; set; }
        [JsonProperty("castspec")]
        public List<uint> Castspec { get; set; }
    }

    /// <summary>
    /// This class manages JSON files storing cast configurations each user perfers.
    /// </summary>
    class TalkerConfigManager
    {
        ///
        private static TalkerConfigManager manager = new TalkerConfigManager();
        private static TalkerConfig config;
        private const string json_file_path = "talkerconfig.json";

        /// <summary>
        /// 
        /// </summary>
        private TalkerConfigManager()
        {
            if (!File.Exists(json_file_path))
            {
                config = new TalkerConfig();
                UpdateConfigFile(json_file_path);
            }
            else
            {
                config = GetJsonConfigFromFile(json_file_path);
            }
        }

        /// <summary>
        /// Get the unique instance of this class.
        /// </summary>
        /// <returns>The instance of TalkerConfigManager. </returns>
        public static TalkerConfigManager getInstance()
        {
            return manager;
        }

        /// <summary>
        /// Get the configuration entry of the user whose ID is `user` in the guild whose ID id `guild`.
        /// </summary>
        /// <param name="guild">ID of the guild.</param>
        /// <param name="user">ID of the user.</param>
        /// <returns>The configuration parameters of the specified user.</returns>
        public ConfigEntry GetConfigEntry(ulong guild, ulong user)
        {
            var updated = false;
            if (!config.Entries.ContainsKey(guild))
            {
                config.Entries.Add(guild, new Dictionary<ulong, ConfigEntry>());
                updated = true;
            }

            if (!config.Entries[guild].ContainsKey(user))
            {
                config.Entries[guild].Add(user, new ConfigEntry()
                {
                    Cast = "さとうささら",
                    Volume = 80,
                    Tone = 50,
                    Tonescale = 50,
                    Speed = 50,
                    Alpha = 50,
                    Castspec = new List<uint>() { 100, 0, 0, 0 },
                });
                updated = true;
            }

            if (updated)
            {
                UpdateConfigFile(json_file_path);
            }

            return config.Entries[guild][user];
        }
        
        /// <summary>
        /// Notify the manager that something has been changed with configurations.
        /// In response, the manager writes current configurations into a JSON file.
        /// </summary>
        public void NotifyUpdate()
        {
            UpdateConfigFile(json_file_path);
        }

        /// <summary>
        /// Deserialize a JSON file `filepath` and get an object that represents it.
        /// </summary>
        /// <param name="filepath">Path to a JSON file.</param>
        /// <returns>An object that represents the specified JSON file.</returns>
        private TalkerConfig GetJsonConfigFromFile(string filepath)
        {
            using (var stream = new StreamReader(filepath, Encoding.UTF8))
            {
                var json = stream.ReadToEnd();
                return JsonConvert.DeserializeObject<TalkerConfig>(json);
            }
        }

        /// <summary>
        /// Update a JSON file `filepath` with current configurations.
        /// </summary>
        /// <param name="filepath">Path to a JSON file.</param>
        private void UpdateConfigFile(string filepath)
        {
            var json = JsonConvert.SerializeObject(config);
            using (var stream = new StreamWriter(filepath, false, Encoding.UTF8))
            {
                stream.Write(json);
            }
        }
    }
}
