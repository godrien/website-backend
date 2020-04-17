using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsStatisticService.W3ChampionsStats.DistinctPlayersPerDays;
using W3ChampionsStatisticService.W3ChampionsStats.GameLengths;
using W3ChampionsStatisticService.W3ChampionsStats.GamesPerDays;
using W3ChampionsStatisticService.W3ChampionsStats.HourOfPlay;
using W3ChampionsStatisticService.W3ChampionsStats.RaceAndWinStats;

namespace W3ChampionsStatisticService.Ports
{
    public interface IW3StatsRepo
    {
        Task<Wc3Stats> Load();
        Task Save(Wc3Stats stat);
        Task<GameDay> LoadGamesPerDay(DateTime date);
        Task Save(GameDay stat);
        Task<GameLengthStats> LoadGameLengths();
        Task Save(GameLengthStats stat);
        Task<PlayersOnGameDay> LoadPlayersPerDay(DateTime date);
        Task Save(PlayersOnGameDay stat);
        Task<List<PlayersOnGameDay>> LoadPlayersPerDayBetween(DateTimeOffset from, DateTimeOffset to);
        Task<List<GameDay>> LoadGamesPerDayBetween(DateTimeOffset from, DateTimeOffset to);
        Task<HourOfPlayStats> LoadHourOfPlay();
        Task Save(HourOfPlayStats stat);
    }
}