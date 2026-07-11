using UnityEngine;

namespace Catalyst.Cards.Presentation
{
    [CreateAssetMenu(
        fileName = "CardStyleMap",
        menuName = "Catalyst/Cards/Card Style Map"
    )]
    public sealed class CardStyleMap : ScriptableObject
    {
        [Header("Styles")]
        [SerializeField] private CardStyle elementalStyle = new CardStyle();
        [SerializeField] private CardStyle compoundStyle = new CardStyle();
        [SerializeField] private CardStyle mixtureStyle = new CardStyle();

        public bool TryGetStyle(CardType cardType, out CardStyle style)
        {
            switch (cardType)
            {
                case CardType.Elemental:
                    style = elementalStyle;
                    break;

                case CardType.Compound:
                    style = compoundStyle;
                    break;

                case CardType.Mixture:
                    style = mixtureStyle;
                    break;

                default:
                    style = null;
                    break;
            }

            return style != null;
        }

        public CardStyle GetStyle(CardType cardType)
        {
            if (TryGetStyle(cardType, out CardStyle style))
            {
                return style;
            }

            Debug.LogError(
                $"No card style was configured for card type '{cardType}'.",
                this
            );

            return null;
        }
    }
}