using System;

namespace RustMyAdmin.Backend.Config {

    using Newtonsoft.Json;

    using System.IO;
    using System.Text.RegularExpressions;

    public class ConfigManager {

        public Dictionary<string, object> Configurations { get; private set; }

        readonly Regex MultiConfRegex = new Regex(@"([A-z_]\.?)+");

        public ConfigManager() {
            Configurations = new Dictionary<string, object>();
        }

        public async Task LoadConfigAsync() {
            using (var fReader = Extensions.ConfigFile.OpenText()) {
                var fContents = await fReader.ReadToEndAsync();
                Configurations = JsonConvert.DeserializeObject<Dictionary<string, object>>(fContents) ?? new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Determines whether or not a given config exists.
        /// </summary>
        /// <remarks >
        /// This method can use a specific nomenclature for retrieving contacts, with a dot-syntax.
        /// This means if several nested configs are present, the value of a nested configuration can be tested by using dot-naming.
        /// 
        /// <code >
        /// async Task<string> GetFooBarBazConfig() {
        ///     var configMan = new ConfigManager();
        ///     await configMan.LoadConfigAsync();
        ///     
        ///     if (!configMan.HasConfig("Foo.Bar.Baz")) { return string.Empty; }
        ///     
        ///     return configMan.GetConfig("Foo.Bar.Baz");
        /// }
        /// </code>
        /// </remarks>
        /// <param name="config">The name of the config to check for.</param>
        /// <returns><code >true</code> if the config name was found.</returns>
        public bool HasConfig(string config) {
            if (MultiConfRegex.IsMatch(config)) {
                return HasConfig(Configurations, config);
            }

            return Configurations.ContainsKey(config);
        }

        private bool HasConfig(Dictionary<string, object> cfg, string configName) {
            if (cfg == null) { throw new ArgumentNullException(nameof(cfg), "Config object must not be null!"); }
            if (string.IsNullOrEmpty(configName)) { throw new ArgumentNullException(nameof(configName), "The config name must not be null or empty!"); }


            if (MultiConfRegex.IsMatch(configName)) {
                var indexOfFirstPeriod = configName.IndexOf('.');

            }
        }

    }

}

