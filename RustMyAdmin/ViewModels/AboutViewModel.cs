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

    public class AboutViewModel: BaseViewModel {

        ConfigManager m_configManager;

        public AboutViewModel() {
            base.Instance = this;
            m_configManager = ConfigManager.Instance;
        }



    }
}

