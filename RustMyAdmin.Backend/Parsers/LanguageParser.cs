using System;

namespace RustMyAdmin.Backend.Parsers {

    using Exceptions;

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

        public static List<LanguageParser> Languages { get; private set; } = new List<LanguageParser>();

        public static LanguageParser GetLanguageById(string langId) {
            if (string.IsNullOrEmpty(langId)) { throw new ArgumentNullException(nameof(langId), "The language ID must not be null or empty!"); }

            var lang = Languages.FirstOrDefault(x => x.LangIdentity == langId);

            if (lang == default) {
                throw new InvalidOperationException($"The language with the ID { langId } could not be found!");
            }

            return lang;
        }

        const string LangNameTag = "lang_identifier";
        const string LangDescriptionTag = "lang_description";
        const string LangVersionTag = "lang_version";
        const string VariableDetectionRegexPattern = @"(\$\{[A-z_][A-z_]+\})";

        readonly Regex VariableDetectionRegex = new Regex(VariableDetectionRegexPattern, RegexOptions.Compiled);

        public FileInfo? LangFile { get; private set; }

        public string LangIdentity { get; private set; }
        public string LangDescription { get; private set; }
        public string LangVersion { get; private set; }

        internal static LanguageParser? FallbackInstance { get; private set; } = null;

        private Dictionary<string, Dictionary<string, string>> m_sections;

        /// <summary>
        /// Dictionary containing a key-value map with variable name and expansion functions.
        /// </summary>
        private Dictionary<string, Func<string>> m_variableExpanders = new Dictionary<string, Func<string>> {
            { "year",       DateTime.Now.Year.ToString },
            { "month",      DateTime.Now.Month.ToString },
            { "day",        DateTime.Now.Day.ToString },
            { "HH", () =>   DateTime.Now.Hour.ToString("HH") },
            { "hh", () =>   DateTime.Now.Hour.ToString("hh") },
            { "tt", () =>   DateTime.Now.ToString("tt") },
            { "mm",         DateTime.Now.Minute.ToString },
            // add more as seen fit
        };

        private LanguageParser() => m_sections = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Constructs a new instance of this object and sets the path to the language file.
        /// </summary>
        /// <remarks >
        /// This is the recommended constructor to use.
        /// </remarks>
        /// <param name="langFile">The path to the lang file</param>
        public LanguageParser(FileInfo langFile) : this() {
            LangFile = langFile;
        }

        /// <summary>
        /// Takes the contents of the lang file and attempts to parse them.
        /// </summary>
        /// <remarks >
        /// This constructor will directly attempt to parse the input!
        /// </remarks>
        /// <param name="langFileContents">The contents of the language file.</param>
        public LanguageParser(ref string langFileContents) : this() {
            LangFile = null;
            ParseLangFile(ref langFileContents);
        }

        public bool ParseLangFile() {
            if (LangFile == null || String.IsNullOrEmpty(LangFile.FullName)) {
                throw new InvalidOperationException("Language file path must not be null or empty!");
            } else if (!LangFile.Exists) {
                throw new InvalidOperationException($"Language file does not exist.");
            }

            var fileContents = File.ReadAllText(LangFile.FullName);
            var success = ParseLangFile(ref fileContents);

            if (success) {
                Languages.Add(this);
            }

            return success;
        }

        /// <summary>
        /// Attempts to get a translation string, without having to faff around with any potentially arising exceptions.
        /// </summary>
        /// <param name="section">The section to search in</param>
        /// <param name="name">The name of the translation</param>
        /// <param name="text">Out-param containing the translation text.</param>
        /// <param name="default">The default value</param>
        /// <returns>true if the translation was found or the default was used. False otherwise.</returns>
        public bool TryGetTranslation(string section, string name, out string text, string? @default = null) {
            try {
                text = GetTranslation(section, name, @default);
            } catch {
                // simple catch-all because the programmer didn't want to faff around with them.
                text = @default == null ? string.Empty : @default;
                return string.IsNullOrEmpty(text); // still return 'true' if the default was used.
            }

            return true;
        }

