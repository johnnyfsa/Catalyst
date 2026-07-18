using System;
using Catalyst.Reactions.Runtime.Resolution;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class
        ReactionExecutionValidationResultTests
    {
        [Test]
        public void Valid_ReturnsValidResult()
        {
            ReactionExecutionValidationResult result =
                ReactionExecutionValidationResult.Valid();

            Assert.That(
                result.IsValid,
                Is.True
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure.None
                )
            );
        }

        [TestCase(
            ReactionResolutionFailure.SessionIsNull
        )]
        [TestCase(
            ReactionResolutionFailure.PlanIsNull
        )]
        [TestCase(
            ReactionResolutionFailure
                .DuplicateReactantInstance
        )]
        [TestCase(
            ReactionResolutionFailure
                .ReactantDoesNotBelongToSession
        )]
        [TestCase(
            ReactionResolutionFailure
                .ReactantIsNotOnReactionTable
        )]
        [TestCase(
            ReactionResolutionFailure.InsufficientHeat
        )]
        [TestCase(
            ReactionResolutionFailure
                .InsufficientProductCapacity
        )]
        [TestCase(
            ReactionResolutionFailure.InsufficientElectricity
        )]

        public void Invalid_PreservesExecutionFailure(
            ReactionResolutionFailure failure
        )
        {
            ReactionExecutionValidationResult result =
                ReactionExecutionValidationResult.Invalid(
                    failure
                );

            Assert.That(
                result.IsValid,
                Is.False
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(failure)
            );

        }

        [Test]
        public void Invalid_WithNone_Throws()
        {
            Assert.That(
                () => ReactionExecutionValidationResult
                    .Invalid(
                        ReactionResolutionFailure.None
                    ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [TestCase(
            ReactionResolutionFailure.ReactionIsNull
        )]
        [TestCase(
            ReactionResolutionFailure
                .ReactionDefinitionIsInvalid
        )]
        [TestCase(
            ReactionResolutionFailure.TableDoesNotMatch
        )]
        public void Invalid_WithPlanningFailure_Throws(
            ReactionResolutionFailure failure
        )
        {
            Assert.That(
                () => ReactionExecutionValidationResult
                    .Invalid(failure),
                Throws.TypeOf<ArgumentException>()
            );
        }
    }
}