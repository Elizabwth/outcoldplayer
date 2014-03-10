﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    internal class AddToLibraryAction : ISelectedObjectAction
    {
        private readonly IApplicationStateService stateService;

        private readonly IPlaylistsService playlistsService;

        private readonly ISongsService songsService;

        public AddToLibraryAction(
            IApplicationStateService stateService,
            IPlaylistsService playlistsService,
            ISongsService songsService)
        {
            this.stateService = stateService;
            this.playlistsService = playlistsService;
            this.songsService = songsService;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.Add;
            }
        }

        public string Title
        {
            get
            {
                return "Library";
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            if (!this.stateService.IsOnline())
            {
                return false;
            }

            bool hasNotLibrary = false;
            foreach (var obj in selectedObjects)
            {
                var song = obj as Song;
                if (song != null)
                {
                    if (!song.IsLibrary)
                    {
                        hasNotLibrary = true;
                    }
                }
                else
                {
                    var playlist = (IPlaylist)obj;
                    if (playlist.PlaylistType == PlaylistType.Radio)
                    {
                        return false;
                    }

                    hasNotLibrary |= string.IsNullOrEmpty(playlist.Id) || playlist is UserPlaylist;
                }
            }

            return hasNotLibrary;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            List<Song> songs = new List<Song>();

            foreach (var obj in selectedObjects)
            {
                var song = obj as Song;
                if (song != null)
                {
                    if (!song.IsLibrary)
                    {
                        songs.Add(song);
                    }
                }
                else
                {
                    var playlist = (IPlaylist)obj;
                    if (string.IsNullOrEmpty(playlist.Id))
                    {
                        songs.AddRange((await this.playlistsService.GetSongsAsync((IPlaylist)obj)).Where(x => !x.IsLibrary));
                    }
                }
            }

            IList<Song> addedSongs = await this.songsService.AddToLibraryAsync(songs);

            return addedSongs != null;
        }
    }
}
