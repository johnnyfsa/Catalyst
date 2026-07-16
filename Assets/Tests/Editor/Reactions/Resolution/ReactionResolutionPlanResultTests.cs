using System;
using Catalyst.Cards.Definitions;
using Catalyst.Reactions.Definitions;
using Catalyst.Reactions.Runtime;
using Catalyst.Reactions.Runtime.Resolution;
using Catalyst.Tests.EditMode.Common.Creation;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class ReactionResolutionPlanResultTests
    {
        private TestCardFactory cardFactory;
        private TestReactionFactory reactionFactory;

        private ReactionResolutionPlan plan;

        [SetUp]
        public void SetUp()
        {
            cardFactory = new TestCardFactory();
            reactionFactory = new TestReactionFactory();

            CardDefinition hydrogen =
                cardFactory.CreateDefinition();

            CardDefinition oxygen =
                cardFactory.CreateDefinition();

            CardDefinition water =
                cardFactory.CreateDefinition();

            plan =
                new ReactionResolutionPlan(
                    new[]
                    {
                        cardFactory.CreateInstance(
                            hydrogen
                        ),
                        cardFactory.CreateInstance(
                            hydrogen
                        ),
                        cardFactory.CreateInstance(
                            oxygen
                        )
                    },
                    new[]
                    {
                        new ReactionProductPlanEntry(
                            water,
                            2
                        )
                    },
                    requiredHeat: 0,
                    producedHeat: 1
                );
        }

        [TearDown]
        public void TearDown()
        {
            reactionFactory?.Dispose();
            reactionFactory = null;

            cardFactory?.DisposeCreatedDefinitions();
            cardFactory = null;
        }

        [Test]
        public void Success_PreservesPlan()
        {
            ReactionResolutionPlanResult result =
                ReactionResolutionPlanResult.Success(
                    plan
                );

            Assert.That(
                result.Succeeded,
                Is.True
            );

            Assert.That(
                result.Plan,
                Is.SameAs(plan)
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure.None
                )
            );

            Assert.That(
                result.DefinitionFailure,
                Is.EqualTo(
                    ReactionDefinitionValidationFailure.None
                )
            );
        }

        [Test]
        public void Success_WithNullPlan_Throws()
        {
            Assert.That(
                () => ReactionResolutionPlanResult
                    .Success(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [TestCase(
            ReactionResolutionFailure.ReactionIsNull
        )]
        [TestCase(
            ReactionResolutionFailure.TableDoesNotMatch
        )]
        [TestCase(
            ReactionResolutionFailure.InsufficientHeat
        )]
        [TestCase(
            ReactionResolutionFailure
                .InsufficientProductCapacity
        )]
        public void Fail_PreservesFailure(
            ReactionResolutionFailure failure
        )
        {
            ReactionResolutionPlanResult result =
                ReactionResolutionPlanResult.Fail(
                    failure
                );

            Assert.That(
                result.Succeeded,
                Is.False
            );

            Assert.That(
                result.Plan,
                Is.Null
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(failure)
            );

            Assert.That(
                result.DefinitionFailure,
                Is.EqualTo(
                    ReactionDefinitionValidationFailure.None
                )
            );
        }

        [Test]
        public void Fail_WithNone_Throws()
        {
            Assert.That(
                () => ReactionResolutionPlanResult.Fail(
                    ReactionResolutionFailure.None
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Fail_WithInvalidDefinition_Throws()
        {
            Assert.That(
                () => ReactionResolutionPlanResult.Fail(
                    ReactionResolutionFailure
                        .ReactionDefinitionIsInvalid
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void InvalidDefinition_PreservesValidationFailure()
        {
            ReactionResolutionPlanResult result =
                ReactionResolutionPlanResult
                    .InvalidDefinition(
                        ReactionDefinitionValidationFailure
                            .DuplicateReactantDefinition
                    );

            Assert.That(
                result.Succeeded,
                Is.False
            );

            Assert.That(
                result.Plan,
                Is.Null
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure
                        .ReactionDefinitionIsInvalid
                )
            );

            Assert.That(
                result.DefinitionFailure,
                Is.EqualTo(
                    ReactionDefinitionValidationFailure
                        .DuplicateReactantDefinition
                )
            );
        }

        [Test]
        public void InvalidDefinition_WithNone_Throws()
        {
            Assert.That(
                () => ReactionResolutionPlanResult
                    .InvalidDefinition(
                        ReactionDefinitionValidationFailure
                            .None
                    ),
                Throws.TypeOf<ArgumentException>()
            );
        }
    }
}