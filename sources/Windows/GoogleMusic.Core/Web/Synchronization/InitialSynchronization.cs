﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;

    public interface IInitialSynchronization
    {
        Task InitializeAsync(IProgress<double> progress = null);
    }

    public class InitialSynchronization : IInitialSynchronization
    {
        private readonly ILogger logger;

        private readonly IGoogleMusicSynchronizationService synchronizationService;

        private readonly ISongsCachingService songsCachingService;

        private readonly IAlbumArtCacheService albumArtCacheService;

        private readonly DbContext dbContext;

        public InitialSynchronization(
            ILogManager logManager,
            IGoogleMusicSynchronizationService synchronizationService,
            ISongsCachingService songsCachingService,
            IAlbumArtCacheService albumArtCacheService)
        {
            this.dbContext = new DbContext();
            this.logger = logManager.CreateLogger("InitialSynchronization");
            this.synchronizationService = synchronizationService;
            this.songsCachingService = songsCachingService;
            this.albumArtCacheService = albumArtCacheService;
        }

        public async Task InitializeAsync(IProgress<double> progress)
        {
            Exception exception = null;

            try
            {
                try
                {
                    await this.albumArtCacheService.ClearCacheAsync();
                }
                catch (Exception e)
                {
                    this.logger.Debug(e, "Could not clear the cache, possible that tables are empty");
                }

                await this.synchronizationService.Update(progress);

                await this.songsCachingService.RestoreCacheAsync();
            }
            catch (Exception e)
            {
                this.logger.Debug(e, "Initialization failed");
                exception = e;
            }

            if (exception != null)
            {
                try
                {
                    await this.dbContext.DeleteDatabaseAsync();
                }
                catch (Exception e)
                {
                    this.logger.Error(e, "Could not drop database after initialization failed");
                }
                
                throw exception;
            }
        }
    }
}
