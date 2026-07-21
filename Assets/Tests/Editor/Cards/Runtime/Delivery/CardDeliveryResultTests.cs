using System;
using Catalyst.Cards.Runtime.Delivery;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Delivery
{
    public sealed class CardDeliveryResultTests
    {
        [Test]
        public void Success_CreatesSuccessfulResult()
        {
            CardDeliveryResult result =
                CardDeliveryResult.Success();

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(CardDeliveryFailure.None)
            );
        }

        [Test]
        public void Fail_CreatesFailedResult()
        {
            CardDeliveryResult result =
                CardDeliveryResult.Fail(
                    CardDeliveryFailure.CardNotInHand
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardDeliveryFailure.CardNotInHand
                )
            );
        }

        [Test]
        public void Fail_WithNoneFailure_Throws()
        {
            Assert.That(
                () => CardDeliveryResult.Fail(
                    CardDeliveryFailure.None
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }
    }
}