        /// <summary>
        /// Gets a translation string depending on the section and name of the translation.
        /// </summary>
        /// <param name="section">The section to search in.</param>
        /// <param name="name">The name of the translation string.</param>
        /// <param name="default">A default string to return if the parameter is not found.</param>
        /// <returns>The requested translation string.</returns>
        /// <exception cref="ArgumentException">If an invalid argument was passed.</exception>
        /// <exception cref="LanguageParserException">If an error occurred; errors include wrong sections or translation names.</exception>
        public string GetTranslation(string section, string name, string? @default = null) {
            if (string.IsNullOrEmpty(section)) { throw new ArgumentException("Section variable must not be null or empty! This indicates a bug!", nameof(section)); }
            if (string.IsNullOrEmpty(name)) { throw new ArgumentException("Name variable must not be null or empty! This indicates a bug!", nameof(name)); }

            if (!m_sections.TryGetValue(section, out var sectDict)) {
                if (@default != null) {
                    return @default;
                } else if (FallbackInstance != null) {
                    return FallbackInstance.GetTranslation(section, name, @default);
                }
                throw new LanguageParserException(section, name, "Could not find the requested section!");
            } else if (!sectDict.TryGetValue(name, out var translation)) {
                if (@default != null) {
                    return @default;
                } else if (FallbackInstance != null) {
                    return FallbackInstance.GetTranslation(section, name, @default);
                }
                throw new LanguageParserException(section, name, "Could not find the requested translation string in the selected section!");
            } else {
                return HandleVariableInTranslation(ref translation);
            }
        }

        /// <summary>
        /// Handles the text replacement of variables in the translation text.
        /// </summary>
        /// <param name="translation">A reference to the current translation string</param>
        /// <returns>The translation string with all variables replaced.</returns>
        private string HandleVariableInTranslation(ref string translation) {
            MatchCollection matches;
            if (!TranslationContainsVariable(ref translation, out matches)) { return translation; }

            foreach (Match match in matches) {
                translation = translation.Replace(match.Value, GetReplacementForVariable(match.Value));
            }

            return translation;
        }

        /// <summary>
        /// Determines whether or not a translation string contains a variable or not.
        /// </summary>
        /// <param name="translation">The translation to check for variables.</param>
        /// <returns><code>true</code> if the translation string contains a variable.</returns>
        private bool TranslationContainsVariable(ref string translation, out MatchCollection matches) {
            return (matches = VariableDetectionRegex.Matches(translation)).Count > 0;
        }

