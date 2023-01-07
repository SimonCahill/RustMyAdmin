using System;

namespace RustMyAdmin.Backend.Config {

    using Newtonsoft.Json;

    using System.IO;
    using System.Text.RegularExpressions;

    public class ConfigManager {

        private static ConfigManager? _instance = null;

        public static ConfigManager Instance {
            get => _instance ?? (_instance = new ConfigManager());
        }

        public Dictionary<string, object> Configurations { get; private set; }

        private FileInfo m_configFile;

        readonly Regex MultiConfRegex = new Regex(@"^([A-z_]+\.)([A-z_]+\.?)+$");

        public ConfigManager(FileInfo? configFile = null) {
            Configurations = new Dictionary<string, object>();
            m_configFile = configFile ?? Extensions.ConfigFile;
        }

        public async Task LoadConfigAsync() {
            if (!m_configFile.Exists || m_configFile.Length <= 10) {
                await SaveConfigAsync(true);
            }

            using (var fReader = m_configFile.OpenText()) {
                var fContents = await fReader.ReadToEndAsync();
                Configurations = JsonConvert.DeserializeObject<Dictionary<string, object>>(fContents) ?? new Dictionary<string, object>();
            }
        }

        public async Task SaveConfigAsync(bool writeDefault = false) {
            if (m_configFile.Directory?.Exists == false) {
                m_configFile.Directory?.Create();
            }

            using (var fWriter = new StreamWriter(m_configFile.OpenWrite())) {
                string jsonString = string.Empty;
                if (writeDefault) {
                    LoadDefaults();
                }
                jsonString = JsonConvert.SerializeObject(Configurations);

                await fWriter.WriteAsync(jsonString);
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
            if (string.IsNullOrEmpty(config)) { throw new ArgumentNullException(nameof(config), "The name of the config must not be null or empty!"); }

            if (MultiConfRegex.IsMatch(config)) {
                return HasConfig(Configurations, config);
            }

            return Configurations.ContainsKey(config);
        }

        private bool HasConfig(Dictionary<string, object> cfg, string configName) {
            if (cfg == null) { throw new ArgumentNullException(nameof(cfg), "Config object must not be null!"); }
            if (string.IsNullOrEmpty(configName)) { throw new ArgumentNullException(nameof(configName), "The config name must not be null or empty!"); }


            if (MultiConfRegex.IsMatch(configName)) {
                var curSection = GetConfigSection(configName, out var nextSection);
                if (!cfg.ContainsKey(curSection)) { return false; }

                if (cfg.TryGetValue(configName, out var subConf)) {
                    if (IsConfigDictSubType(subConf)) {
                        return HasConfig(subConf as Dictionary<string, object>, nextSection);
                    }
                }

                return false;
            }

            return cfg.ContainsKey(configName);
        }

        /// <summary>
        /// Gets the desired config with specified type.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to retrieve. This method will attempt to cast the config value to the type passed.</typeparam>
        /// <param name="configName">The name of the configuration to retrieve.</param>
        /// <returns>The config value.</returns>
        /// <exception cref="ArgumentNullException">If the input parameter "configName" is null or empty.</exception>
        /// <exception cref="ConfigNotFoundException">If the desired configuration could not be found.</exception>
        public T GetConfig<T>(string configName) {
            if (string.IsNullOrEmpty(configName)) { throw new ArgumentNullException(nameof(configName), "The name of the config must not be null or empty!"); }
            if (!HasConfig(configName)) { throw new ConfigNotFoundException(configName); }

            return GetConfig<T>(Configurations, configName);
        }

        private T GetConfig<T>(Dictionary<string, object> obj, string configName) {
            if (string.IsNullOrEmpty(configName)) { throw new ArgumentNullException(nameof(configName), "The name of the config must not be null or empty!"); }
            if (obj == null) { return default; }
            if (!HasConfig(obj, configName)) { throw new ConfigNotFoundException(configName); }

            var nestedConfig = MultiConfRegex.IsMatch(configName);

            if (nestedConfig) {
                var curSection = GetConfigSection(configName, out var nextSection);
                if (!obj.TryGetValue(curSection, out var subconf)) {
                    if (!IsConfigDictSubType(subconf)) { return (T)subconf; }

                    return GetConfig<T>(subconf as Dictionary<string, object>, nextSection);
                }
            }

            if (obj.TryGetValue(configName, out var conf)) {
                return (T)conf;
            }

            throw new ConfigNotFoundException(nameof(configName));
        }

        private bool IsConfigDictSubType(object obj) => obj is Dictionary<string, object>;

        private string GetConfigSection(string config, out string next) {
            if (!MultiConfRegex.IsMatch(config)) {
                next = string.Empty;
                return config;
            }

            var indexOfFirstPeriod = config.IndexOf('.');
            next = config.Substring(indexOfFirstPeriod + 1);
            return config.Substring(0, indexOfFirstPeriod - 1);
        }

        /// <summary>
        /// Loads a default set of configurations
        /// </summary>
        private void LoadDefaults() {
            Configurations = new Dictionary<string, object> {
                { "Language", "en_EN" }
                // TODO: Add more configs as needed here
            };
        }

    }

}

