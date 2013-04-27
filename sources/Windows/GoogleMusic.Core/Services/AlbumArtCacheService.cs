﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    using Windows.Storage;

    public class AlbumArtCacheService : IAlbumArtCacheService
    {
        private const string AlbumArtCacheFolder = "AlbumArtCache";

        private readonly ILogger logger;
        private readonly ICachedAlbumArtsRepository cachedAlbumArtsRepository;

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(2);
        private readonly ConcurrentDictionary<CachedKey, Task<CachedAlbumArt>> downloadTasks = new ConcurrentDictionary<CachedKey, Task<CachedAlbumArt>>();
        private readonly HttpClient httpClient = new HttpClient();
        private StorageFolder cacheFolder;

        public AlbumArtCacheService(
            ILogManager logManager, 
            ICachedAlbumArtsRepository cachedAlbumArtsRepository)
        {
            this.logger = logManager.CreateLogger("AlbumArtCacheService");
            this.cachedAlbumArtsRepository = cachedAlbumArtsRepository;
        }

        public async Task<string> GetCachedImageAsync(Uri url, uint size)
        {
            if (this.cacheFolder == null)
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                this.cacheFolder = await localFolder.CreateFolderAsync(AlbumArtCacheFolder, CreationCollisionOption.OpenIfExists);
                this.DeleteRemovedItems();
            }

            CachedAlbumArt cache = await this.cachedAlbumArtsRepository.FindAsync(url, size);
            if (cache == null)
            {
                await this.semaphoreSlim.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

                try
                {
                    var cachedKey = new CachedKey(url, size);
                    cache = await this.downloadTasks.GetOrAdd(
                        cachedKey, 
                        uri => Task.Run(
                            async () =>
                                {
                                    CachedAlbumArt downloadedCache = await this.cachedAlbumArtsRepository.FindAsync(url, size);
                                    if (downloadedCache == null)
                                    {
                                        string fileName = Guid.NewGuid().ToString() + ".jpg";
                                        string subFolderName = fileName.Substring(0, 1);

                                        var folder = await this.cacheFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.OpenIfExists);
                                        var file = await folder.CreateFileAsync(fileName);

                                        using (var imageStream = await this.httpClient.GetStreamAsync(url.ChangeSize(size)))
                                        {
                                            var targetStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                                            using (Stream fileStream = targetStream.AsStreamForWrite())
                                            {
                                                await imageStream.CopyToAsync(fileStream);
                                                await fileStream.FlushAsync();
                                            }
                                        }

                                        downloadedCache = new CachedAlbumArt() { AlbumArtUrl = url, Size = size, Path = Path.Combine(subFolderName, fileName) };

                                        await this.cachedAlbumArtsRepository.AddAsync(downloadedCache);
                                    }

                                    return downloadedCache;
                                }));

                    Task<CachedAlbumArt> task;
                    this.downloadTasks.TryRemove(cachedKey, out task);
                }
                finally
                {
                    this.semaphoreSlim.Release();
                }
            }

            return Path.Combine(AlbumArtCacheFolder, cache.Path);
        }

        private async void DeleteRemovedItems()
        {
            var removedItems = await this.cachedAlbumArtsRepository.GetRemovedCachedItemsAsync();
            foreach (var cache in removedItems)
            {
                var file = await StorageFile.GetFileFromPathAsync(Path.Combine(AlbumArtCacheFolder, cache.Path));
                if (file != null)
                {
                    try
                    {
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch (Exception e)
                    {
                        this.logger.Warning(e, "DeleteRemovedItems: Could not delete cached image {0}.", file.Path);
                    }
                }
            }

            try
            {
                await this.cachedAlbumArtsRepository.DeleteCachedItemsAsync(removedItems);
            }
            catch (Exception e)
            {
                this.logger.Error(e, "DeleteCachedItemsAsync failed.");
            }
        }

        private struct CachedKey : IEquatable<CachedKey>
        {
            public CachedKey(Uri albumArtUrl, uint size)
                : this()
            {
                this.AlbumArtUrl = albumArtUrl;
                this.Size = size;
            }

            public Uri AlbumArtUrl { get; set; }

            public uint Size { get; set; }

            public bool Equals(CachedKey other)
            {
                return object.Equals(this.AlbumArtUrl, other.AlbumArtUrl) && this.Size == other.Size;
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is CachedKey && this.Equals((CachedKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((this.AlbumArtUrl != null ? this.AlbumArtUrl.GetHashCode() : 0) * 397) ^ (int)this.Size;
                }
            }
        }
    }
}