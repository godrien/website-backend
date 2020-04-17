using System;
using NUnit.Framework;
using W3ChampionsStatisticService.Matches;
using W3ChampionsStatisticService.W3ChampionsStats.HourOfPlay;

namespace WC3ChampionsStatisticService.UnitTests
{
    [TestFixture]
    public class PlayTimePerDayStats
    {

        [Test]
        [Ignore("later")]
        public void PlayTimesPerDay()
        {
            var dateTime = new DateTime(2020, 10, 16);
            var hourOfPlayStats = HourOfPlayStats.Create(dateTime);

            hourOfPlayStats.Apply(GameMode.GM_1v1, DateTimeOffset.UtcNow, dateTime);

            Assert.AreEqual(1, hourOfPlayStats.PlayTimesPerMode[0].PlayTimePerHour[0].Games);
            Assert.AreEqual(0, hourOfPlayStats.PlayTimesPerMode[0].PlayTimePerHour[1].Games);
        }
    }
}