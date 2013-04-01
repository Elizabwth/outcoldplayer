﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public interface ISystemPlaylistsRepository : IPlaylistRepository<SystemPlaylist>
    {
        Task<SystemPlaylist> GetAsync(SystemPlaylistType systemPlaylistType);

        Task<IList<Song>> GetSongsAsync(SystemPlaylistType systemPlaylistType);

        Task<IList<SystemPlaylist>> GetAllAsync();
    }

    public class SystemPlaylistsRepository : RepositoryBase, ISystemPlaylistsRepository
    {
        private const int HighlyRatedValue = 4;
        private const int LastAddedSongsCount = 500;

        private const string SqlHiglyRatedSongsPlaylits = @"
select count(*) as SongsCount, sum(s.[Duration]) as Duration, ?2 as [SystemPlaylistType]
from [Song] as s
where s.[Rating] >= ?1 
";

        private const string SqlLastAddedPlaylist = @"
select count(*) as SongsCount, sum(x.[Duration]) as Duration, ?2 as [SystemPlaylistType] from 
(
  select *
  from [Song] as s  
  order by s.[CreationDate] desc
  limit ?1
) as x
";

        private const string SqlAllSongsPlaylist = @"
select count(*) as SongsCount, sum(s.[Duration]) as Duration, ?1 as [SystemPlaylistType] from [Song] as s
";

        private const string SqlAllSongs = @"
select s.* 
from [Song] as s
order by coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]), s.[AlbumTitleNorm], coalesce(nullif(s.Disc, 0), 1), s.Track
";

        private const string SqlHighlyRatedSongs = @"
select s.* 
from [Song] as s
where s.[Rating] >= ?1 
order by s.TitleNorm
";

        private const string SqlLastAddedSongs = @"
select *
from [Song] as x  
order by x.[CreationDate] desc
limit ?1
";

        public async Task<SystemPlaylist> GetHighlyRatedPlaylistAsync()
        {
            return (await this.Connection.QueryAsync<SystemPlaylist>(SqlHiglyRatedSongsPlaylits, HighlyRatedValue, SystemPlaylistType.HighlyRated)).First();
        }

        public async Task<SystemPlaylist> GetLastAddedSongsPlaylistAsync()
        {
            return (await this.Connection.QueryAsync<SystemPlaylist>(SqlLastAddedPlaylist, LastAddedSongsCount, SystemPlaylistType.LastAdded)).First();
        }

        public async Task<SystemPlaylist> GetAllSongsPlaylistAsync()
        {
            return (await this.Connection.QueryAsync<SystemPlaylist>(SqlAllSongsPlaylist, SystemPlaylistType.AllSongs)).First();
        }


        public Task<IList<Song>> GetSongsAsync(int id)
        {
            return this.GetSongsAsync((SystemPlaylistType)id);
        }

        public Task<IList<Song>> GetSongsAsync(SystemPlaylistType systemPlaylistType)
        {
            switch (systemPlaylistType)
            {
                case SystemPlaylistType.AllSongs:
                    return this.GetAllSongsAsync();
                case SystemPlaylistType.HighlyRated:
                    return this.GetHighlyRatedSongsAsync();
                case SystemPlaylistType.LastAdded:
                    return this.GetLastAddedSongsAsync();
                default:
                    throw new ArgumentOutOfRangeException("systemPlaylistType");
            }
        }

        public Task<IList<SystemPlaylist>> GetAllAsync()
        {
            return this.GetAllAsync(order: Order.None, take: null);
        }

        public Task<int> GetCountAsync()
        {
            return Task.FromResult(3);
        }

        public async Task<IList<SystemPlaylist>> GetAllAsync(Order order, uint? take = null)
        {
            return await Task.WhenAll(
                this.GetAllSongsPlaylistAsync(),
                this.GetHighlyRatedPlaylistAsync(),
                this.GetLastAddedSongsPlaylistAsync());
        }

        public Task<IList<SystemPlaylist>> SearchAsync(string searchQuery, uint? take)
        {
            throw new NotSupportedException();
        }

        public Task<SystemPlaylist> GetAsync(int id)
        {
            return this.GetAsync((SystemPlaylistType)id);
        }

        public Task<SystemPlaylist> GetAsync(SystemPlaylistType systemPlaylistType)
        {
            switch (systemPlaylistType)
            {
                case SystemPlaylistType.AllSongs:
                    return this.GetAllSongsPlaylistAsync();
                case SystemPlaylistType.HighlyRated:
                    return this.GetHighlyRatedPlaylistAsync();
                case SystemPlaylistType.LastAdded:
                    return this.GetLastAddedSongsPlaylistAsync();
                default:
                    throw new ArgumentOutOfRangeException("systemPlaylistType");
            }
        }

        private async Task<IList<Song>> GetAllSongsAsync()
        {
            return await this.Connection.QueryAsync<Song>(SqlAllSongs);
        }

        private async Task<IList<Song>> GetHighlyRatedSongsAsync()
        {
            return await this.Connection.QueryAsync<Song>(SqlHighlyRatedSongs, HighlyRatedValue);
        }

        private async Task<IList<Song>> GetLastAddedSongsAsync()
        {
            return await this.Connection.QueryAsync<Song>(SqlLastAddedSongs, LastAddedSongsCount);
        }
    }
}
