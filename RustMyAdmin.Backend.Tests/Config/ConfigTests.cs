using System;

namespace RustMyAdmin.Backend.Tests.Config {

    using Microsoft.VisualBasic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json;

    using RustMyAdmin.Backend;
    using RustMyAdmin.Backend.Config;
    using RustMyAdmin.Backend.Exceptions;

    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains the unit tests for the ConfigManager class.
    /// </summary>
    [TestClass]
    public class ConfigTests {

        internal static Dictionary<string, object> TestConfiguration = new Dictionary<string, object> {
            { "TestConfig_A", true },
            { "TestConfig_B", 1337 },
            {
                "TestConfig_C", new Dictionary<string, object> {
                    { "TestConfig_C_A", new { I = "I", Am = 0, A = true, ThingBecauseObjectIsReserved = 'G' } }
                }
            }
        };

        internal string TestConfigurationJson { get; } = JsonConvert.SerializeObject(TestConfiguration);
        internal FileInfo TestConfigFile { get; private set; }

        public ConfigTests() {
            TestConfigFile = GetTmpFile();
        }

        private FileInfo GetTmpFile() {
            var tmpFile = new FileInfo(Path.Combine(Path.GetTempFileName()));

            using (var fWriter = new StreamWriter(tmpFile.OpenWrite())) {
                fWriter.WriteLine(TestConfigurationJson);
            }

            return tmpFile;
        }

        [TestMethod]
        public void CanInstantiateConfigManagerClass() {
            var configMan = new ConfigManager(TestConfigFile);

            Assert.IsNotNull(configMan);
        }

        [TestMethod]
        public async void ConfigManagerDoesNotThrowExceptionWhenLoading() {
            var configMan = new ConfigManager(TestConfigFile);

            await configMan.LoadConfigAsync();
        }

        [TestMethod]
        public async void ConfigManagerCanFindTestValues() {
            var configMan = new ConfigManager(TestConfigFile);

            await configMan.LoadConfigAsync();

            Assert.IsTrue(configMan.HasConfig("TestConfig_A"));
            bool configA = configMan.GetConfig<bool>("TestConfig_A");
            Assert.AreEqual(configA, TestConfiguration["TestConfig_A"]);

            Assert.IsTrue(configMan.HasConfig("TestConfig_B"));
            var configB = configMan.GetConfig<int>("TestConfig_B");
            Assert.AreEqual(configB, TestConfiguration["TestConfig_B"]);

            Assert.IsTrue(configMan.HasConfig("TestConfig_C"));
            var configC = configMan.GetConfig<Dictionary<string, object>>("TestConfig_C");
            Assert.AreEqual(configC, TestConfiguration["TestConfig_C"]);
        }

        [TestMethod]
        public async void ConfigManagerCanFindMultiConfigNames() {
            var configMan = new ConfigManager(TestConfigFile);
            await configMan.LoadConfigAsync();

            Assert.IsTrue(configMan.HasConfig("TestConfig_C.TestConfig_C_A"));
            object config = configMan.GetConfig<dynamic>("TestConfig_C.TestConfig_C_A");
            Assert.AreEqual(config, (TestConfiguration["TestConfig_C"] as Dictionary<string, object>)["TestConfig_C_A"]);
        }
    }
}

