using Data.NetFx47.Nightly;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SamuraiApp.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        private void Seed(SamuraiContext context)
        {
            if (context.Samurais.Any())
            {
                return;
            }
            var samurai = new Samurai
            {
                Name = "Kambei Shimada",
                Quotes = new List<Quote>
                               {
                                 new Quote {Text = "I've come to save you"}
                               }
            };
            var samurai2 = new Samurai
            {
                Name = "Kyūzō",
                Quotes = new List<Quote> {
          new Quote {Text = "Watch out for my sharp sword!"},
          new Quote {Text="I told you to watch out for the sharp sword! Oh well!" }
        }
            };
            var samurai3 = new Samurai { Name = "Shichirōji " };
            samurai3.SecretIdentity = new SecretIdentity { RealName = "Julie" };

            context.Samurais.AddRange(samurai, samurai2, samurai3);

            context.SaveChanges();
        }

        [TestMethod]
        public void SeedWithQueryReturnsCorrectNumberOfSamuraisQuotesAndIdentities()
        {
            using (var context = new SamuraiContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                Seed(context);

                var sCount = context.Samurais.ToList();
                var qCount = context.Quotes.ToList();
                var iCount = context.Samurais.Where(s => s.SecretIdentity != null).ToList();
                Assert.AreEqual(3, sCount);
                Assert.AreEqual(3, qCount);
                Assert.AreEqual(1, iCount);
            }
        }

        [TestMethod]
        public void EagerLoadViaProjectionRetrievesRelatedData()
        {
            using (var context = new SamuraiContext())
            {
                var samuraiList = context.Samurais
              .Select(s => new { Samurai = s, Quotes = s.Quotes })
              .ToList();
                Assert.AreEqual(3, samuraiList.Sum(s => s.Quotes.Count()));
            }
            //all results are in memory, but navigations are not fixed up
            //watch this github issue:https://github.com/aspnet/EntityFramework/issues/7131
        }

        [TestMethod]
        public void EagerLoadViaProjectionTracksRelatedData()
        {
            using (var context = new SamuraiContext())
            {
                context.Samurais
                .Select(s => new { Samurai = s, Quotes = s.Quotes })
                .ToList();
                Assert.AreEqual(3, context.ChangeTracker.Entries<Quote>().Count());
            }
        }

        [TestMethod]
        public void EagerLoadViaProjectionTracksRelatedDataIfModified()
        {
            using (var context = new SamuraiContext())
            {
                var samuraiGraph = context.Samurais
              .Select(s => new { Samurai = s, Quotes = s.Quotes })
              .First();
                samuraiGraph.Samurai.Name = "make name different";
                samuraiGraph.Quotes[0].Text = "make quote different";

                Assert.AreEqual(2, context.ChangeTracker.Entries().Count(e => e.State == EntityState.Modified));
            }
        }
    }
}