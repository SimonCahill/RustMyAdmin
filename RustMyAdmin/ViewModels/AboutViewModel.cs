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
    using Microsoft.UI.Xaml.Media.Imaging;

    public class AboutViewModel: BaseViewModel {

        ConfigManager m_configManager;
        LanguageParser m_language;

        public AboutViewModel() {
            base.Instance = this;
            m_configManager = ConfigManager.Instance;
            m_language = LanguageParser.GetLanguageById(m_configManager.GetConfig<string>("Language"));
        }


        public string AboutText {
            get => m_language.GetTranslation("about_page", "about_text");
        }

        public string Copyright {
            get => m_language.GetTranslation("about_page", "copyright");
        }
    }
}

