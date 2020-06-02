using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using W3ChampionsStatisticService.Clans;

namespace WC3ChampionsStatisticService.UnitTests
{
    [TestFixture]
    public class ClanTests : IntegrationTestBase
    {
        private ClanRepository _clanRepository;
        private ClanCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _clanRepository = new ClanRepository(MongoClient);
            _handler = new ClanCommandHandler(_clanRepository);
        }

        [Test]
        public void InvitePlayer_ClanNotPresent()
        {
            Assert.ThrowsAsync<ValidationException>(async () =>
                await _handler.InviteToClan(ObjectId.GenerateNewId().ToString(), "peter#123", "doesNotMatter"));
        }

        [Test]
        public async Task InvitePlayer_ThatHasAlreadySigned_Founder()
        {
            var clan = await _handler.CreateClan("egal", "Peter#123");

            Assert.ThrowsAsync<ValidationException>(async () => await _handler.InviteToClan(clan.IdRaw, "Peter#123", "Peter#123"));
        }

        [Test]
        public async Task PromoteToShaman()
        {
            var clan = await CreateFoundedClanForTest();

            await _handler.AddShamanToClan(clan.Members[2], clan.IdRaw, clan.ChiefTain);

            var clanLoaded = await _clanRepository.LoadClan(clan.IdRaw);

            Assert.AreEqual(clan.Members[2], clanLoaded.Shamans.Single());
        }

        [Test]
        public async Task DemoteShaman()
        {
            var clan = await CreateFoundedClanForTest();

            await _handler.AddShamanToClan(clan.Members[2], clan.IdRaw, clan.ChiefTain);
            await _handler.RemoveShamanFromClan(clan.Members[2], clan.IdRaw, clan.ChiefTain);

            var clanLoaded = await _clanRepository.LoadClan(clan.IdRaw);

            Assert.IsEmpty(clanLoaded.Shamans);
        }

        [Test]
        public async Task PromoteShamanThatISNotInClan_Fails()
        {
            var clan = await CreateFoundedClanForTest();

            Assert.ThrowsAsync<ValidationException>(async () => await _handler.AddShamanToClan("NotInChal#123", clan.IdRaw, clan.ChiefTain));
        }

        [Test]
        public async Task PromoteShamanThatIsChieftain_Fails()
        {
            var clan = await CreateFoundedClanForTest();

            Assert.ThrowsAsync<ValidationException>(async () => await _handler.AddShamanToClan(clan.ChiefTain, clan.IdRaw, clan.ChiefTain));
        }

        [Test]
        public async Task InvitePlayer_PlayerRejects_IsNotAddedToFoundingFathers()
        {
            var clan = await _handler.CreateClan("egal", "Peter#123");
            await _handler.InviteToClan(clan.IdRaw, "NewGUY#123", "Peter#123");
            await _handler.RevokeInvitationToClan("NewGUY#123", clan.IdRaw, "Peter#123");

            var clanLoaded = await _clanRepository.LoadClan(clan.IdRaw);

            Assert.AreEqual(1, clanLoaded.FoundingFathers.Count);
            Assert.AreEqual("Peter#123", clanLoaded.FoundingFathers[0]);
        }

        [Test]
        public async Task InvitePlayer_ThatHasAlreadySigned()
        {
            var clan = await _handler.CreateClan("egal", "Peter#123");
            await _handler.InviteToClan(clan.IdRaw, "NewGUY#123", "Peter#123");
            await _handler.AcceptInvite(clan.IdRaw, "NewGUY#123");

            Assert.ThrowsAsync<ValidationException>(async () => await _handler.InviteToClan(clan.IdRaw, "NewGUY#123", "Peter#123"));
        }

        [Test]
        public async Task InvitePlayer()
        {
            var clan = await CreateFoundedClanForTest();
            await _handler.InviteToClan(clan.Id.ToString(), "peter#123", clan.ChiefTain);

            var member = await _clanRepository.LoadMemberShip("peter#123");
            var clanLoaded = await _clanRepository.LoadClan(clan.Id.ToString());

            Assert.AreEqual("peter#123", member.BattleTag);
            Assert.AreEqual(clanLoaded.Id, member.PendingInviteFromClan);
            Assert.AreEqual(clanLoaded.PendingInvites.Single(), member.BattleTag);
        }

