using System;

namespace RustMyAdmin.Backend.Config {

    using Newtonsoft.Json;

    using System.IO;

    public class ConfigManager {

        public Dictionary<string, object> Configurations { get; private set; }


        public ConfigManager() {
            Configurations = new Dictionary<string, object>();
        }

        public async Task LoadConfigAsync() {
            using (var fReader = Extensions.ConfigFile.OpenText()) {
                var fContents = await fReader.ReadToEndAsync();
                Configurations = JsonConvert.DeserializeObject<Dictionary<string, object>>(fContents) ?? new Dictionary<string, object>();
            }
        }

    }

}

