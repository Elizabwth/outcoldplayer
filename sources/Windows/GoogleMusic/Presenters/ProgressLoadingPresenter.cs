﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Web;

    using Windows.System;
    using Windows.UI.Core;
    using Windows.UI.Popups;

    public class ProgressLoadingPresenter : ViewPresenterBase<IView>
    {
        private const int AskForReviewStarts = 10;
        private const string DoNotAskToReviewKey = "DoNotAskToReviewKey";
        private const string CountOfStartsBeforeReview = "CountOfStartsBeforeReview";

        private const string CurrentVersion = "1.2.1";

        private readonly ISongsService songsService;
        private readonly IPlaylistsWebService playlistsWebService;

        private readonly ISongWebService songWebService;

        private readonly INavigationService navigationService;

        private readonly ISearchService searchService;

        private readonly ISettingsService settingsService;

        public ProgressLoadingPresenter(
            IDependencyResolverContainer container, 
            IView view,
            ISongsService songsService,
            INavigationService navigationService,
            ISearchService searchService,
            ISettingsService settingsService,
            IPlaylistsWebService playlistsWebService,
            ISongWebService songWebService)
            : base(container, view)
        {
            this.songsService = songsService;
            this.navigationService = navigationService;
            this.searchService = searchService;
            this.settingsService = settingsService;
            this.playlistsWebService = playlistsWebService;
            this.songWebService = songWebService;
            this.BindingModel = new ProgressLoadingBindingModel();
        }

        public ProgressLoadingBindingModel BindingModel { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            this.LoadSongs();
        }

        public void LoadSongs()
        {
            this.BindingModel.Progress = 0;
            this.BindingModel.Message = "Initializing...";
            this.BindingModel.IsFailed = false;

            this.songWebService.GetStatusAsync().ContinueWith(
                tStatus =>
                {
                    if (tStatus.IsCompleted && !tStatus.IsFaulted)
                    {
                        this.BindingModel.Maximum = tStatus.Result.AvailableTracks * 2;
                        this.BindingModel.Message = "Loading playlists...";

                        this.songsService.GetAllPlaylistsAsync().ContinueWith(
                            tPlaylists =>
                            {
                                this.BindingModel.Progress = tStatus.Result.AvailableTracks;

                                if (tPlaylists.IsCompleted && !tPlaylists.IsFaulted)
                                {
                                    this.BindingModel.Message = "Loading songs...";
                                    Progress<int> progress = new Progress<int>();
                                    progress.ProgressChanged += async (sender, i) =>
                                        {
                                            await this.Dispatcher.RunAsync(
                                                CoreDispatcherPriority.High,
                                                () =>
                                                    {
                                                        this.BindingModel.Progress = tStatus.Result.AvailableTracks + i;
                                                    });
                                        };

                                    this.songsService.GetAllGoogleSongsAsync(progress).ContinueWith(
                                        tSongs =>
                                        {
                                            if (tSongs.IsCompleted && !tSongs.IsFaulted)
                                            {
                                                bool dontAsk = this.settingsService.GetRoamingValue<bool>(DoNotAskToReviewKey);
                                                if (!dontAsk)
                                                {
                                                    int startsCount = this.settingsService.GetRoamingValue<int>(CountOfStartsBeforeReview);
                                                    if (startsCount >= AskForReviewStarts)
                                                    {
                                                        var dialog = new MessageDialog("If you are enjoy using gMusic appication, would you mind taking a moment to rate it? Good ratings help us a lot. It won't take more than a minute. Thanks for your support!");
                                                        dialog.Commands.Add(
                                                            new UICommand(
                                                                "Rate",
                                                                (cmd) =>
                                                                {
                                                                    this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true);
                                                                    var tLauncher = Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=47286outcoldman.gMusic_z1q2m7teapq4y"));
                                                                }));
                                                        dialog.Commands.Add(
                                                            new UICommand(
                                                                "No, thanks",
                                                                (cmd) => this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true)));
                                                        dialog.Commands.Add(
                                                            new UICommand(
                                                                "Remind me later",
                                                                (cmd) => this.settingsService.SetRoamingValue<int>(CountOfStartsBeforeReview, 0)));

                                                        var tResult = dialog.ShowAsync();
                                                    }
                                                    else
                                                    {
                                                        this.settingsService.SetRoamingValue<int>(CountOfStartsBeforeReview, startsCount + 1);
                                                    }
                                                }

                                                bool forceToShowUpdates = false;
                                                if (this.settingsService.GetRoamingValue<bool>("VersionHistory v1.1", defaultValue: false))
                                                {
                                                    forceToShowUpdates = true;
                                                    this.settingsService.RemoveRoamingValue("VersionHistory v1.1");
                                                }

                                                if (string.Equals(
                                                        this.settingsService.GetValue<string>("Version", CurrentVersion),
                                                        CurrentVersion,
                                                        StringComparison.OrdinalIgnoreCase) || !forceToShowUpdates)
                                                {
                                                    this.searchService.Register();
                                                    this.settingsService.SetValue("Version", CurrentVersion);
                                                    this.navigationService.NavigateTo<IStartView>();
                                                }
                                                else
                                                {
                                                    this.settingsService.SetValue("Version", CurrentVersion);
                                                    this.navigationService.NavigateTo<IWhatIsNewView>(keepInHistory: false);
                                                }
                                            }
                                            else
                                            {
                                                this.BindingModel.Message = "Cannot load data...";
                                                this.BindingModel.IsFailed = true;
                                            }
                                        },
                                        TaskScheduler.FromCurrentSynchronizationContext());
                                }
                                else
                                {
                                    this.BindingModel.Message = "Cannot load data...";
                                    this.BindingModel.IsFailed = true;
                                }
                            },
                            TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else
                    {
                        this.BindingModel.Message = "Cannot load data...";
                        this.BindingModel.IsFailed = true;
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}