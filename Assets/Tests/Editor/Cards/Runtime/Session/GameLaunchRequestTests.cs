using System;
using Catalyst.Game.Launch;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Game.Launch
{
    public sealed class GameLaunchRequestTests
    {
        [Test]
        public void Constructor_PreservesSeedAndEntryMode()
        {
            var request =
                new GameLaunchRequest(
                    seed: 123,
                    entryMode:
                        StageEntryMode.SkipBriefing
                );

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
        public void Constructor_WithUnsupportedEntryMode_Throws()
        {
            var unsupportedMode =
                (StageEntryMode)999;

            Assert.That(
                () => new GameLaunchRequest(
                    seed: 123,
                    entryMode: unsupportedMode
                ),
                Throws.TypeOf
                    <ArgumentOutOfRangeException>()
            );
        }
    }
}