using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Reactions.Definitions;
using Catalyst.Reactions.Runtime;
using Catalyst.Tests.EditMode.Common.Creation;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Reactions
{
    public sealed class ReactionDefinitionValidatorTests
    {
        private readonly List<ReactionDefinition>
            createdReactionDefinitions = new();

        private TestCardFactory cardFactory;

        private CardDefinition hydrogen;
        private CardDefinition oxygen;
        private CardDefinition water;

        [SetUp]
        public void SetUp()
        {
            cardFactory = new TestCardFactory();

            hydrogen = cardFactory.CreateDefinition();
            oxygen = cardFactory.CreateDefinition();
            water = cardFactory.CreateDefinition();
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
        public void Validate_WithValidDefinition_ReturnsValid()
        {
            ReactionDefinition reaction =
                CreateValidReaction();

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            Assert.That(
                result.IsValid,
                Is.True
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionDefinitionValidationFailure.None
                )
            );
        }

        [Test]
        public void Validate_WithNullDefinition_ReturnsDefinitionIsNull()
        {
            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    null
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .DefinitionIsNull
            );
        }

        [Test]
        public void Validate_WithNullReactionId_ReturnsReactionIdIsEmpty()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: null,
                    reactants: CreateValidReactants(),
                    products: CreateValidProducts()
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ReactionIdIsEmpty
            );
        }

        [Test]
        public void Validate_WithEmptyReactionId_ReturnsReactionIdIsEmpty()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: string.Empty,
                    reactants: CreateValidReactants(),
                    products: CreateValidProducts()
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ReactionIdIsEmpty
            );
        }

        [Test]
        public void Validate_WithWhitespaceReactionId_ReturnsReactionIdIsEmpty()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "   ",
                    reactants: CreateValidReactants(),
                    products: CreateValidProducts()
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ReactionIdIsEmpty
            );
        }

        [Test]
        public void Validate_WithNullReactantCollection_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: null,
                    products: CreateValidProducts()
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ReactantCollectionIsNull
            );
        }

        [Test]
        public void Validate_WithNullProductCollection_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: CreateValidReactants(),
                    products: null
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ProductCollectionIsNull
            );
        }

        [Test]
        public void Validate_WithNoReactants_ReturnsNoReactants()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants:
                        new ReactionCardAmount[0],
                    products:
                        CreateValidProducts()
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .NoReactants
            );
        }

        [Test]
        public void Validate_WithNoProducts_ReturnsNoProducts()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants:
                        CreateValidReactants(),
                    products:
                        new ReactionCardAmount[0]
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .NoProducts
            );
        }

        [Test]
        public void Validate_WithNullReactantEntry_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: new ReactionCardAmount[]
                    {
                        new ReactionCardAmount(
                            hydrogen,
                            2
                        ),
                        null
                    },
                    products: CreateValidProducts()
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ReactantEntryIsNull
            );
        }

        [Test]
        public void Validate_WithNullProductEntry_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: CreateValidReactants(),
                    products: new ReactionCardAmount[]
                    {
                        null
                    }
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ProductEntryIsNull
            );
        }

        [Test]
        public void Validate_WithNullReactantDefinition_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: new[]
                    {
                        new ReactionCardAmount(
                            null,
                            2
                        ),
                        new ReactionCardAmount(
                            oxygen,
                            1
                        )
                    },
                    products: CreateValidProducts()
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ReactantDefinitionIsNull
            );
        }

        [Test]
        public void Validate_WithNullProductDefinition_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: CreateValidReactants(),
                    products: new[]
                    {
                        new ReactionCardAmount(
                            null,
                            2
                        )
                    }
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ProductDefinitionIsNull
            );
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Validate_WithNonPositiveReactantQuantity_ReturnsExpectedFailure(
            int quantity
        )
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: new[]
                    {
                        new ReactionCardAmount(
                            hydrogen,
                            quantity
                        ),
                        new ReactionCardAmount(
                            oxygen,
                            1
                        )
                    },
                    products: CreateValidProducts()
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ReactantQuantityIsNotPositive
            );
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Validate_WithNonPositiveProductQuantity_ReturnsExpectedFailure(
            int quantity
        )
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: CreateValidReactants(),
                    products: new[]
                    {
                        new ReactionCardAmount(
                            water,
                            quantity
                        )
                    }
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ProductQuantityIsNotPositive
            );
        }

        [Test]
        public void Validate_WithDuplicateReactantDefinition_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: new[]
                    {
                        new ReactionCardAmount(
                            hydrogen,
                            1
                        ),
                        new ReactionCardAmount(
                            hydrogen,
                            1
                        ),
                        new ReactionCardAmount(
                            oxygen,
                            1
                        )
                    },
                    products: CreateValidProducts()
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .DuplicateReactantDefinition
            );
        }

        [Test]
        public void Validate_WithDuplicateProductDefinition_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "invalid_products",
                    reactants: CreateValidReactants(),
                    products: new[]
                    {
                        new ReactionCardAmount(
                            water,
                            1
                        ),
                        new ReactionCardAmount(
                            water,
                            1
                        )
                    }
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .DuplicateProductDefinition
            );
        }

        [Test]
        public void Validate_WithNegativeRequiredHeat_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: CreateValidReactants(),
                    products: CreateValidProducts(),
                    requiredHeat: -1
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .RequiredHeatIsNegative
            );
        }

        [Test]
        public void Validate_WithNegativeProducedHeat_ReturnsExpectedFailure()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "hydrogen_combustion",
                    reactants: CreateValidReactants(),
                    products: CreateValidProducts(),
                    producedHeat: -1
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            AssertInvalid(
                result,
                ReactionDefinitionValidationFailure
                    .ProducedHeatIsNegative
            );
        }

        [Test]
        public void Validate_WithRequiredHeat_ReturnsValid()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "heated_reaction",
                    reactants: CreateValidReactants(),
                    products: CreateValidProducts(),
                    requiredHeat: 1
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            Assert.That(
                result.IsValid,
                Is.True
            );
        }

        [Test]
        public void Validate_WithProducedHeat_ReturnsValid()
        {
            ReactionDefinition reaction =
                CreateReaction(
                    reactionId: "exothermic_reaction",
                    reactants: CreateValidReactants(),
                    products: CreateValidProducts(),
                    producedHeat: 1
                );

            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            Assert.That(
                result.IsValid,
                Is.True
            );
        }

        private ReactionDefinition CreateValidReaction()
        {
            return CreateReaction(
                reactionId: "hydrogen_combustion",
                reactants: CreateValidReactants(),
                products: CreateValidProducts(),
                producedHeat: 1
            );
        }

        private ReactionCardAmount[] CreateValidReactants()
        {
            return new[]
            {
                new ReactionCardAmount(
                    hydrogen,
                    2
                ),
                new ReactionCardAmount(
                    oxygen,
                    1
                )
            };
        }

        private ReactionCardAmount[] CreateValidProducts()
        {
            return new[]
            {
                new ReactionCardAmount(
                    water,
                    2
                )
            };
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

        private static void AssertInvalid(
            ReactionDefinitionValidationResult result,
            ReactionDefinitionValidationFailure expectedFailure
        )
        {
            Assert.That(
                result.IsValid,
                Is.False
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(expectedFailure)
            );
        }
    }
}