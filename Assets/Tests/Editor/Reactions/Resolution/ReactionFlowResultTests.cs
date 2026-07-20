using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Reactions.Runtime.Resolution;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class ReactionFlowResultTests
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

            ReactionFlowResult result =
                ReactionFlowResult.Success(
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
                () => ReactionFlowResult.Success(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Success_WithEmptyProducts_Throws()
        {
            Assert.That(
                () => ReactionFlowResult.Success(
                    Array.Empty<CardInstance>()
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Success_WithNullProductEntry_Throws()
        {
            Assert.That(
                () => ReactionFlowResult.Success(
                    new CardInstance[]
                    {
                        null
                    }
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
        [TestCase(
            ReactionResolutionFailure
                .InsufficientHeat
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
            ReactionFlowResult result =
                ReactionFlowResult.Fail(
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
                () => ReactionFlowResult.Fail(
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