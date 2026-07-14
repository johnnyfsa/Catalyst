using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Creation
{
    public sealed class DeckRuntimeBuilderTests
    {
        private CardDefinition hydrogen;
        private CardDefinition oxygen;

        [SetUp]
        public void SetUp()
        {
            hydrogen =
                ScriptableObject.CreateInstance<CardDefinition>();

            oxygen =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (hydrogen != null)
            {
                UnityEngine.Object.DestroyImmediate(hydrogen);
            }

            if (oxygen != null)
            {
                UnityEngine.Object.DestroyImmediate(oxygen);
            }
        }

        [Test]
        public void Build_CreatesExpectedTotalQuantity()
        {
            DeckRuntimeBuilder builder =
                CreateBuilder(CreateSequentialIds(5));

            DeckEntry[] entries =
            {
                new DeckEntry(hydrogen, 3),
                new DeckEntry(oxygen, 2)
            };

            DeckRuntime deck = builder.Build(entries);

            Assert.That(deck.Count, Is.EqualTo(5));
        }

        [Test]
        public void Build_CreatesExpectedQuantityPerDefinition()
        {
            DeckRuntimeBuilder builder =
                CreateBuilder(CreateSequentialIds(5));

            DeckEntry[] entries =
            {
                new DeckEntry(hydrogen, 3),
                new DeckEntry(oxygen, 2)
            };

            DeckRuntime deck = builder.Build(entries);

            int hydrogenCount = deck.Cards.Count(
                card => ReferenceEquals(
                    card.Definition,
                    hydrogen
                )
            );

            int oxygenCount = deck.Cards.Count(
                card => ReferenceEquals(
                    card.Definition,
                    oxygen
                )
            );

            Assert.That(hydrogenCount, Is.EqualTo(3));
            Assert.That(oxygenCount, Is.EqualTo(2));
        }

        [Test]
        public void Build_CreatesDifferentInstancesForSameDefinition()
        {
            DeckRuntimeBuilder builder =
                CreateBuilder(CreateSequentialIds(3));

            DeckRuntime deck = builder.Build(
                new[]
                {
                    new DeckEntry(hydrogen, 3)
                }
            );

            Assert.That(
                deck.Cards[0],
                Is.Not.SameAs(deck.Cards[1])
            );

            Assert.That(
                deck.Cards[1],
                Is.Not.SameAs(deck.Cards[2])
            );

            Assert.That(
                deck.Cards.All(
                    card => ReferenceEquals(
                        card.Definition,
                        hydrogen
                    )
                ),
                Is.True
            );
        }

        [Test]
        public void Build_CreatesUniqueInstanceIds()
        {
            DeckRuntimeBuilder builder =
                CreateBuilder(CreateSequentialIds(6));

            DeckRuntime deck = builder.Build(
                new[]
                {
                    new DeckEntry(hydrogen, 4),
                    new DeckEntry(oxygen, 2)
                }
            );

            int uniqueIdCount = deck.Cards
                .Select(card => card.InstanceId)
                .Distinct()
                .Count();

            Assert.That(uniqueIdCount, Is.EqualTo(6));
        }

        [Test]
        public void Build_PreservesEntryOrderBeforeShuffle()
        {
            DeckRuntimeBuilder builder =
                CreateBuilder(CreateSequentialIds(4));

            DeckRuntime deck = builder.Build(
                new[]
                {
                    new DeckEntry(hydrogen, 2),
                    new DeckEntry(oxygen, 2)
                }
            );

            Assert.That(
                deck.Cards[0].Definition,
                Is.SameAs(hydrogen)
            );

            Assert.That(
                deck.Cards[1].Definition,
                Is.SameAs(hydrogen)
            );

            Assert.That(
                deck.Cards[2].Definition,
                Is.SameAs(oxygen)
            );

            Assert.That(
                deck.Cards[3].Definition,
                Is.SameAs(oxygen)
            );
        }

        [Test]
        public void Build_WithEmptyEntries_CreatesEmptyDeck()
        {
            DeckRuntimeBuilder builder =
                CreateBuilder(Array.Empty<Guid>());

            DeckRuntime deck = builder.Build(
                Array.Empty<DeckEntry>()
            );

            Assert.That(deck.IsEmpty, Is.True);
        }

        [Test]
        public void Build_WithNullEntries_Throws()
        {
            DeckRuntimeBuilder builder =
                CreateBuilder(Array.Empty<Guid>());

            Assert.That(
                () => builder.Build(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Build_WithNullEntry_Throws()
        {
            DeckRuntimeBuilder builder =
                CreateBuilder(CreateSequentialIds(1));

            DeckEntry[] entries =
            {
                new DeckEntry(hydrogen, 1),
                null
            };

            Assert.That(
                () => builder.Build(entries),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Build_WhenIdSourceRepeatsId_Throws()
        {
            Guid duplicateId =
                Guid.Parse(
                    "59af4028-a1d7-4985-9f2b-bd3d01250ee7"
                );

            DeckRuntimeBuilder builder =
                CreateBuilder(
                    new[]
                    {
                        duplicateId,
                        duplicateId
                    }
                );

            Assert.That(
                () => builder.Build(
                    new[]
                    {
                        new DeckEntry(hydrogen, 2)
                    }
                ),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void BuildAndShuffle_WithSameIdsAndSeed_ProducesSameOrder()
        {
            Guid[] ids = CreateSequentialIds(10);

            DeckRuntime firstDeck =
                CreateBuilder(ids).Build(
                    new[]
                    {
                        new DeckEntry(hydrogen, 5),
                        new DeckEntry(oxygen, 5)
                    }
                );

            DeckRuntime secondDeck =
                CreateBuilder(ids).Build(
                    new[]
                    {
                        new DeckEntry(hydrogen, 5),
                        new DeckEntry(oxygen, 5)
                    }
                );

            firstDeck.Shuffle(
                new SeededRandomSource(12345)
            );

            secondDeck.Shuffle(
                new SeededRandomSource(12345)
            );

            Guid[] firstOrder = firstDeck.Cards
                .Select(card => card.InstanceId)
                .ToArray();

            Guid[] secondOrder = secondDeck.Cards
                .Select(card => card.InstanceId)
                .ToArray();

            Assert.That(
                firstOrder,
                Is.EqualTo(secondOrder)
            );
        }

        private static DeckRuntimeBuilder CreateBuilder(
            IEnumerable<Guid> ids
        )
        {
            SequenceIdSource idSource =
                new SequenceIdSource(ids);

            CardInstanceFactory factory =
                new CardInstanceFactory(idSource);

            return new DeckRuntimeBuilder(factory);
        }

        private static Guid[] CreateSequentialIds(
            int count
        )
        {
            Guid[] ids = new Guid[count];

            for (int index = 0;
                 index < count;
                 index++)
            {
                string suffix =
                    (index + 1).ToString("D12");

                ids[index] = Guid.Parse(
                    $"00000000-0000-0000-0000-{suffix}"
                );
            }

            return ids;
        }

        private sealed class SequenceIdSource
            : ICardInstanceIdSource
        {
            private readonly Queue<Guid> ids;

            public SequenceIdSource(
                IEnumerable<Guid> ids
            )
            {
                this.ids = new Queue<Guid>(ids);
            }

            public Guid NextId()
            {
                if (ids.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No more test IDs are available."
                    );
                }

                return ids.Dequeue();
            }
        }
    }
}