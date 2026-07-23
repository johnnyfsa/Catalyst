using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Game.Bootstrap;
using Catalyst.Reactions.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Game.Bootstrap
{
    public sealed class GameSessionBootstrapTests
    {
        private const string DeckEntriesFieldName =
    "entries";

        private const string DeckDefinitionFieldName =
            "deckDefinition";

        private const string ReactionLibraryFieldName =
            "reactionLibrary";

        private const string ReactionsFieldName =
            "reactions";
        private GameObject bootstrapObject;
        private GameSessionBootstrap bootstrap;

        private readonly List<UnityEngine.Object> createdObjects =
            new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            bootstrapObject =
                new GameObject(
                    nameof(GameSessionBootstrapTests)
                );

            bootstrap =
                bootstrapObject.AddComponent
                    <GameSessionBootstrap>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (
                UnityEngine.Object createdObject
                in createdObjects
            )
            {
                if (createdObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        createdObject
                    );
                }
            }

            createdObjects.Clear();

            if (bootstrapObject != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    bootstrapObject
                );
            }
        }

        [Test]
        public void Awake_WithValidConfiguration_CreatesReactionFlow()
        {
            CardDefinition reactant =
                CreateCardDefinition();

            DeckDefinition deckDefinition =
                CreateDeckDefinition(
                    new DeckEntry(
                        reactant,
                        quantity: 1
                    )
                );

            ReactionDefinition firstReaction =
                CreateReactionDefinition();

            ReactionDefinition secondReaction =
                CreateReactionDefinition();

            ReactionLibraryDefinition library =
                CreateReactionLibrary(
                    firstReaction,
                    secondReaction
                );

            SetPrivateField(
                bootstrap,
                DeckDefinitionFieldName,
                deckDefinition
            );

            SetPrivateField(
                bootstrap,
                ReactionLibraryFieldName,
                library
            );

            SetPrivateField(
                bootstrap,
                "initialHandSize",
                1
            );

            SetPrivateField(
                bootstrap,
                "maxHandSize",
                1
            );

            SetPrivateField(
                 bootstrap,
                "useTurnLimit",
                false
            );

            InvokeAwake();

            Assert.That(
                bootstrap.Session,
                Is.Not.Null
            );

            Assert.That(
                bootstrap.SessionFlow,
                Is.Not.Null
            );

            Assert.That(
                bootstrap.ReactionFlow,
                Is.Not.Null
            );

            Assert.That(
                bootstrap.AvailableReactions.Count,
                Is.EqualTo(2)
            );

            Assert.That(
                bootstrap.ReactionFlow
                    .AvailableReactions.Count,
                Is.EqualTo(2)
            );

            Assert.That(
                bootstrap.ReactionFlow
                    .AvailableReactions[0],
                Is.SameAs(firstReaction)
            );

            Assert.That(
                bootstrap.ReactionFlow
                    .AvailableReactions[1],
                Is.SameAs(secondReaction)
            );
        }

        [Test]
        public void InitializeFromDeckDefinition_ValidDefinition_BuildsSession()
        {
            CardDefinition hydrogen =
                CreateCardDefinition();

            CardDefinition oxygen =
                CreateCardDefinition();

            DeckDefinition deckDefinition =
                CreateDeckDefinition(
                    new DeckEntry(hydrogen, 3),
                    new DeckEntry(oxygen, 2)
                );

            var config =
                new GameSessionConfig(
                    initialHandSize: 2,
                    maxHandSize: 4
                );


            GameSession session =
                InvokeInitializeFromDeckDefinition(
                    deckDefinition,
                    config,
                    new SeededRandomSource(12345)
                );

            Assert.That(session, Is.Not.Null);

            Assert.That(
                bootstrap.Session,
                Is.SameAs(session)
            );

            Assert.That(
                session.SessionCards.Count,
                Is.EqualTo(5)
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(2)
            );

            Assert.That(
                session.Deck.Count,
                Is.EqualTo(3)
            );
        }

        [Test]
        public void InitializeFromDeckDefinition_NullDefinition_Throws()
        {
            var config =
                new GameSessionConfig(
                    initialHandSize: 1,
                    maxHandSize: 1
                );

            TargetInvocationException exception =
                Assert.Throws<TargetInvocationException>(
                    () =>
                        InvokeInitializeFromDeckDefinition(
                            null,
                            config,
                            new SeededRandomSource(12345)
                        )
                );

            Assert.That(
                exception.InnerException,
                Is.TypeOf<ArgumentNullException>()
            );

            var argumentException =
                (ArgumentNullException)
                exception.InnerException;

            Assert.That(
                argumentException.ParamName,
                Is.EqualTo("definition")
            );

            Assert.That(
                bootstrap.Session,
                Is.Null
            );
        }

        [Test]
        public void InitializeFromDeckEntries_ValidEntries_BuildsSession()
        {
            CardDefinition hydrogen =
                CreateCardDefinition();

            CardDefinition oxygen =
                CreateCardDefinition();

            var entries =
                new[]
                {
                    new DeckEntry(hydrogen, 2),
                    new DeckEntry(oxygen, 2)
                };

            var config =
                new GameSessionConfig(
                    initialHandSize: 2,
                    maxHandSize: 4
                );

            GameSession session =
                InvokeInitializeFromDeckEntries(
                    entries,
                    config,
                    new SeededRandomSource(54321)
                );

            Assert.That(session, Is.Not.Null);

            Assert.That(
                bootstrap.Session,
                Is.SameAs(session)
            );

            Assert.That(
                session.SessionCards.Count,
                Is.EqualTo(4)
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(2)
            );

            Assert.That(
                session.Deck.Count,
                Is.EqualTo(2)
            );
        }

        [Test]
        public void Initialize_WhenAlreadyInitialized_Throws()
        {
            CardDefinition hydrogen =
                CreateCardDefinition();

            DeckDefinition deckDefinition =
                CreateDeckDefinition(
                    new DeckEntry(hydrogen, 2)
                );

            var config =
                new GameSessionConfig(
                    initialHandSize: 1,
                    maxHandSize: 2
                );

            InvokeInitializeFromDeckDefinition(
                deckDefinition,
                config,
                new SeededRandomSource(12345)
            );

            TargetInvocationException exception =
                Assert.Throws<TargetInvocationException>(
                    () =>
                        InvokeInitializeFromDeckEntries(
                            new[]
                            {
                                new DeckEntry(
                                    hydrogen,
                                    2
                                )
                            },
                            config,
                            new SeededRandomSource(54321)
                        )
                );

            Assert.That(
                exception.InnerException,
                Is.TypeOf<InvalidOperationException>()
            );

            Assert.That(
                exception.InnerException.Message,
                Is.EqualTo(
                    "The game session has already been initialized."
                )
            );
        }

        private GameSession
            InvokeInitializeFromDeckDefinition(
                DeckDefinition definition,
                GameSessionConfig config,
                IRandomSource randomSource
            )
        {
            MethodInfo method =
                FindInitializationMethod(
                    "InitializeFromDeckDefinition",
                    typeof(DeckDefinition),
                    typeof(GameSessionConfig),
                    typeof(IRandomSource)
                );

            return (GameSession)method.Invoke(
                bootstrap,
                new object[]
                {
                    definition,
                    config,
                    randomSource
                }
            );
        }

        private GameSession InvokeInitializeFromDeckEntries(
            IEnumerable<DeckEntry> entries,
            GameSessionConfig config,
            IRandomSource randomSource
        )
        {
            MethodInfo method =
                FindInitializationMethod(
                    "InitializeFromDeckEntries",
                    typeof(IEnumerable<DeckEntry>),
                    typeof(GameSessionConfig),
                    typeof(IRandomSource)
                );

            return (GameSession)method.Invoke(
                bootstrap,
                new object[]
                {
                    entries,
                    config,
                    randomSource
                }
            );
        }

        private static MethodInfo FindInitializationMethod(
            string methodName,
            params Type[] parameterTypes
        )
        {
            MethodInfo method =
                typeof(GameSessionBootstrap).GetMethod(
                    methodName,
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic,
                    binder: null,
                    types: parameterTypes,
                    modifiers: null
                );

            Assert.That(
                method,
                Is.Not.Null,
                $"Could not find method '{methodName}' " +
                $"with parameters: " +
                $"{string.Join(", ", parameterTypes.Select(type => type.Name))}."
            );

            return method;
        }

        private CardDefinition CreateCardDefinition()
        {
            CardDefinition definition =
                ScriptableObject.CreateInstance
                    <CardDefinition>();

            createdObjects.Add(definition);

            return definition;
        }

        private DeckDefinition CreateDeckDefinition(
    params DeckEntry[] entries
)
        {
            DeckDefinition definition =
                ScriptableObject.CreateInstance
                    <DeckDefinition>();

            createdObjects.Add(definition);

            SetPrivateField(
                definition,
                DeckEntriesFieldName,
                new List<DeckEntry>(entries)
            );

            return definition;
        }

        private ReactionDefinition CreateReactionDefinition()
        {
            ReactionDefinition definition =
                ScriptableObject.CreateInstance
                    <ReactionDefinition>();

            createdObjects.Add(definition);

            return definition;
        }

        private static void SetPrivateField(
    object target,
    string fieldName,
    object value
)
        {
            FieldInfo field =
                target.GetType().GetField(
                    fieldName,
                    BindingFlags.Instance
                    | BindingFlags.NonPublic
                );

            Assert.That(
                field,
                Is.Not.Null,
                $"Could not find private field " +
                $"'{fieldName}' in " +
                $"{target.GetType().Name}."
            );

            field.SetValue(
                target,
                value
            );
        }

        private ReactionLibraryDefinition
    CreateReactionLibrary(
        params ReactionDefinition[] reactions
    )
        {
            ReactionLibraryDefinition definition =
                ScriptableObject.CreateInstance
                    <ReactionLibraryDefinition>();

            createdObjects.Add(definition);

            SetPrivateField(
                definition,
                ReactionsFieldName,
                new List<ReactionDefinition>(
                    reactions
                )
            );

            return definition;
        }

        private void InvokeAwake()
        {
            MethodInfo method =
                typeof(GameSessionBootstrap).GetMethod(
                    "Awake",
                    BindingFlags.Instance
                    | BindingFlags.NonPublic
                );

            Assert.That(
                method,
                Is.Not.Null,
                $"Could not find private method " +
                $"'Awake' in " +
                $"{nameof(GameSessionBootstrap)}."
            );

            method.Invoke(
                bootstrap,
                Array.Empty<object>()
            );
        }
    }

}