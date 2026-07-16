using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Reactions.Definitions;
using Catalyst.Reactions.Runtime;
using Catalyst.Tests.EditMode.Common.Creation;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Reactions
{
    public sealed class ReactionMatcherServiceTests
    {
        private readonly List<ReactionDefinition>
            createdReactionDefinitions = new();

        private TestCardFactory cardFactory;
        private ReactionMatcherService matcher;

        private CardDefinition hydrogen;
        private CardDefinition oxygen;
        private CardDefinition water;
        private CardDefinition methane;

        [SetUp]
        public void SetUp()
        {
            cardFactory = new TestCardFactory();
            matcher = new ReactionMatcherService();

            hydrogen = cardFactory.CreateDefinition();
            oxygen = cardFactory.CreateDefinition();
            water = cardFactory.CreateDefinition();
            methane = cardFactory.CreateDefinition();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (
                ReactionDefinition reactionDefinition
                in createdReactionDefinitions
            )
            {
                if (reactionDefinition != null)
                {
                    Object.DestroyImmediate(
                        reactionDefinition
                    );
                }
            }

            createdReactionDefinitions.Clear();

            cardFactory?.DisposeCreatedDefinitions();
            cardFactory = null;
        }

        [Test]
        public void Match_WithExactComposition_Succeeds()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] cards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            Assert.That(
                result.Succeeded,
                Is.True
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionMatchFailure.None
                )
            );
        }

        [Test]
        public void Match_WithDifferentCardOrder_Succeeds()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] cards =
            {
                cardFactory.CreateInstance(oxygen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            Assert.That(
                result.Succeeded,
                Is.True
            );
        }

        [Test]
        public void Match_WithMissingReactant_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] cards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            AssertCompositionDoesNotMatch(result);
        }

        [Test]
        public void Match_WithExcessRequiredReactant_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] cards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            AssertCompositionDoesNotMatch(result);
        }

        [Test]
        public void Match_WithExcessOxygen_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] cards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen),
                cardFactory.CreateInstance(oxygen)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            AssertCompositionDoesNotMatch(result);
        }

        [Test]
        public void Match_WithUnexpectedSubstance_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] cards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen),
                cardFactory.CreateInstance(methane)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            AssertCompositionDoesNotMatch(result);
        }

        [Test]
        public void Match_WithWrongSubstanceCount_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] cards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen),
                cardFactory.CreateInstance(methane)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            AssertCompositionDoesNotMatch(result);
        }

        [Test]
        public void Match_WithEmptyCardCollection_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] cards =
            {
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            AssertCompositionDoesNotMatch(result);
        }

        [Test]
        public void Match_UsesCardDefinitionInsteadOfInstanceIdentity()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance firstHydrogen =
                cardFactory.CreateInstance(hydrogen);

            CardInstance secondHydrogen =
                cardFactory.CreateInstance(hydrogen);

            Assert.That(
                firstHydrogen,
                Is.Not.SameAs(secondHydrogen)
            );

            Assert.That(
                firstHydrogen.InstanceId,
                Is.Not.EqualTo(
                    secondHydrogen.InstanceId
                )
            );

            Assert.That(
                firstHydrogen.Definition,
                Is.SameAs(
                    secondHydrogen.Definition
                )
            );

            CardInstance[] cards =
            {
                firstHydrogen,
                secondHydrogen,
                cardFactory.CreateInstance(oxygen)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            Assert.That(
                result.Succeeded,
                Is.True
            );
        }

        [Test]
        public void Match_WithNullReaction_FailsExplicitly()
        {
            CardInstance[] cards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen)
            };

            ReactionMatchResult result =
                matcher.Match(
                    null,
                    cards
                );

            Assert.That(
                result.Succeeded,
                Is.False
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionMatchFailure.ReactionIsNull
                )
            );
        }

        [Test]
        public void Match_WithNullCardCollection_FailsExplicitly()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    null
                );

            Assert.That(
                result.Succeeded,
                Is.False
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionMatchFailure.CardCollectionIsNull
                )
            );
        }

        [Test]
        public void Match_WithNullCardInsideCollection_FailsExplicitly()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] cards =
            {
                cardFactory.CreateInstance(hydrogen),
                null,
                cardFactory.CreateInstance(oxygen)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            Assert.That(
                result.Succeeded,
                Is.False
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionMatchFailure.TableContainsNullCard
                )
            );
        }

        [Test]
        public void Match_WithInvalidReactionDefinition_FailsExplicitly()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: string.Empty,
                    reactants: new[]
                    {
                        new ReactionCardAmount(
                            hydrogen,
                            2
                        ),
                        new ReactionCardAmount(
                            oxygen,
                            1
                        )
                    },
                    products: new[]
                    {
                        new ReactionCardAmount(
                            water,
                            2
                        )
                    }
                );

            CardInstance[] cards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen)
            };

            ReactionMatchResult result =
                matcher.Match(
                    reaction,
                    cards
                );

            Assert.That(
                result.Succeeded,
                Is.False
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionMatchFailure
                        .ReactionDefinitionIsInvalid
                )
            );

            Assert.That(
                result.DefinitionFailure,
                Is.EqualTo(
                    ReactionDefinitionValidationFailure
                        .ReactionIdIsEmpty
                )
            );
        }

        private ReactionDefinition
            CreateHydrogenCombustionReaction()
        {
            return CreateReaction(
                reactionId: "hydrogen_combustion",
                reactants: new[]
                {
                    new ReactionCardAmount(
                        hydrogen,
                        2
                    ),
                    new ReactionCardAmount(
                        oxygen,
                        1
                    )
                },
                products: new[]
                {
                    new ReactionCardAmount(
                        water,
                        2
                    )
                },
                producedHeat: 1
            );
        }

        private ReactionDefinition CreateReaction(
            string reactionId,
            IEnumerable<ReactionCardAmount> reactants,
            IEnumerable<ReactionCardAmount> products,
            int requiredHeat = 0,
            int producedHeat = 0
        )
        {
            ReactionDefinition reaction =
                ScriptableObject
                    .CreateInstance<ReactionDefinition>();

            reaction.ConfigureForTests(
                reactionId,
                reactants,
                products,
                requiredHeat,
                producedHeat
            );

            createdReactionDefinitions.Add(reaction);

            return reaction;
        }

        private static void AssertCompositionDoesNotMatch(
            ReactionMatchResult result
        )
        {
            Assert.That(
                result.Succeeded,
                Is.False
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionMatchFailure
                        .CompositionDoesNotMatch
                )
            );
        }
    }
}