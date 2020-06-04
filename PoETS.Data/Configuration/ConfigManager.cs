using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data.Configuration {
    public class ConfigManager {
        #region Singleton
        private static ConfigManager Instance = null;
        public static Config GetConfig() {
            if (Instance == null) {
                Instance = new ConfigManager();
            }

            return Instance.Config;
        }
        #endregion

        private string ConfigFilePath = ".\\appConfig.json";
        private Config Config { get; set; }

        private ConfigManager() {
            LoadConfig();
        }

        private void LoadConfig() {
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));
            VerifyConfig();
        }

        private void VerifyConfig() {
            Config.Synonyms = Config.Synonyms.Select(s => {
                var words = s.Words.Select(w => w.ToLower());
                s.Words = words.ToList();
                return s;
            }).ToList();

            WriteConfig();
        }

        private void WriteConfig() {
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(Config));
        }
    }
}
