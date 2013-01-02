﻿//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Presenters.Settings;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Settings;
    using OutcoldSolutions.GoogleMusic.WebServices;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Core;
    using Windows.UI.Xaml;

    public sealed partial class App : Application
    {
        private ILogManager logManager;

        private ISettingsService settingsService;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        public static IDependencyResolverContainer Container { get; private set; }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainView mainView = Window.Current.Content as MainView;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (mainView == null)
            {
                Container = new DependencyResolverContainer();

                using (var registration = Container.Registration())
                {
                    registration.Register<ILogManager>().AsSingleton<LogManager>();

                    registration.Register<IMainView>().And<IMediaElemenetContainerView>().And<ICurrentContextCommands>().AsSingleton<MainView>();
                    registration.Register<INavigationService>().And<MainViewPresenter>().AsSingleton<MainViewPresenter>();

                    registration.Register<IAuthentificationView>().As<AuthentificationView>();
                    registration.Register<AuthentificationPresenter>();

                    registration.Register<IStartView>().AsSingleton<StartView>();
                    registration.Register<StartViewPresenter>().AsSingleton();

                    registration.Register<IPlaylistsView>().AsSingleton<PlaylistsView>();
                    registration.Register<PlaylistsViewPresenter>().AsSingleton();

                    registration.Register<IPlaylistView>().AsSingleton<PlaylistView>();
                    registration.Register<PlaylistViewPresenter>().AsSingleton();

                    registration.Register<ICurrentPlaylistView>().AsSingleton<CurrentPlaylistView>();
                    registration.Register<CurrentPlaylistViewPresenter>().AsSingleton();

                    registration.Register<IProgressLoadingView>().As<ProgressLoadingView>();
                    registration.Register<ProgressLoadingPresenter>();

                    registration.Register<ICurrentPlaylistService>().And<PlayerViewPresenter>().AsSingleton<PlayerViewPresenter>();

                    // Settings
                    registration.Register<ISettingsCommands>().AsSingleton<SettingsCommands>();

                    // Settings views
                    registration.Register<AccountView>();
                    registration.Register<AccountViewPresenter>();

                    // Services
                    registration.Register<IClientLoginService>().As<ClientLoginService>();
                    registration.Register<IGoogleWebService>().AsSingleton<GoogleWebService>();
                    registration.Register<IUserDataStorage>().AsSingleton<UserDataStorage>();
                    registration.Register<IAuthentificationService>().As<AuthentificationService>();
                    registration.Register<IPlaylistsWebService>().As<PlaylistsWebService>();
                    registration.Register<ISongWebService>().AsSingleton<SongWebService>();
                    registration.Register<ISongsService>().AsSingleton<SongsService>();
                    registration.Register<ISettingsService>().AsSingleton<SettingsService>();

                    registration.Register<IDispatcher>().AsSingleton(new DispatcherContainer(CoreWindow.GetForCurrentThread().Dispatcher));
                }

                this.logManager = Container.Resolve<ILogManager>();
                this.logManager.AddWriter(new FileLogWriter());

                this.settingsService = Container.Resolve<ISettingsService>();

                this.UpdateLogLevel();
                this.settingsService.ValueChanged += (sender, eventArgs) =>
                    {
                        if (string.Equals(eventArgs.Key, "IsLoggingOn", StringComparison.OrdinalIgnoreCase))
                        {
                            Task.Factory.StartNew(this.UpdateLogLevel);
                        }
                    };

                // Create a Frame to act as the navigation context and navigate to the first page
                mainView = (MainView)Container.Resolve<IMainView>();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }
                
                // Place the frame in the current Window
                Window.Current.Content = mainView;

                // Initialize settings view
                Container.Resolve<ISettingsCommands>();
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void UpdateLogLevel()
        {
            this.logManager.LogLevel = this.settingsService.GetValue("IsLoggingOn", defaultValue: false) ? LogLevel.Error : LogLevel.None;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
