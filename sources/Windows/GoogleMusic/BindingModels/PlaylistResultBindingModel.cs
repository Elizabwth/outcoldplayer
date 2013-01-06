﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Globalization;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistResultBindingModel : SearchResultBindingModel
    {
        private readonly Playlist result;

        public PlaylistResultBindingModel(Playlist result)
        {
            this.result = result;
        }

        public Playlist Result
        {
            get
            {
                return this.result;
            }
        }

        public override string Title
        {
            get
            {
                return this.result.Title;
            }
        }

        public override string Subtitle
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} songs", this.result.Songs.Count);
            }
        }

        public override string Description
        {
            get
            {
                if (this.result is Album)
                {
                    return "Album";
                }

                if (this.result is Artist)
                {
                    return "Artist";
                }

                if (this.result is Genre)
                {
                    return "Genre";
                }

                if (this.result is MusicPlaylist)
                {
                    return "Playlist";
                }

                return null;
            }
        }

        public override string ImageUrl
        {
            get
            {
                return this.result.AlbumArtUrl;
            }
        }
    }
}