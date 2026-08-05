using Catalyst.Game.Launch;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Game.Launch
{
    public sealed class GameLaunchContextTests
    {
        [SetUp]
        public void SetUp()
        {
            GameLaunchContext.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            GameLaunchContext.Clear();
        }

        [Test]
        public void Prepare_StoresPendingRequest()
        {
            GameLaunchContext.Prepare(
                new GameLaunchRequest(
                    seed: 123,
                    entryMode:
                        StageEntryMode.SkipBriefing
                )
            );

            Assert.That(
                GameLaunchContext.HasPendingRequest,
                Is.True
            );
        }

        [Test]
        public void TryConsume_ReturnsPreparedRequest()
        {
            GameLaunchContext.Prepare(
                new GameLaunchRequest(
                    seed: 123,
                    entryMode:
                        StageEntryMode.SkipBriefing
                )
            );

            bool consumed =
                GameLaunchContext.TryConsume(
                    out GameLaunchRequest request
                );

            Assert.That(consumed, Is.True);

            Assert.That(
                request.Seed,
                Is.EqualTo(123)
            );

            Assert.That(
                request.EntryMode,
                Is.EqualTo(
                    StageEntryMode.SkipBriefing
                )
            );
        }

        [Test]
        public void TryConsume_ClearsPreparedRequest()
        {
            GameLaunchContext.Prepare(
                new GameLaunchRequest(
                    seed: 123,
                    entryMode:
                        StageEntryMode.ShowBriefing
                )
            );

            GameLaunchContext.TryConsume(
                out _
            );

            bool consumedAgain =
                GameLaunchContext.TryConsume(
                    out _
                );

            Assert.That(
                consumedAgain,
                Is.False
            );

            Assert.That(
                GameLaunchContext.HasPendingRequest,
                Is.False
            );
        }

        [Test]
        public void Clear_RemovesPendingRequest()
        {
            GameLaunchContext.Prepare(
                new GameLaunchRequest(
                    seed: 123,
                    entryMode:
                        StageEntryMode.ShowBriefing
                )
            );

            GameLaunchContext.Clear();

            bool consumed =
                GameLaunchContext.TryConsume(
                    out _
                );

            Assert.That(consumed, Is.False);
        }

        [Test]
        public void Prepare_ReplacesExistingPendingRequest()
        {
            GameLaunchContext.Prepare(
                new GameLaunchRequest(
                    seed: 111,
                    entryMode:
                        StageEntryMode.ShowBriefing
                )
            );

            GameLaunchContext.Prepare(
                new GameLaunchRequest(
                    seed: 222,
                    entryMode:
                        StageEntryMode.SkipBriefing
                )
            );

            GameLaunchContext.TryConsume(
                out GameLaunchRequest request
            );

            Assert.That(
                request.Seed,
                Is.EqualTo(222)
            );

            Assert.That(
                request.EntryMode,
                Is.EqualTo(
                    StageEntryMode.SkipBriefing
                )
            );
        }
    }
}