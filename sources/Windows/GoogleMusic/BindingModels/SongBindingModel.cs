﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public class SongBindingModel : BindingModelBase
    {
        private readonly GoogleMusicSong song;
        private bool isPlaying = false;
        private bool isSelected = false;
        private int index = 0;

        public SongBindingModel(GoogleMusicSong song)
        {
            this.song = song;
        }

        public string Title
        {
            get
            {
                return this.song.Title;
            }
        }

        public string Artist
        {
            get
            {
                return this.song.Artist;
            }
        }

        public string Album
        {
            get
            {
                return this.song.Album;
            }
        }

        public int Plays
        {
            get
            {
                return this.song.PlayCount;
            }
        }

        public string Time
        {
            get
            {
                var timeSpan = TimeSpan.FromMilliseconds(this.song.DurationMillis);
                return string.Format("{0:N0}:{1:00}", timeSpan.TotalMinutes, timeSpan.Seconds);
            }
        }

        public int Raiting
        {
            get
            {
                return this.song.Rating;
            }
        }

        public ImageSource AlbumArt
        {
            get
            {
                if (this.song.AlbumArtUrl != null)
                {
                    // TODO: Load only 40x40 image
                    return new BitmapImage(new Uri("https:" + this.song.AlbumArtUrl));
                }

                // TODO: Some default image
                return null;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return this.isPlaying;
            }
            
            set
            {
                if (this.isPlaying != value)
                {
                    this.isPlaying = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }

            set
            {
                if (this.index != value)
                {
                    this.index = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public GoogleMusicSong GetSong()
        {
            return this.song;
        }
    }
}