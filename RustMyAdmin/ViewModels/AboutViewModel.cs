/**
 * AboutViewModel.cs
 *
 * Author: Simon Cahill (simon@simonc.eu)
 * File creation: 02.01.2023
 *
 * Copyright: © Simon Cahill - All Rights Reserved
 *
 */

using System;

namespace RustMyAdmin.ViewModels {

    using Backend.Config;
    using Backend.Parsers;

    public class AboutViewModel: BaseViewModel {

        ConfigManager m_configManager;
        LanguageParser m_language;

        public AboutViewModel() {
            base.Instance = this;
            m_configManager = ConfigManager.Instance;
            //m_language = LanguageParser.GetLanguageById(m_configManager.GetConfig<string>("Language"));
        }


        public string AboutText {
            //get => m_language.GetTranslation("about_page", "about_text");
            get =>
                "RustMyAdmin is the easy-to-use and all-encompassing Rust or Oxide server management system.\n" +
                "";
        }
    }
}