        [Test]
        public async Task RevokeInvite()
        {
            var clan = await CreateFoundedClanForTest();

            await _handler.InviteToClan(clan.Id.ToString(), "peter#123", clan.ChiefTain);
            await _handler.RevokeInvitationToClan("peter#123", clan.Id.ToString(), clan.ChiefTain);

            var member = await _clanRepository.LoadMemberShip("peter#123");
            var clanLoaded = await _clanRepository.LoadClan(clan.Id.ToString());

            Assert.AreEqual("peter#123", member.BattleTag);
            Assert.IsNull(member.PendingInviteFromClan);
            Assert.IsEmpty(clanLoaded.PendingInvites);
        }

        [Test]
        public async Task SignPetition()
        {
            var clanNameExpected = "Cool Shit";
            var clan = await _handler.CreateClan(clanNameExpected, "Peter#123");

            await _handler.InviteToClan(clan.Id.ToString(), "peter#123", "Peter#123");
            await _handler.AcceptInvite(clan.Id.ToString(), "peter#123");

            var member = await _clanRepository.LoadMemberShip("peter#123");
            var clanLoaded = await _clanRepository.LoadClan(clan.Id.ToString());

            Assert.AreEqual("peter#123", member.BattleTag);
            Assert.IsNull(member.PendingInviteFromClan);
            Assert.AreEqual(clanLoaded.Id, member.ClanId);
            Assert.IsEmpty(clanLoaded.PendingInvites);
            Assert.AreEqual("peter#123", clanLoaded.FoundingFathers[1]);
        }

        [Test]
        public async Task CreateClan()
        {
            var clanNameExpected = "Cool Shit";
            var clan = await _handler.CreateClan(clanNameExpected, "Peter#123");

            var clanLoaded = await _clanRepository.LoadClan(clan.Id.ToString());

            Assert.AreEqual(clan.ClanName, clanNameExpected);
            Assert.AreEqual(clan.ClanName, clanNameExpected);
            Assert.AreNotEqual(clanLoaded.Id, ObjectId.Empty);
        }

        [Test]
        public async Task CreatClanWithSameNameNotPossible()
        {
            var clanNameExpected = "Cool Shit";
            await _handler.CreateClan(clanNameExpected, "Peter#123");

            Assert.ThrowsAsync<ValidationException>(async () =>
                await _handler.CreateClan(clanNameExpected, "Peter#123"));
        }

        [Test]
        public async Task CreatClan_FounderGetsCreated()
        {
            var clanNameExpected = "Cool Shit";
            var clan = await _handler.CreateClan(clanNameExpected, "Peter#123");

            var founder = await _clanRepository.LoadMemberShip("Peter#123");

            Assert.AreEqual(founder.ClanId, clan.Id);
            Assert.AreEqual(founder.BattleTag, "Peter#123");
        }

        [Test]
        public async Task CreatClan_FoundingTwiceIsProhibitted()
        {
            await _handler.CreateClan("Cool Shit", "Peter#123");

            Assert.ThrowsAsync<ValidationException>(async () => await _handler.CreateClan("Cool Shit NEW", "Peter#123"));
        }

        [Test]
        public async Task DeleteClan()
        {
            var clan = await CreateFoundedClanForTest();

            await _handler.DeleteClan(clan.IdRaw, clan.ChiefTain);

            var clanLoaded = await _clanRepository.LoadClan(clan.IdRaw);
            Assert.IsNull(clanLoaded);


            var chieftain = await _clanRepository.LoadMemberShip(clan.ChiefTain);
            Assert.IsNull(chieftain.ClanId);

            foreach (var clanMember in clan.Members)
            {
                var member = await _clanRepository.LoadMemberShip(clanMember);
                Assert.IsNull(member.ClanId);
            }
        }

        private async Task<Clan> CreateFoundedClanForTest()
        {
            var clanNameExpected = "Cool Shit";
            var clan = await _handler.CreateClan(clanNameExpected, "Peter#123");
            for (int i = 0; i < 6; i++)
            {
                await _handler.InviteToClan(clan.Id.ToString(), $"btag#{i}", "Peter#123");
                await _handler.AcceptInvite(clan.Id.ToString(), $"btag#{i}");
            }

            return await _clanRepository.LoadClan(clan.IdRaw);
        }
    }
}