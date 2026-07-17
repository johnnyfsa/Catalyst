using System;
using Catalyst.Cards.Runtime.Resources;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Resources
{
    public sealed class ResourceCounterRuntimeTests
    {
        [Test]
        public void Constructor_WithoutInitialAmount_StartsAtZero()
        {
            var counter = new ResourceCounterRuntime();

            Assert.That(
                counter.Amount,
                Is.EqualTo(0)
            );
        }

        [Test]
        public void Constructor_WithPositiveInitialAmount_PreservesAmount()
        {
            var counter =
                new ResourceCounterRuntime(3);

            Assert.That(
                counter.Amount,
                Is.EqualTo(3)
            );
        }

        [Test]
        public void Constructor_WithNegativeInitialAmount_Throws()
        {
            Assert.That(
                () => new ResourceCounterRuntime(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }

        [Test]
        public void CanConsume_WithZero_ReturnsTrue()
        {
            var counter =
                new ResourceCounterRuntime(2);

            Assert.That(
                counter.CanConsume(0),
                Is.True
            );
        }

        [Test]
        public void CanConsume_WithExactAmount_ReturnsTrue()
        {
            var counter =
                new ResourceCounterRuntime(2);

            Assert.That(
                counter.CanConsume(2),
                Is.True
            );
        }

        [Test]
        public void CanConsume_WithInsufficientAmount_ReturnsFalse()
        {
            var counter =
                new ResourceCounterRuntime(2);

            Assert.That(
                counter.CanConsume(3),
                Is.False
            );
        }

        [Test]
        public void CanConsume_WithNegativeAmount_ReturnsFalse()
        {
            var counter =
                new ResourceCounterRuntime(2);

            Assert.That(
                counter.CanConsume(-1),
                Is.False
            );
        }

        [Test]
        public void TryConsume_WithAvailableAmount_DecreasesAmount()
        {
            var counter =
                new ResourceCounterRuntime(3);

            bool succeeded =
                counter.TryConsume(2);

            Assert.That(succeeded, Is.True);

            Assert.That(
                counter.Amount,
                Is.EqualTo(1)
            );
        }

        [Test]
        public void TryConsume_WithExactAmount_ReducesAmountToZero()
        {
            var counter =
                new ResourceCounterRuntime(2);

            bool succeeded =
                counter.TryConsume(2);

            Assert.That(succeeded, Is.True);

            Assert.That(
                counter.Amount,
                Is.EqualTo(0)
            );
        }

        [Test]
        public void TryConsume_WithInsufficientAmount_DoesNotChangeAmount()
        {
            var counter =
                new ResourceCounterRuntime(2);

            bool succeeded =
                counter.TryConsume(3);

            Assert.That(succeeded, Is.False);

            Assert.That(
                counter.Amount,
                Is.EqualTo(2)
            );
        }

        [Test]
        public void TryConsume_WithNegativeAmount_Throws()
        {
            var counter =
                new ResourceCounterRuntime(2);

            Assert.That(
                () => counter.TryConsume(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }

        [Test]
        public void Add_WithPositiveAmount_IncreasesAmount()
        {
            var counter =
                new ResourceCounterRuntime(2);

            counter.Add(3);

            Assert.That(
                counter.Amount,
                Is.EqualTo(5)
            );
        }

        [Test]
        public void Add_WithZero_DoesNotChangeAmount()
        {
            var counter =
                new ResourceCounterRuntime(2);

            counter.Add(0);

            Assert.That(
                counter.Amount,
                Is.EqualTo(2)
            );
        }

        [Test]
        public void Add_WithNegativeAmount_Throws()
        {
            var counter =
                new ResourceCounterRuntime(2);

            Assert.That(
                () => counter.Add(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }

        [Test]
        public void Add_WhenResultOverflows_Throws()
        {
            var counter =
                new ResourceCounterRuntime(int.MaxValue);

            Assert.That(
                () => counter.Add(1),
                Throws.TypeOf<OverflowException>()
            );
        }
    }
}