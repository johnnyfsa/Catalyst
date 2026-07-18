using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Reactions.Runtime.Resolution;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class ReactionExecutionResultTests
    {
        private CardDefinition definition;

        [SetUp]
        public void SetUp()
        {
            definition =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (definition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    definition
                );
            }
        }

        [Test]
        public void Success_ReturnsSuccessfulResult()
        {
            CardInstance product =
                CreateCard(1);

            ReactionExecutionResult result =
                ReactionExecutionResult.Success(
                    new[]
                    {
                        product
                    }
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure.None
                )
            );

            Assert.That(
                result.CreatedProducts.Count,
                Is.EqualTo(1)
            );

            Assert.That(
                result.CreatedProducts[0],
                Is.SameAs(product)
            );
        }

        [Test]
        public void Success_WithNullProducts_Throws()
        {
            Assert.That(
                () => ReactionExecutionResult.Success(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Success_WithEmptyProducts_Throws()
        {
            Assert.That(
                () => ReactionExecutionResult.Success(
                    Array.Empty<CardInstance>()
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Success_WithNullProductEntry_Throws()
        {
            Assert.That(
                () => ReactionExecutionResult.Success(
                    new CardInstance[]
                    {
                        null
                    }
                ),
                Throws.TypeOf<ArgumentException>()
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
                .ReactantInstanceDoesNotMatchSessionCard
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
                .InsufficientElectricity
        )]
        [TestCase(
            ReactionResolutionFailure
                .InsufficientProductCapacity
        )]
        public void Fail_PreservesFailure(
            ReactionResolutionFailure failure
        )
        {
            ReactionExecutionResult result =
                ReactionExecutionResult.Fail(
                    failure
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(failure)
            );

            Assert.That(
                result.CreatedProducts,
                Is.Empty
            );
        }

        [Test]
        public void Fail_WithNone_Throws()
        {
            Assert.That(
                () => ReactionExecutionResult.Fail(
                    ReactionResolutionFailure.None
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        private CardInstance CreateCard(
            int value
        )
        {
            string suffix =
                value.ToString("D12");

            Guid id =
                Guid.Parse(
                    $"00000000-0000-0000-0000-{suffix}"
                );

            return new CardInstance(
                id,
                definition
            );
        }
    }
}