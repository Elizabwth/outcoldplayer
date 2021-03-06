﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistNavigationRequest
    {
        public PlaylistNavigationRequest(IPlaylist playlist)
        {
            this.PlaylistType = playlist.PlaylistType;
            this.PlaylistId = playlist.Id;
            this.Playlist = playlist;
        }

        public PlaylistNavigationRequest(IPlaylist playlist, IList<Song> songs)
        {
            if (playlist != null)
            {
                this.PlaylistType = playlist.PlaylistType;
                this.PlaylistId = playlist.Id;
                this.Playlist = playlist;
            }

            this.Songs = songs;
        }

        public PlaylistNavigationRequest(IPlaylist playlist, string title, string subtitle, IList<Song> songs)
        {
            if (playlist != null)
            {
                this.PlaylistType = playlist.PlaylistType;
                this.PlaylistId = playlist.Id;
                this.Playlist = playlist;
            }

            this.Title = title;
            this.Subtitle = subtitle;
            this.Songs = songs;
        }

        public PlaylistNavigationRequest(IPlaylist playlist, string title, string subtitle, IList<IPlaylist> playlists)
        {
            if (playlist != null)
            {
                this.PlaylistType = playlist.PlaylistType;
                this.PlaylistId = playlist.Id;
                this.Playlist = playlist;
            }

            this.Title = title;
            this.Subtitle = subtitle;
            this.Playlists = playlists;
        }

        public PlaylistNavigationRequest(string title, string subtitle, IList<Song> songs)
        {
            this.Title = title;
            this.Subtitle = subtitle;
            this.Songs = songs;
        }

        public PlaylistNavigationRequest(string title, string subtitle, IList<IPlaylist> playlists)
        {
            this.Title = title;
            this.Subtitle = subtitle;
            this.Playlists = playlists;
        }

        public IPlaylist Playlist { get; set; }

        public PlaylistType PlaylistType { get; set; }

        public string PlaylistId { get; set; }

        public IList<Song> Songs { get; set; }

        public IList<IPlaylist> Playlists { get; set; } 

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public bool ForceToShowAllAccess { get; set; }

        public bool ForceToPlay { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((PlaylistNavigationRequest)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)this.PlaylistType * 397) ^ (this.PlaylistId != null ? this.PlaylistId.GetHashCode() : 0);
            }
        }

        protected bool Equals(PlaylistNavigationRequest other)
        {
            bool result = this.PlaylistType == other.PlaylistType && this.PlaylistId != null && other.PlaylistId != null
                   && string.Equals(this.PlaylistId, other.PlaylistId, StringComparison.OrdinalIgnoreCase)
                   && this.Songs == null && other.Songs == null
                   && this.Playlists == null && other.Playlists == null;

            if (result && string.IsNullOrEmpty(this.PlaylistId) && this.Playlist != null)
            {
                if (this.PlaylistType == PlaylistType.Album)
                {
                    result = string.Equals(
                        ((Album)this.Playlist).GoogleAlbumId,
                        ((Album)other.Playlist).GoogleAlbumId,
                        StringComparison.OrdinalIgnoreCase);
                }
                else if (this.PlaylistType == PlaylistType.Artist)
                {
                    result = string.Equals(
                       ((Artist)this.Playlist).GoogleArtistId,
                       ((Artist)other.Playlist).GoogleArtistId,
                       StringComparison.OrdinalIgnoreCase);
                }
            }

            return result;
        }
    }
}