﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Globalization;
    using System.Text;

    using OutcoldSolutions.GoogleMusic.Models;

    public class SongBindingModel : BindingModelBase
    {
        private Song metadata;
        private SongState songState = SongState.None;

        public SongBindingModel(Song metadata)
        {
            this.metadata = metadata;
        }

        public string Title
        {
            get
            {
                return this.Metadata.Title;
            }
        }

        public double Duration
        {
            get
            {
                return this.Metadata.Duration.TotalSeconds;
            }
        }

        public string Artist
        {
            get
            {
                return string.IsNullOrEmpty(this.Metadata.ArtistTitle) ? 
                     string.IsNullOrEmpty(this.Metadata.AlbumArtistTitle) ?
                      "Unknown"
                     : this.Metadata.AlbumArtistTitle
                     : this.Metadata.ArtistTitle;
            }
        }

        public string Album
        {
            get
            {
                return string.IsNullOrEmpty(this.Metadata.AlbumTitle) ? "Unknown" : this.Metadata.AlbumTitle;
            }
        }

        public string ArtistAndAlbum
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Artist, Album);
            }
        }

        public uint PlayCount
        {
            get
            {
                return this.Metadata.PlayCount;
            }
        }

        public int Rating
        {
            get
            {
                return this.Metadata.Rating;
            }
        }

        public string Track
        {
            get
            {
                return !this.Metadata.Track.HasValue || this.Metadata.Track.Value == 0 ? string.Empty : this.Metadata.Track.Value.ToString();
            }
        }

        public bool IsCached
        {
            get
            {
                return this.Metadata.IsCached;
            }

            set
            {
                this.Metadata.IsCached = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public Song Metadata
        {
            get
            {
                return this.metadata;
            }

            set
            {
                this.metadata = value;
                this.RaiseAllPropertiesChanged();
            }
        }

        public SongState State
        {
            get
            {
                return this.songState;
            }

            set
            {
                this.songState = value;
                this.RaiseCurrentPropertyChanged();
                this.RaisePropertyChanged(() => this.IsPlaying);
            }
        }

        public bool IsPlaying
        {
            get
            {
                return this.songState == SongState.Playing || this.songState == SongState.Paused;
            }
        }

        public bool IsExplicit
        {
            get
            {
                return this.Metadata.IsExplicit();
            }
        }

        public bool IsDeleted
        {
            get
            {
                return this.Metadata.IsDeleted();
            }
        }

        public bool IsAllAccess
        {
            get
            {
                return this.Metadata.IsAllAccess();
            }
        }

        public void RaiseAllPropertiesChanged()
        {
            this.RaisePropertyChanged(() => this.Title);
            this.RaisePropertyChanged(() => this.Duration);
            this.RaisePropertyChanged(() => this.Artist);
            this.RaisePropertyChanged(() => this.Album);
            this.RaisePropertyChanged(() => this.PlayCount);
            this.RaisePropertyChanged(() => this.Rating);
            this.RaisePropertyChanged(() => this.IsExplicit);
            this.RaisePropertyChanged(() => this.IsDeleted);
            this.RaisePropertyChanged(() => this.IsAllAccess);
        }
    }
}