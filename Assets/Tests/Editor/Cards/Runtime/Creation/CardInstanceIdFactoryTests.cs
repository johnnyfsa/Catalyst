using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Creation;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Creation
{
    public sealed class CardInstanceFactoryTests
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
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Create_UsesIdSourceAndDefinition()
        {
            Guid expectedId =
                Guid.Parse(
                    "b85241e3-e454-4bb3-8fde-1b64482482cb"
                );

            FixedIdSource idSource =
                new FixedIdSource(expectedId);

            CardInstanceFactory factory =
                new CardInstanceFactory(idSource);

            CardInstance instance =
                factory.Create(definition);

            Assert.That(
                instance.InstanceId,
                Is.EqualTo(expectedId)
            );

            Assert.That(
                instance.Definition,
                Is.SameAs(definition)
            );
        }

        [Test]
        public void Create_WithNullDefinition_Throws()
        {
            CardInstanceFactory factory =
                new CardInstanceFactory(
                    new FixedIdSource(Guid.NewGuid())
                );

            Assert.That(
                () => factory.Create(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Create_WhenIdSourceReturnsEmptyId_Throws()
        {
            CardInstanceFactory factory =
                new CardInstanceFactory(
                    new FixedIdSource(Guid.Empty)
                );

            Assert.That(
                () => factory.Create(definition),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void Constructor_WithNullIdSource_Throws()
        {
            Assert.That(
                () => new CardInstanceFactory(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        private sealed class FixedIdSource
            : ICardInstanceIdSource
        {
            private readonly Guid id;

            public FixedIdSource(Guid id)
            {
                this.id = id;
            }

            public Guid NextId()
            {
                return id;
            }
        }
    }
}