using System;
using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Tests.EditMode.Common.Doubles;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Common.Creation
{
    internal sealed class TestCardFactory
    {
        private readonly CardInstanceFactory instanceFactory;
        private readonly List<UnityEngine.Object> createdObjects;

        public TestCardFactory()
        {
            instanceFactory =
                new CardInstanceFactory(
                    new SequentialCardInstanceIdSource()
                );

            createdObjects =
                new List<UnityEngine.Object>();
        }

        public CardDefinition CreateDefinition()
        {
            CardDefinition definition =
                ScriptableObject
                    .CreateInstance<CardDefinition>();

            createdObjects.Add(definition);

            return definition;
        }

        public CardInstance CreateInstance(
            CardDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition)
                );
            }

            return instanceFactory.Create(definition);
        }

        public CardInstance[] CreateInstances(
            CardDefinition definition,
            int quantity)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition)
                );
            }

            if (quantity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantity)
                );
            }

            var instances =
                new CardInstance[quantity];

            for (int index = 0;
                 index < quantity;
                 index++)
            {
                instances[index] =
                    instanceFactory.Create(definition);
            }

            return instances;
        }

        public void DisposeCreatedDefinitions()
        {
            foreach (UnityEngine.Object createdObject
                     in createdObjects)
            {
                if (createdObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        createdObject
                    );
                }
            }

            createdObjects.Clear();
        }
    }
}