﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;

    public class StartRadioAction : ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;
        private readonly IApplicationStateService stateService;
        private readonly ISettingsService settingsService;
        private readonly IRadioStationsService radioStationsService;
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;

        public StartRadioAction(
            IApplicationResources applicationResources,
            IApplicationStateService stateService,
            ISettingsService settingsService,
            IRadioStationsService radioStationsService,
            IPlayQueueService playQueueService,
            INavigationService navigationService)
        {
            this.applicationResources = applicationResources;
            this.stateService = stateService;
            this.settingsService = settingsService;
            this.radioStationsService = radioStationsService;
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.MusicInfo;
            }
        }

        public string Title
        {
            get
            {
                return this.settingsService.GetIsAllAccessAvailable()
                    ? this.applicationResources.GetString("Toolbar_StartRadio")
                    : this.applicationResources.GetString("Toolbar_StartInstantMix");
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            if (!this.stateService.IsOnline())
            {
                return false;
            }

            if (selectedObjects.Count != 1)
            {
                return false;
            }

            return selectedObjects[0] is Song ||
                selectedObjects[0] is Album ||
                selectedObjects[0] is Artist;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            if (!this.CanExecute(selectedObjects))
            {
                return null;
            }

            Tuple<Radio, IList<Song>> radio = null;

            object selectedObject = selectedObjects[0];
            if (selectedObject is Song)
            {
                radio = await this.radioStationsService.CreateAsync((Song)selectedObject);
            }

            if (selectedObject is Album)
            {
                radio = await this.radioStationsService.CreateAsync((Album)selectedObject);
            }

            if (selectedObject is Artist)
            {
                radio = await this.radioStationsService.CreateAsync((Artist)selectedObject);
            }

            if (radio != null)
            {
                await this.playQueueService.PlayAsync(radio.Item1, radio.Item2, -1);
                this.navigationService.NavigateTo<ICurrentPlaylistPageView>();
            }

            return radio != null;
        }
    }
}
