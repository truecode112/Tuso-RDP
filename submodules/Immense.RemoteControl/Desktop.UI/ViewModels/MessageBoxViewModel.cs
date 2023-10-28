﻿using Avalonia.Controls;
using Immense.RemoteControl.Desktop.UI.Controls;
using Immense.RemoteControl.Desktop.UI.Services;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Immense.RemoteControl.Desktop.Shared.Abstractions;
using Microsoft.Extensions.Logging;

namespace Immense.RemoteControl.Desktop.UI.ViewModels
{
    public interface IMessageBoxViewModel
    {
        bool AreYesNoButtonsVisible { get; set; }
        string Caption { get; set; }
        bool IsOkButtonVisible { get; set; }
        string Message { get; set; }
        ICommand NoCommand { get; }
        ICommand OKCommand { get; }
        MessageBoxResult Result { get; set; }
        ICommand YesCommand { get; }
    }

    public class MessageBoxViewModel : BrandedViewModelBase, IMessageBoxViewModel
    {
        public MessageBoxViewModel
            (IBrandingProvider brandingProvider,
            IAvaloniaDispatcher dispatcher,
            ILogger<BrandedViewModelBase> logger)
            : base(brandingProvider, dispatcher, logger)
        {
        }

        public bool AreYesNoButtonsVisible
        {
            get => Get<bool>();
            set => Set(value);
        }

        public string Caption
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public bool IsOkButtonVisible
        {
            get => Get<bool>();
            set => Set(value);
        }

        public string Message
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }
        public ICommand NoCommand => new RelayCommand<Window>(window =>
        {
            Result = MessageBoxResult.No;
            window?.Close();
        });

        public ICommand OKCommand => new RelayCommand<Window>(window =>
        {
            Result = MessageBoxResult.OK;
            window?.Close();
        });

        public MessageBoxResult Result { get; set; } = MessageBoxResult.Cancel;

        public ICommand YesCommand => new RelayCommand<Window>(window =>
        {
            Result = MessageBoxResult.Yes;
            window?.Close();
        });
    }
}
