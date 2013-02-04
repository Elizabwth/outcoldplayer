﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class PlaylistsWebService : IPlaylistsWebService
    {
        private const string PlaylistsUrl = "music/services/loadplaylist";
        private const string AddPlaylistUrl = "music/services/addplaylist";
        private const string DeletePlaylistUrl = "music/services/deleteplaylist";
        private const string ChangePlaylistNameUrl = "music/services/modifyplaylist";
        private const string AddToPlaylistUrl = "music/services/addtoplaylist";
        private const string DeleteSongUrl = "music/services/deletesong";

        private readonly IGoogleMusicWebService googleMusicWebService;

        public PlaylistsWebService(
            IGoogleMusicWebService googleMusicWebService)
        {
            this.googleMusicWebService = googleMusicWebService;
        }

        public async Task<GoogleMusicPlaylists> GetAllAsync()
        {
            return await this.googleMusicWebService.PostAsync<GoogleMusicPlaylists>(PlaylistsUrl);
        }

        public async Task<GoogleMusicPlaylist> GetAsync(Guid playlistId)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { 
                                                "id", JsonConvert.ToString(playlistId)
                                            }
                                        };

            return await this.googleMusicWebService.PostAsync<GoogleMusicPlaylist>(PlaylistsUrl, jsonProperties: jsonProperties);
        }

        public async Task<AddPlaylistResp> CreateAsync(string name)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "title", JsonConvert.ToString(name) },
                                            { "playlistType", JsonConvert.ToString("USER_GENERATED") },
                                            { "songRefs", JsonConvert.SerializeObject(new string[] { }) }
                                        };

            return await this.googleMusicWebService.PostAsync<AddPlaylistResp>(AddPlaylistUrl, jsonProperties: jsonProperties);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "id", JsonConvert.ToString(id) },
                                            { "requestType", JsonConvert.ToString(1) },
                                            { "requestCause", JsonConvert.ToString(1) }
                                        };

            var deletePlaylistResp = await this.googleMusicWebService.PostAsync<DeletePlaylistResp>(DeletePlaylistUrl, jsonProperties: jsonProperties);

            return deletePlaylistResp.DeleteId == id;
        }

        public async Task<bool> ChangeNameAsync(Guid id, string name)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "playlistId", JsonConvert.ToString(id) },
                                            { "playlistName", JsonConvert.ToString(name) }
                                        };

            var response = await this.googleMusicWebService.PostAsync<CommonResponse>(ChangePlaylistNameUrl, jsonProperties: jsonProperties);
            return !response.Success.HasValue || response.Success.Value;
        }

        public async Task<AddSongResp> AddSongAsync(Guid playlistId, Guid songId)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "playlistId", JsonConvert.ToString(playlistId) },
                                            { "songRefs", JsonConvert.SerializeObject(new[] { new { id = songId, type = 1 } }) }
                                        };

            return await this.googleMusicWebService.PostAsync<AddSongResp>(AddToPlaylistUrl, jsonProperties: jsonProperties);
        }

        public async Task<bool> RemoveSongAsync(Guid playlistId, Guid songId, Guid entryId)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "listId", JsonConvert.ToString(playlistId) },
                                            { "songIds", JsonConvert.SerializeObject(new[] { songId }) },
                                            { "entryIds", JsonConvert.SerializeObject(new[] { entryId }) }
                                        };

            var response = await this.googleMusicWebService.PostAsync<CommonResponse>(DeleteSongUrl, jsonProperties: jsonProperties);
            return !response.Success.HasValue || response.Success.Value;
        }
    }
}