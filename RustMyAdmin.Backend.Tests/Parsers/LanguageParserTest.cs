using System;

namespace RustMyAdmin.Backend.Tests.Parsers {
    using Microsoft.VisualBasic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RustMyAdmin.Backend;
    using RustMyAdmin.Backend.Exceptions;
    using RustMyAdmin.Backend.Parsers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains test cases for the LanguageParser class in the MyRustAdmin.Backend library.
    /// </summary>
    [TestClass]
    public class LanguageParserTest {

        const string UnixLikeTestFile = @"/tmp/tmp_lang_file.lang";

        readonly string WindowsTestFile = @$"{ Path.GetTempPath() }\tmp_lang_file.lang";

        const string ExpectedLangIdentifier = "en_EN";
        const string ExpectedLangDescription = "Standard English (not American)";

        string TestTranslation = File.ReadAllText("Resources/Translations/en_EN.lang");

        [TestMethod]
        public void CreateNewInstanceOfObjectWithInvalidFilePath() {
            FileInfo langFile;

            switch (Environment.OSVersion.Platform) {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    langFile = new FileInfo(UnixLikeTestFile);
                    break;
                case PlatformID.Win32NT:
                    langFile = new FileInfo(WindowsTestFile);
                    break;
                default:
                    Assert.Fail("Invalid platform for testing!");
                    return;
            }

            var langParser = new LanguageParser(langFile);

            Assert.IsInstanceOfType(langParser, typeof(LanguageParser));
        }

        [TestMethod]
        public void CreateNewInstanceOfObjectAndParseLangFile() {
            var file = WriteTempFile();

            var langParser = new LanguageParser(file);

            Assert.AreEqual(file, langParser.LangFile);

            Assert.IsTrue(langParser.ParseLangFile());

            Assert.AreEqual(langParser.LangIdentity, ExpectedLangIdentifier);
            Assert.AreEqual(langParser.LangDescription, ExpectedLangDescription);
        }

        [TestMethod]
        public void CreateNewInstanceWithContents() {
            var langParser = new LanguageParser(ref TestTranslation);

            Assert.AreEqual(langParser.LangIdentity, ExpectedLangIdentifier);
            Assert.AreEqual(langParser.LangDescription, ExpectedLangDescription);
        }

        [TestMethod]
        public void GetSampleTextFromFile() {
            var langParser = new LanguageParser(ref TestTranslation);

            Assert.IsFalse(string.IsNullOrEmpty(langParser.GetTranslation("home_page", "page_title")));
        }

        [TestMethod]
        public void GetInvalidTranslation() {
            var langParser = new LanguageParser(ref TestTranslation);

            Assert.ThrowsException<ArgumentException>(() => langParser.GetTranslation(null, null));
            Assert.ThrowsException<ArgumentException>(() => langParser.GetTranslation("home_page", null));
            Assert.ThrowsException<LanguageParserException>(() => langParser.GetTranslation("invalid_section", "page_title"));
            Assert.ThrowsException<LanguageParserException>(() => langParser.GetTranslation("home_page", "invalid_translation_string_this_should_never_exist"));
        }

        [TestMethod]
        public void GetValidMultiLineString() {
            var langParser = new LanguageParser(ref TestTranslation);

            var text = langParser.GetTranslation("home_page", "welcome_text");
            Assert.IsFalse(string.IsNullOrEmpty(text));
            Assert.IsTrue(text.Count(char.IsWhiteSpace) > 0);
        }

        internal FileInfo WriteTempFile() {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    File.WriteAllText(UnixLikeTestFile, TestTranslation);
                    return new FileInfo(UnixLikeTestFile);
                case PlatformID.Win32NT:
                    File.WriteAllText(WindowsTestFile, TestTranslation);
                    return new FileInfo(WindowsTestFile);
                default: throw new Exception("Unsupported test platform!");
            }
        }

    }
}
