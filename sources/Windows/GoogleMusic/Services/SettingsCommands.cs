﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Views.Settings;

    using Windows.ApplicationModel.Store;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    public class SettingsCommands : ISettingsCommands
    {
        private const double SettingsWidth = 346;
        private const double LargeSettingsWidth = 646;

        private readonly IMediaElemenetContainerView mediaElemenetContainerView;

        private readonly Dictionary<Popup, PopupType> settingsPopups = new Dictionary<Popup, PopupType>();

        public SettingsCommands(IMediaElemenetContainerView mediaElemenetContainerView)
        {
            this.mediaElemenetContainerView = mediaElemenetContainerView;
        }

        private enum PopupType
        {
            Settings,
            LargeSettings,
            Full
        }

        public void Register()
        {
            SettingsPane.GetForCurrentView().CommandsRequested += this.CommandsRequested;
        }

        public void Unregister()
        {
            SettingsPane.GetForCurrentView().CommandsRequested -= this.CommandsRequested;
        }

        public void ActivateSettings(string name)
        {
            if (name == "upgrade")
            {
                this.CreatePopup(new UpgradeView());
            }
            else if (name == "link-lastfm")
            {
                this.CreatePopup(new LastfmAuthentificationPageView(), PopupType.Full);
            }
        }

        private void CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var cmd = new SettingsCommand(
                "accounts",
                "Accounts",
                (x) => this.CreatePopup(new AccountPageView()));

            args.Request.ApplicationCommands.Add(cmd);

            if (!this.AllPurchased())
            {
                cmd = new SettingsCommand("upgrade", "Upgrade", (x) => this.CreatePopup(new UpgradeView())); 
                args.Request.ApplicationCommands.Add(cmd);
            }

            cmd = new SettingsCommand(
                "support",
                "Support",
                (x) => this.CreatePopup(new SupportView()));

            args.Request.ApplicationCommands.Add(cmd);

            cmd = new SettingsCommand(
                "privacy",
                "Privacy Policy",
                (x) => this.CreatePopup(new PrivacyView()));

            args.Request.ApplicationCommands.Add(cmd);
        }

        private bool AllPurchased()
        {
#if DEBUG
            return CurrentAppSimulator.LicenseInformation.ProductLicenses.ContainsKey("Ultimate")
                && CurrentAppSimulator.LicenseInformation.ProductLicenses["Ultimate"].IsActive;
#else
            return CurrentApp.LicenseInformation.ProductLicenses.ContainsKey("Ultimate")
                && CurrentApp.LicenseInformation.ProductLicenses["Ultimate"].IsActive;
#endif
        }

        private void CreatePopup(UserControl view, PopupType type = PopupType.Settings)
        {
            var settingsWidth = type == PopupType.LargeSettings
                                    ? LargeSettingsWidth
                                    : (type == PopupType.Full) ? Window.Current.Bounds.Width : SettingsWidth;
            
            var settingsPopup = new Popup();
            settingsPopup.Closed += this.OnPopupClosed;
            Window.Current.Activated += this.OnWindowActivated;
            settingsPopup.IsLightDismissEnabled = type != PopupType.Full;
            settingsPopup.Width = settingsWidth;
            settingsPopup.Height = Window.Current.Bounds.Height;

            view.Height = settingsPopup.Height;
            view.Width = settingsPopup.Width;

            settingsPopup.Child = view;
            settingsPopup.SetValue(Canvas.LeftProperty, Window.Current.Bounds.Width - settingsWidth);
            settingsPopup.SetValue(Canvas.TopProperty, 0);
            settingsPopup.IsOpen = true;

            this.settingsPopups.Add(settingsPopup, type);
        }

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                foreach (var settingsPopup in this.settingsPopups)
                {
                    if (settingsPopup.Value != PopupType.Full)
                    {
                        settingsPopup.Key.IsOpen = false;
                    }
                }
            }
        }

        private void OnPopupClosed(object sender, object e)
        {
            var popup = sender as Popup;
            if (popup != null)
            {
                Window.Current.Activated -= this.OnWindowActivated;
                popup.Closed -= this.OnPopupClosed;
                this.settingsPopups.Remove(popup);
            }
        }
    }
}