        /// <summary>
        /// Gets a replacement string for the variable.
        /// </summary>
        /// <param name="variable">The variable to replace.</param>
        /// <returns>The string value the variable represents, or string.Empty.</returns>
        private string GetReplacementForVariable(string variable) {
            // Get the actual name of the variable first
            var varName = GetVariableName(ref variable);

            if (m_variableExpanders.TryGetValue(varName, out var func)) {
                return func();
            }

            switch (varName) {
                case LangNameTag:
                    return LangIdentity ?? string.Empty;
                case LangDescriptionTag:
                    return LangDescription ?? string.Empty;
                case LangVersionTag:
                    return LangVersion ?? string.Empty;
                default:
                    break;
            }

            foreach (var section in m_sections) {
                if (section.Value.TryGetValue(variable, out var replacement)) {
                    return replacement;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the name of the variable contained within the `${variable_name}` structure
        /// </summary>
        /// <param name="variable">The variable string</param>
        /// <returns>The name of the variable.</returns>
        private string GetVariableName(ref string variable) {
            var indexOfFirstBracket = variable.IndexOf('{') + 1;
            var indexOfLastBracket = variable.LastIndexOf('}') - 2;

            return variable.Substring(indexOfFirstBracket, indexOfLastBracket); 
        }

        /// <summary>
        /// Parses the contents of the language file.
        /// </summary>
        /// <param name="langFileContents">The actual file contents.</param>
        /// <returns>true if the file was parsed correctly. False otherwise.</returns>
        /// <exception cref="ArgumentException">If an invalid argument was passed.</exception>
        private bool ParseLangFile(ref string langFileContents) {
            if (String.IsNullOrEmpty(langFileContents)) { throw new ArgumentException($"{nameof(langFileContents)} must not be empty!"); }

            // Ensure the m_sections dictionary is empty
            m_sections.Clear();

            var lines = langFileContents.Split(new char[] { '\n' }); // Split text string into lines

            // Yes, this will create a closure, but it's more more legible than a huge effin foreach
            var currentSection = string.Empty; // By default, we're in a global section
            var currentDict = new Dictionary<string, string>();

            foreach (var line in lines) {
                HandleSingleLineInTranslationFile(line, ref currentSection, ref currentDict);
            }

            // The last element cannot be added by the HandleSingleLineInTranslationFile method, so we have to manually add
            // the dictionary here.
            // The variables are passed as references, so it's all good
            m_sections.TryAdd(currentSection, currentDict);

            return !string.IsNullOrEmpty(LangIdentity) && !string.IsNullOrEmpty(LangDescription) && m_sections.Count > 0;
        }

        /// <summary>
        /// Handles a single line from a translation file.
        /// </summary>
        /// <param name="line">The line to parse</param>
        private void HandleSingleLineInTranslationFile(string line, ref string currentSection, ref Dictionary<string, string> currentDict) {
            // First trim line
            line = line.Trim();

            // Now remove any comments
            if (string.IsNullOrEmpty(line) || line[0] == '#') { return; } // skip

            if (IsLineSection(ref line, out var newSection)) {
                m_sections.Add(currentSection, currentDict);
                currentSection = newSection;
                currentDict = new Dictionary<string, string>();
                return;
            }

            var translation = GetTranslation(ref line);
            if (translation == null) { return; } // not a valid translation string

#pragma warning disable CS8604 // Possible null reference argument.
            if (translation?.IsMultiLine == true && currentDict.ContainsKey(translation?.TranslationName)) {
                currentDict[translation?.TranslationName] += $"\n{translation?.Contents}";
            } else {
                // First check for special values, such as lang_identifier, lang_description, and lang_version
                if (translation?.TranslationName.Equals(LangNameTag, StringComparison.InvariantCultureIgnoreCase) == true) {
                    LangIdentity = translation?.Contents ?? string.Empty;
                    return;
                } else if (translation?.TranslationName.Equals(LangDescriptionTag, StringComparison.InvariantCultureIgnoreCase) == true) {
                    LangDescription = translation?.Contents ?? string.Empty;
                    return;
                } else if (translation?.TranslationName.Equals(LangVersionTag, StringComparison.InvariantCultureIgnoreCase) == true) {
                    LangVersion = translation?.Contents ?? string.Empty;
                    return;
                }

                currentDict.TryAdd(translation?.TranslationName, translation?.Contents);
            }
#pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <summary>
        /// Determines whether or not a given line is a new section
        /// </summary>
        /// <param name="line">A ref variable to the current line</param>
        /// <param name="sectionName">An out param containing the name of the string or <code >string.Empty</code> if no match was found.</param>
        /// <returns></returns>
        private bool IsLineSection(ref string line, out string sectionName) {
            var sectionRegex = new Regex(@"\[[A-z0-9-_ ]+\]");

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
            translationContents = line.Substring(indexOfFirstEquals + 1).Trim().Trim('"');

            return (isMultiLineComponent, translationName, translationContents);
        }

        /// <summary>
        /// Replaces certain special character strings with actual characters.
        /// </summary>
        /// <param name="translation">The translation content</param>
        /// <returns>A new string with the replaced values. The translation param is not touched.</returns>
        private string ReplaceSpecialCharsInTranslation(ref string translation) {
            return translation.Replace(@"\r", string.Empty)
                              .Replace(@"\n", "\n")
                              .Replace(@"\t", "\t")
                              .Replace("\\\"", "\"")
                              .Replace(@"\'", "\'"); // Maybe add more in the future
        }

    }
}
