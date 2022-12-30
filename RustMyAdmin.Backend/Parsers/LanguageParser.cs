using System;

namespace RustMyAdmin.Backend.Parsers {

    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Simple class which parses language files, which are a psuedo-ini format.
    /// </summary>
    public class LanguageParser {

        public FileInfo? LangFile { get; private set; }

        private Dictionary<string, Dictionary<string, string>> m_sections;

        private LanguageParser() => m_sections = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Constructs a new instance of this object and sets the path to the language file.
        /// </summary>
        /// <remarks >
        /// This is the recommended constructor to use.
        /// </remarks>
        /// <param name="langFile">The path to the lang file</param>
        public LanguageParser(FileInfo langFile): this() {
            LangFile = langFile;
        }

        /// <summary>
        /// Takes the contents of the lang file and attempts to parse them.
        /// </summary>
        /// <remarks >
        /// This constructor will directly attempt to parse the input!
        /// </remarks>
        /// <param name="langFileContents">The contents of the language file.</param>
        public LanguageParser(ref string langFileContents): this() {
            LangFile = null;
        }

        public void ParseLangFile() {
            if (LangFile == null || String.IsNullOrEmpty(LangFile.FullName)) {
                throw new InvalidOperationException("Language file path must not be null or empty!");
            } else if (!LangFile.Exists) {
                throw new InvalidOperationException($"Language file does not exist.");
            }

            var fileContents = File.ReadAllText(LangFile.FullName);
            ParseLangFile(ref fileContents);
        }

        private void ParseLangFile(ref string langFileContents) {
            if (String.IsNullOrEmpty(langFileContents)) { throw new ArgumentException($"{ nameof(langFileContents) } must not be empty!"); }

            // Ensure the m_sections dictionary is empty
            m_sections.Clear();

            var lines = langFileContents.Split(new char[] { '\n' }); // Split text string into lines

            // Yes, this will create a closure, but it's more more legible than a huge effin foreach
            var currentSection = string.Empty; // By default, we're in a global section
            m_sections.Add(currentSection, new Dictionary<string, string>());

            // Converted this to lambda so we can mutate the 'line' variable as need be
            lines.ToList().ForEach(line => {
                // First trim line
                line = line.Trim();

                // Now remove any comments
                if (string.IsNullOrEmpty(line) || line[0] == '#') { return; } // skip

                if (IsLineSection(ref line, out currentSection)) {
                    m_sections.Add(currentSection, new Dictionary<string, string>());
                    return;
                }

                var translation = GetTranslation(ref line);
                if (translation == null) { return; } // not a valid translation string

#pragma warning disable CS8604 // Possible null reference argument.
                if (translation?.IsMultiLine == true) {
                    if (m_sections[currentSection].ContainsKey(translation?.TranslationName)) {
                        m_sections[currentSection][translation?.TranslationName] = $"{ m_sections[currentSection][translation?.TranslationName] }\n{ translation?.Contents }";
                    } else {
                        m_sections[currentSection].Add(translation?.TranslationName, translation?.Contents);
                    }
                } else {
                    m_sections[currentSection].Add(translation?.TranslationName, translation?.Contents);
                }
#pragma warning restore CS8604 // Possible null reference argument.
            });
        }

        /// <summary>
        /// Determines whether or not a given line is a new section
        /// </summary>
        /// <param name="line">A ref variable to the current line</param>
        /// <param name="sectionName">An out param containing the name of the string or <code >string.Empty</code> if no match was found.</param>
        /// <returns></returns>
        private bool IsLineSection(ref string line, out string sectionName) {
            var sectionRegex = new Regex(@"\[[A-z0-9-_ ]\]");

            var matches = sectionRegex.IsMatch(line);
            if (matches) {
                sectionName = line.Trim('[', ']');
                return true;
            }

            sectionName = string.Empty;
            return false;
        }

        /// <summary>
        /// Parses the translation contents of a given line.
        /// </summary>
        /// <remarks >
        /// The translation contents <b>DO NOT</b> contain newlines in the case of multi-line strings!
        /// </remarks>
        /// <param name="line">The line to parse</param>
        /// <returns>A tuple containing a bool indicating whether or not the string is multiline, the name of the translation, and finally the translation string itself - or at least the current portion of it.</returns>
        private (bool IsMultiLine, string TranslationName, string Contents)? GetTranslation(ref string line) {
            var indexOfFirstEquals = line.IndexOf('=');

            if (indexOfFirstEquals == -1) { return null; }

            var isMultiLineComponent = false;
            var translationName = string.Empty;
            var translationContents = string.Empty;

            translationName = line.Substring(0, indexOfFirstEquals - 1).Trim();

            if (translationName.EndsWith("[]", StringComparison.InvariantCultureIgnoreCase)) {
                isMultiLineComponent = true;
                translationName = translationName.Substring(0, translationName.Length - 2);
            }
            translationContents = line.Substring(indexOfFirstEquals + 1);

            return (isMultiLineComponent, translationName, translationContents);
        }

    }
}
