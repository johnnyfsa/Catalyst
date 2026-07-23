using Catalyst.Reactions.Runtime;
using Catalyst.Reactions.Runtime.Resolution;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class ReactionResolutionResultTests
    {
        [Test]
        public void Success_ReturnsSucceededResult()
        {
            ReactionResolutionResult result =
                ReactionResolutionResult.Success();

            Assert.That(
                result.Succeeded,
                Is.True
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

        [TestCase(
    ReactionResolutionFailure.SessionIsNull
)]
        [TestCase(
    ReactionResolutionFailure.PlanIsNull
)]
        [TestCase(
    ReactionResolutionFailure.ReactionIsNull
)]
        [TestCase(
    ReactionResolutionFailure.ReactionUnavailable
)]
        [TestCase(
    ReactionResolutionFailure.TableDoesNotMatch
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
    ReactionResolutionFailure
        .ReactantInstanceDoesNotMatchSessionCard
)]
        public void Fail_PreservesExpectedFailure(
    ReactionResolutionFailure failure
)
        {
            ReactionResolutionResult result =
                ReactionResolutionResult.Fail(
                    failure
                );

            Assert.That(
                result.Succeeded,
                Is.False
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
        public void InvalidDefinition_PreservesValidationFailure()
        {
            ReactionResolutionResult result =
                ReactionResolutionResult
                    .InvalidDefinition(
                        ReactionDefinitionValidationFailure
                            .DuplicateReactantDefinition
                    );

            Assert.That(
                result.Succeeded,
                Is.False
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
        public void Fail_WithNone_Throws()
        {
            Assert.That(
                () => ReactionResolutionResult.Fail(
                    ReactionResolutionFailure.None
                ),
                Throws.ArgumentException
            );
        }

        [Test]
        public void Fail_WithInvalidDefinitionFailure_Throws()
        {
            Assert.That(
                () => ReactionResolutionResult.Fail(
                    ReactionResolutionFailure
                        .ReactionDefinitionIsInvalid
                ),
                Throws.ArgumentException
            );
        }

        [Test]
        public void InvalidDefinition_WithNone_Throws()
        {
            Assert.That(
                () => ReactionResolutionResult
                    .InvalidDefinition(
                        ReactionDefinitionValidationFailure.None
                    ),
                Throws.ArgumentException
            );
        }
    }
}