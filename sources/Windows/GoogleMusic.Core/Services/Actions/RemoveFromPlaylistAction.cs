﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------


namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Views;

    public class RemoveFromPlaylistAction : ISelectedObjectAction
    {
        private readonly INavigationService navigationService;
        private readonly IUserPlaylistsService userPlaylistsService;

        public RemoveFromPlaylistAction(
            INavigationService navigationService,
            IUserPlaylistsService userPlaylistsService)
        {
            this.navigationService = navigationService;
            this.userPlaylistsService = userPlaylistsService;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.Remove;
            }
        }

        public string Title
        {
            get
            {
                return "Remove from playlist";
            }
        }

        public ActionGroup Group
        {
            get
            {
                return ActionGroup.Playlist;
            }
        }

        public int Priority
        {
            get
            {
                return 500;
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            IPageView currentView = this.navigationService.GetCurrentView();
            if (!(currentView is IPlaylistPageView))
            {
                return false;
            }

            var presenter = currentView.GetPresenter<BindingModelBase>();
            var playlistPageViewPresenter = presenter as PlaylistPageViewPresenter;
            if (playlistPageViewPresenter == null)
            {
                return false;
            }

            var bindingModel = playlistPageViewPresenter.BindingModel;

            if (bindingModel == null)
            {
                return false;
            }

            var playlist = bindingModel.Playlist;
            return playlist is UserPlaylist && !((UserPlaylist)playlist).IsShared;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            if (!this.CanExecute(selectedObjects))
            {
                return null;
            }

            var userPlaylist = (UserPlaylist)this.navigationService.GetCurrentView().GetPresenter<PlaylistPageViewPresenter>().BindingModel.Playlist;
            return await this.userPlaylistsService.RemoveSongsAsync(userPlaylist, selectedObjects.Cast<Song>());
        }
    }
}
