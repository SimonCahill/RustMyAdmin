using System;

namespace RustMyAdmin.Backend.Exceptions {

    /// <summary>
    /// An exception that is thrown when the LanguageParser object encounters a situation it cannot handle.
    /// This exception may or may not be thrown because of programmer errors.
    /// </summary>
    public class LanguageParserException: Exception {

        /// <summary>
        /// Creates a new instance of this exception object.
        /// </summary>
        /// <param name="section">The section that was requested.</param>
        /// <param name="translationName">The name of the requested translation.</param>
        /// <param name="message">The error message.</param>
        public LanguageParserException(string section, string translationName, string message): base(message) {
            Section = section;
            TranslationName = translationName;
        }

        /// <summary>
        /// The requested section.
        /// </summary>
        public string Section { get; private set; }

        /// <summary>
        /// The requested translation.
        /// </summary>
        public string TranslationName { get; private set; }

        /// <summary>
        /// Creates a string representation of this exception instance.
        /// </summary>
        /// <returns>A string containing all vital information about the exception.</returns>
        public override string ToString() => $"{ Message }\nSection: { Section }\nRequested translation: { TranslationName }";

    }

}