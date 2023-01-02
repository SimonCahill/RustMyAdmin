/**
 * BaseViewModel.cs
 *
 * Author: Simon Cahill (simon@simonc.eu)
 * File creation: 02.01.2023
 *
 * Copyright: © Simon Cahill - All Rights Reserved
 *
 */

using System;

namespace RustMyAdmin.ViewModels {

    using System.ComponentModel;

    public abstract class BaseViewModel: INotifyPropertyChanged {

        protected BaseViewModel Instance { get; set; }

        public BaseViewModel() { }

		public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(Instance ?? this, new PropertyChangedEventArgs(propName));
	}
}

