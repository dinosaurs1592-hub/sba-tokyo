using SBATokyo.Prototype.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SBATokyo.Prototype.UI
{
    public class PrototypeHUD : MonoBehaviour
    {
        [SerializeField] private PrototypeGameManager gameManager;
        [SerializeField] private Text speedText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text comboHintText;
        [SerializeField] private Text diagnosticsText;
        [SerializeField] private Slider speedSlider;

        private void Update()
        {
            if (gameManager == null)
            {
                return;
            }

            if (speedText != null)
            {
                speedText.text = $"Speed: {gameManager.CurrentSpeed:0.0}";
            }

            if (distanceText != null)
            {
                distanceText.text = $"Distance: {gameManager.DistanceTravelled:0} | Combo: {gameManager.ComboCount} | Score: {gameManager.Score}";
            }

            if (stateText != null)
            {
                stateText.text = gameManager.IsGameOver ? "GAME OVER | Press R to restart" : "A/D or swipe sideways to change lanes";
            }

            if (comboHintText != null)
            {
                comboHintText.text = gameManager.IsGameOver
                    ? "Prototype reset ready"
                    : "S/down push | Space/up jump | Enter tap | F rail trick";
            }

            if (diagnosticsText != null)
            {
                diagnosticsText.text = $"State: {gameManager.CurrentState} | Speed Ratio: {gameManager.CurrentSpeed / gameManager.MaxSpeed:0.00}";
            }

            if (speedSlider != null)
            {
                speedSlider.minValue = 0f;
                speedSlider.maxValue = gameManager.MaxSpeed;
                speedSlider.value = gameManager.CurrentSpeed;
            }
        }
    }
}
