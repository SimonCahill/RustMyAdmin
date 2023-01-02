using System;

namespace RustMyAdmin.Backend.Config {

    public class ConfigNotFoundException: Exception {

        /// <summary>
        /// Gets or sets the name of the queried config.
        /// </summary>
        public string? NameOfConfig { get; private set; }

        /// <summary>
        /// Constructs a new instance of this object.
        /// </summary>
        /// <param name="nameOfConfig">The name of the queried config.</param>
        /// <param name="msg">The message the exception was thrown with</param>
        public ConfigNotFoundException(string? nameOfConfig, string? msg): base(msg) {
            NameOfConfig = nameOfConfig;
        }

        public ConfigNotFoundException(string? nameOfConfig): this(nameOfConfig, "The queried config could not be found!") { }

        /// <summary>
        /// Gets a string representation of the thrown exception.
        /// </summary>
        /// <returns>A string containing the exception message and the queried config.</returns>
		public override string ToString() {
			return $"{ Message }\nName of queried config: { NameOfConfig }";
		}
	}

}

