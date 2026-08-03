using System;
using Catalyst.Cards.Runtime.Session;
using Catalyst.UI.Definitions.Results;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Catalyst.Tests.EditMode.UI.Definitions.Results
{
    public sealed class GameResultDefinitionLibraryTests
    {
        private GameResultDefinitionLibrary library;

        private GameResultDefinition
            missionCompletedDefinition;

        private GameResultDefinition
            deckOutDefinition;

        private GameResultDefinition
            maxTurnsDefinition;

        [SetUp]
        public void SetUp()
        {
            library =
                ScriptableObject.CreateInstance
                    <GameResultDefinitionLibrary>();

            missionCompletedDefinition =
                CreateDefinition(
                    GameSessionEndReason.MissionCompleted,
                    GameResultOutcome.Victory
                );

            deckOutDefinition =
                CreateDefinition(
                    GameSessionEndReason.DeckOut,
                    GameResultOutcome.Defeat
                );

            maxTurnsDefinition =
                CreateDefinition(
                    GameSessionEndReason.MaxTurnsReached,
                    GameResultOutcome.Defeat
                );
        }

        [TearDown]
        public void TearDown()
        {
            DestroyImmediate(
                missionCompletedDefinition
            );

            DestroyImmediate(
                deckOutDefinition
            );

            DestroyImmediate(
                maxTurnsDefinition
            );

            DestroyImmediate(library);
        }

        [Test]
        public void TryGet_WithMissionCompleted_ReturnsConfiguredDefinition()
        {
            SetDefinitions(
                library,
                missionCompletedDefinition
            );

            bool found =
                library.TryGet(
                    GameSessionEndReason.MissionCompleted,
                    out GameResultDefinition result
                );

            Assert.That(found, Is.True);

            Assert.That(
                result,
                Is.SameAs(missionCompletedDefinition)
            );
        }

        [Test]
        public void TryGet_WithDeckOut_ReturnsConfiguredDefinition()
        {
            SetDefinitions(
                library,
                deckOutDefinition
            );

            bool found =
                library.TryGet(
                    GameSessionEndReason.DeckOut,
                    out GameResultDefinition result
                );

            Assert.That(found, Is.True);

            Assert.That(
                result,
                Is.SameAs(deckOutDefinition)
            );
        }

        [Test]
        public void TryGet_WithMaxTurnsReached_ReturnsConfiguredDefinition()
        {
            SetDefinitions(
                library,
                maxTurnsDefinition
            );

            bool found =
                library.TryGet(
                    GameSessionEndReason.MaxTurnsReached,
                    out GameResultDefinition result
                );

            Assert.That(found, Is.True);

            Assert.That(
                result,
                Is.SameAs(maxTurnsDefinition)
            );
        }

        [Test]
        public void TryGet_WithNone_ReturnsFalse()
        {
            SetDefinitions(
                library,
                missionCompletedDefinition
            );

            bool found =
                library.TryGet(
                    GameSessionEndReason.None,
                    out GameResultDefinition result
                );

            Assert.That(found, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TryGet_WithMissingReason_ReturnsFalse()
        {
            SetDefinitions(
                library,
                missionCompletedDefinition
            );

            bool found =
                library.TryGet(
                    GameSessionEndReason.DeckOut,
                    out GameResultDefinition result
                );

            Assert.That(found, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ValidateConfiguration_WithNullEntry_Throws()
        {
            SetDefinitions(
                library,
                missionCompletedDefinition,
                null
            );

            Assert.That(
                () => library.ValidateConfiguration(),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void ValidateConfiguration_WithNoneReason_Throws()
        {
            GameResultDefinition invalidDefinition =
                CreateDefinition(
                    GameSessionEndReason.None,
                    GameResultOutcome.Defeat
                );

            try
            {
                SetDefinitions(
                    library,
                    invalidDefinition
                );

                Assert.That(
                    () => library.ValidateConfiguration(),
                    Throws.TypeOf<InvalidOperationException>()
                );
            }
            finally
            {
                DestroyImmediate(
                    invalidDefinition
                );
            }
        }

        [Test]
        public void ValidateConfiguration_WithDuplicateReason_Throws()
        {
            GameResultDefinition duplicateDefinition =
                CreateDefinition(
                    GameSessionEndReason.DeckOut,
                    GameResultOutcome.Defeat
                );

            try
            {
                SetDefinitions(
                    library,
                    deckOutDefinition,
                    duplicateDefinition
                );

                Assert.That(
                    () => library.ValidateConfiguration(),
                    Throws.TypeOf<InvalidOperationException>()
                );
            }
            finally
            {
                DestroyImmediate(
                    duplicateDefinition
                );
            }
        }

        [Test]
        public void TryGet_PreservesConfiguredAssetReference()
        {
            SetDefinitions(
                library,
                missionCompletedDefinition,
                deckOutDefinition,
                maxTurnsDefinition
            );

            bool found =
                library.TryGet(
                    GameSessionEndReason.DeckOut,
                    out GameResultDefinition result
                );

            Assert.That(found, Is.True);

            Assert.That(
                result,
                Is.SameAs(deckOutDefinition)
            );

            Assert.That(
                result.Outcome,
                Is.EqualTo(GameResultOutcome.Defeat)
            );
        }

        [Test]
        public void TryGet_WithMultipleReasons_FindsEachDefinition()
        {
            SetDefinitions(
                library,
                missionCompletedDefinition,
                deckOutDefinition,
                maxTurnsDefinition
            );

            Assert.That(
                library.TryGet(
                    GameSessionEndReason.MissionCompleted,
                    out GameResultDefinition missionResult
                ),
                Is.True
            );

            Assert.That(
                library.TryGet(
                    GameSessionEndReason.DeckOut,
                    out GameResultDefinition deckResult
                ),
                Is.True
            );

            Assert.That(
                library.TryGet(
                    GameSessionEndReason.MaxTurnsReached,
                    out GameResultDefinition turnResult
                ),
                Is.True
            );

            Assert.That(
                missionResult,
                Is.SameAs(missionCompletedDefinition)
            );

            Assert.That(
                deckResult,
                Is.SameAs(deckOutDefinition)
            );

            Assert.That(
                turnResult,
                Is.SameAs(maxTurnsDefinition)
            );
        }

        private static GameResultDefinition
            CreateDefinition(
                GameSessionEndReason reason,
                GameResultOutcome outcome
            )
        {
            GameResultDefinition definition =
                ScriptableObject.CreateInstance
                    <GameResultDefinition>();

            SerializedObject serializedDefinition =
                new SerializedObject(definition);

            serializedDefinition
                .FindProperty("endReason")
                .enumValueIndex = (int)reason;

            serializedDefinition
                .FindProperty("outcome")
                .enumValueIndex = (int)outcome;

            serializedDefinition
                .ApplyModifiedPropertiesWithoutUndo();

            return definition;
        }

        private static void SetDefinitions(
            GameResultDefinitionLibrary target,
            params GameResultDefinition[] definitions
        )
        {
            SerializedObject serializedLibrary =
                new SerializedObject(target);

            SerializedProperty definitionsProperty =
                serializedLibrary.FindProperty(
                    "definitions"
                );

            definitionsProperty.arraySize =
                definitions.Length;

            for (
                int index = 0;
                index < definitions.Length;
                index++
            )
            {
                definitionsProperty
                    .GetArrayElementAtIndex(index)
                    .objectReferenceValue =
                        definitions[index];
            }

            serializedLibrary
                .ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DestroyImmediate(
            UnityEngine.Object target
        )
        {
            if (target != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    target
                );
            }
        }
    }
}