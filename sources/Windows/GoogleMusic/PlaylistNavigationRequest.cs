﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistNavigationRequest
    {
        public int PlaylistId { get; set; }

        public SongsContainerType SongsContainerType { get; set; } 
    }
}