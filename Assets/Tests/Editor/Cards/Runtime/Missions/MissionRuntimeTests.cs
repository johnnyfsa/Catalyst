using System;
using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Missions;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Missions
{
    public sealed class MissionRuntimeTests
    {
        private CardDefinition firstDefinition;
        private CardDefinition secondDefinition;

        [SetUp]
        public void SetUp()
        {
            firstDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            secondDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (firstDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    firstDefinition
                );
            }

            if (secondDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    secondDefinition
                );
            }
        }

        [Test]
        public void Constructor_WithNullObjectives_Throws()
        {
            Assert.That(
                () => new MissionRuntime(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Constructor_WithNullObjectiveEntry_Throws()
        {
            Assert.That(
                () => new MissionRuntime(
                    new CardDeliveryZoneRuntime[]
                    {
                        null
                    }
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Constructor_WithNoObjectives_CreatesInactiveMission()
        {
            MissionRuntime mission =
                new MissionRuntime(
                    Array.Empty<CardDeliveryZoneRuntime>()
                );

            Assert.That(
                mission.DeliveryObjectives,
                Is.Not.Null
            );

            Assert.That(
                mission.DeliveryObjectives,
                Is.Empty
            );

            Assert.That(
                mission.HasObjectives,
                Is.False
            );
        }

        [Test]
        public void MissionWithNoObjectives_IsNotCompleted()
        {
            MissionRuntime mission =
                new MissionRuntime(
                    Array.Empty<CardDeliveryZoneRuntime>()
                );

            Assert.That(
                mission.IsCompleted,
                Is.False
            );
        }

        [Test]
        public void MissionWithIncompleteObjective_IsNotCompleted()
        {
            CardDeliveryZoneRuntime objective =
                CreateObjective(
                    firstDefinition,
                    requiredAmount: 2
                );

            MissionRuntime mission =
                new MissionRuntime(
                    new[]
                    {
                        objective
                    }
                );

            Assert.That(
                objective.IsCompleted,
                Is.False
            );

            Assert.That(
                mission.IsCompleted,
                Is.False
            );
        }

        [Test]
        public void MissionWithCompletedObjective_IsCompleted()
        {
            CardDeliveryZoneRuntime objective =
                CreateObjective(
                    firstDefinition,
                    requiredAmount: 1
                );

            Assert.That(
                objective.TryAdd(
                    CreateCard(firstDefinition)
                ),
                Is.True
            );

            MissionRuntime mission =
                new MissionRuntime(
                    new[]
                    {
                        objective
                    }
                );

            Assert.That(
                objective.IsCompleted,
                Is.True
            );

            Assert.That(
                mission.IsCompleted,
                Is.True
            );
        }

        [Test]
        public void MissionWithMixedObjectives_IsNotCompleted()
        {
            CardDeliveryZoneRuntime completedObjective =
                CreateObjective(
                    firstDefinition,
                    requiredAmount: 1
                );

            CardDeliveryZoneRuntime incompleteObjective =
                CreateObjective(
                    secondDefinition,
                    requiredAmount: 2
                );

            Assert.That(
                completedObjective.TryAdd(
                    CreateCard(firstDefinition)
                ),
                Is.True
            );

            MissionRuntime mission =
                new MissionRuntime(
                    new[]
                    {
                        completedObjective,
                        incompleteObjective
                    }
                );

            Assert.That(
                completedObjective.IsCompleted,
                Is.True
            );

            Assert.That(
                incompleteObjective.IsCompleted,
                Is.False
            );

            Assert.That(
                mission.IsCompleted,
                Is.False
            );
        }

        [Test]
        public void MissionWithAllObjectivesCompleted_IsCompleted()
        {
            CardDeliveryZoneRuntime firstObjective =
                CreateObjective(
                    firstDefinition,
                    requiredAmount: 1
                );

            CardDeliveryZoneRuntime secondObjective =
                CreateObjective(
                    secondDefinition,
                    requiredAmount: 2
                );

            Assert.That(
                firstObjective.TryAdd(
                    CreateCard(firstDefinition)
                ),
                Is.True
            );

            Assert.That(
                secondObjective.TryAdd(
                    CreateCard(secondDefinition)
                ),
                Is.True
            );

            Assert.That(
                secondObjective.TryAdd(
                    CreateCard(secondDefinition)
                ),
                Is.True
            );

            MissionRuntime mission =
                new MissionRuntime(
                    new[]
                    {
                        firstObjective,
                        secondObjective
                    }
                );

            Assert.That(
                firstObjective.IsCompleted,
                Is.True
            );

            Assert.That(
                secondObjective.IsCompleted,
                Is.True
            );

            Assert.That(
                mission.IsCompleted,
                Is.True
            );
        }

        [Test]
        public void MissionReflectsObjectiveProgressAfterConstruction()
        {
            CardDeliveryZoneRuntime objective =
                CreateObjective(
                    firstDefinition,
                    requiredAmount: 2
                );

            MissionRuntime mission =
                new MissionRuntime(
                    new[]
                    {
                        objective
                    }
                );

            Assert.That(
                mission.IsCompleted,
                Is.False
            );

            Assert.That(
                objective.TryAdd(
                    CreateCard(firstDefinition)
                ),
                Is.True
            );

            Assert.That(
                mission.IsCompleted,
                Is.False
            );

            Assert.That(
                objective.TryAdd(
                    CreateCard(firstDefinition)
                ),
                Is.True
            );

            Assert.That(
                mission.IsCompleted,
                Is.True
            );
        }

        [Test]
        public void DeliveryObjectives_PreserveConfiguredReferences()
        {
            CardDeliveryZoneRuntime firstObjective =
                CreateObjective(
                    firstDefinition,
                    requiredAmount: 1
                );

            CardDeliveryZoneRuntime secondObjective =
                CreateObjective(
                    secondDefinition,
                    requiredAmount: 2
                );

            MissionRuntime mission =
                new MissionRuntime(
                    new[]
                    {
                        firstObjective,
                        secondObjective
                    }
                );

            Assert.That(
                mission.DeliveryObjectives.Count,
                Is.EqualTo(2)
            );

            Assert.That(
                mission.DeliveryObjectives[0],
                Is.SameAs(firstObjective)
            );

            Assert.That(
                mission.DeliveryObjectives[1],
                Is.SameAs(secondObjective)
            );
        }

        [Test]
        public void DeliveryObjectives_CannotBeModifiedExternally()
        {
            CardDeliveryZoneRuntime objective =
                CreateObjective(
                    firstDefinition,
                    requiredAmount: 1
                );

            MissionRuntime mission =
                new MissionRuntime(
                    new[]
                    {
                        objective
                    }
                );

            IList<CardDeliveryZoneRuntime> exposedCollection =
                mission.DeliveryObjectives
                    as IList<CardDeliveryZoneRuntime>;

            Assert.That(
                exposedCollection,
                Is.Not.Null
            );

            Assert.That(
                exposedCollection.IsReadOnly,
                Is.True
            );

            Assert.That(
                () => exposedCollection.Add(
                    CreateObjective(
                        secondDefinition,
                        requiredAmount: 1
                    )
                ),
                Throws.TypeOf<NotSupportedException>()
            );

            Assert.That(
                mission.DeliveryObjectives.Count,
                Is.EqualTo(1)
            );

            Assert.That(
                mission.DeliveryObjectives[0],
                Is.SameAs(objective)
            );
        }

        private static CardDeliveryZoneRuntime CreateObjective(
            CardDefinition definition,
            int requiredAmount
        )
        {
            return new CardDeliveryZoneRuntime(
                definition,
                requiredAmount
            );
        }

        private static CardInstance CreateCard(
            CardDefinition definition
        )
        {
            return new CardInstance(
                Guid.NewGuid(),
                definition
            );
        }
    }
}