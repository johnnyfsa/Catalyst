using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.Game.UI
{
    public sealed class WaterTankView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Image waterFill;

        [SerializeField]
        private TMP_Text progressText;

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {

        }

        public void SetProgress(
            int currentAmount,
            int requiredAmount
        )
        {
            if (requiredAmount <= 0)
            {
                Debug.LogError(
                    "Water tank required amount must be greater than zero.",
                    this
                );

                return;
            }

            int safeCurrentAmount =
                Mathf.Clamp(
                    currentAmount,
                    0,
                    requiredAmount
                );

            float normalizedProgress =
                (float)safeCurrentAmount /
                requiredAmount;

            waterFill.fillAmount =
                normalizedProgress;

            progressText.text =
                $"{safeCurrentAmount} / {requiredAmount}";
        }

        private void ValidateReferences()
        {
            if (waterFill == null)
            {
                Debug.LogError(
                    $"{nameof(WaterTankView)} requires a water fill image.",
                    this
                );
            }

            if (progressText == null)
            {
                Debug.LogError(
                    $"{nameof(WaterTankView)} requires a progress text.",
                    this
                );
            }
        }
    }
}