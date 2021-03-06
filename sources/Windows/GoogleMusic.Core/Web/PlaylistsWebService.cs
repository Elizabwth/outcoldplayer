﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IPlaylistsWebService
    {
        Task<IList<GoogleMusicPlaylist>> GetAllAsync(DateTime? lastUpdate, IProgress<int> progress = null, Func<IList<GoogleMusicPlaylist>, Task> chunkHandler = null);

        Task<IList<GoogleMusicPlaylistEntry>> GetAllPlaylistEntriesAsync(DateTime? lastUpdate, IProgress<int> progress = null, Func<IList<GoogleMusicPlaylistEntry>, Task> chunkHandler = null);

        Task<GoogleMusicPlaylistBatchResponse> CreateAsync(string name);

        Task<GoogleMusicPlaylistBatchResponse> DeleteAsync(IList<UserPlaylist> playlists);

        Task<GoogleMusicPlaylistBatchResponse> ChangeNameAsync(string id, string name);

        Task<GoogleMusicPlaylistEntriesBatchResponse> AddSongsAsync(UserPlaylist userPlaylist, IDictionary<string, Song> songs);

        Task<GoogleMusicPlaylistEntriesBatchResponse> RemoveSongsAsync(
            UserPlaylist playlist,
            IList<UserPlaylistEntry> entries);

        Task<GoogleMusicSharedPlaylistEntriesResponse> GetAllPlaylistEntriesSharedAsync(
            IList<UserPlaylist> sharedPlaylists,
            DateTime? lastUpdate = null);

        Task<IList<GoogleMusicSong>> GetHighlyRatedSongsAsync();
    }

    public class PlaylistsWebService : IPlaylistsWebService
    {
        private const string PlaylistFeed = "playlistfeed";
        private const string PlEntryFeed = "plentryfeed";
        private const string PlaylistBatch = "playlistbatch";
        private const string PlEntriesBatch = "plentriesbatch";
        private const string PlEntriesShared = "plentries/shared";
        private const string EphemeralTop = "ephemeral/top";

        private readonly IGoogleMusicApisService googleMusicApisService;

        public PlaylistsWebService(
            IGoogleMusicApisService googleMusicApisService)
        {
            this.googleMusicApisService = googleMusicApisService;
        }

        public Task<IList<GoogleMusicPlaylist>> GetAllAsync(DateTime? lastUpdate, IProgress<int> progress = null, Func<IList<GoogleMusicPlaylist>, Task> chunkHandler = null)
        {
            return this.googleMusicApisService.DownloadList(PlaylistFeed, lastUpdate, progress, chunkHandler);
        }

        public Task<IList<GoogleMusicPlaylistEntry>> GetAllPlaylistEntriesAsync(DateTime? lastUpdate, IProgress<int> progress = null, Func<IList<GoogleMusicPlaylistEntry>, Task> chunkHandler = null)
        {
            return this.googleMusicApisService.DownloadList(PlEntryFeed, lastUpdate, progress, chunkHandler);
        }

        public Task<GoogleMusicSharedPlaylistEntriesResponse> GetAllPlaylistEntriesSharedAsync(IList<UserPlaylist> sharedPlaylists, DateTime? lastUpdate = null)
        {
            var json = new
                       {
                           entries = sharedPlaylists.Select(x => 
                                         new
                                         {
                                             maxResults = 20000,
                                             shareToken = x.ShareToken,
                                             updatedMin = lastUpdate.HasValue ? ((ulong)lastUpdate.Value.ToUnixFileTime() * 1000L).ToString("G", CultureInfo.InvariantCulture) : 0.ToString()
                                         }),
                           includeDeleted = false
                       };

            return this.googleMusicApisService.PostAsync<GoogleMusicSharedPlaylistEntriesResponse>(PlEntriesShared, json, useCache: true);
        }

        public Task<IList<GoogleMusicSong>> GetHighlyRatedSongsAsync()
        {
            return this.googleMusicApisService.DownloadList<GoogleMusicSong>(EphemeralTop);
        }

        public async Task<GoogleMusicPlaylistBatchResponse> CreateAsync(string name)
        {
            var json = new
                       {
                           mutations = new []
                                       {
                                           new
                                           {
                                               create = new
                                                        {
                                                            creationTimestamp = "-1",
                                                            deleted = false,
                                                            lastModifiedTimestamp = "0",
                                                            name = name,
                                                            type = "USER_GENERATED"
                                                        }
                                           }
                                       }
                       };

            return await this.googleMusicApisService.PostAsync<GoogleMusicPlaylistBatchResponse>(PlaylistBatch, json);
        }

        public async Task<GoogleMusicPlaylistBatchResponse> DeleteAsync(IList<UserPlaylist> playlists)
        {
            var json = new
            {
                mutations = playlists.Select(x => new
                {
                    delete = x.Id
                }).ToArray()
            };

            return await this.googleMusicApisService.PostAsync<GoogleMusicPlaylistBatchResponse>(PlaylistBatch, json);
        }

        public async Task<GoogleMusicPlaylistBatchResponse> ChangeNameAsync(string id, string name)
        {
            var json = new
                {
                    mutations = new[]
                                           {
                                               new
                                               {
                                                   update = new
                                                            {
                                                                id = id,
                                                                name = name
                                                            }
                                               }
                                           }
                };

            return await this.googleMusicApisService.PostAsync<GoogleMusicPlaylistBatchResponse>(PlaylistBatch, json);
        }

        public async Task<GoogleMusicPlaylistEntriesBatchResponse> AddSongsAsync(UserPlaylist userPlaylist, IDictionary<string, Song> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            var json = new
                {
                    mutations = songs.Select(song => 
                                               new
                                               {
                                                   create = new
                                                            {
                                                                clientId = song.Key,
                                                                creationTimestamp = "-1",
                                                                deleted = false,
                                                                lastModifiedTimestamp = "0",
                                                                playlistId = userPlaylist.PlaylistId,
                                                                source = string.Equals(song.Value.StoreId, song.Value.SongId, StringComparison.OrdinalIgnoreCase) ? 2 : 1,
                                                                trackId = song.Value.SongId
                                                            }
                                               }
                                           ).ToArray()
                };

            return await this.googleMusicApisService.PostAsync<GoogleMusicPlaylistEntriesBatchResponse>(PlEntriesBatch, json);
        }

        public async Task<GoogleMusicPlaylistEntriesBatchResponse> RemoveSongsAsync(UserPlaylist playlist, IList<UserPlaylistEntry> entries)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            var json = new
                       {
                           mutations = entries.Select(x => new
                                                           {
                                                               delete = x.Id
                                                           }).ToArray()
                       };

            return await this.googleMusicApisService.PostAsync<GoogleMusicPlaylistEntriesBatchResponse>(PlEntriesBatch, json);
        }
    }
}