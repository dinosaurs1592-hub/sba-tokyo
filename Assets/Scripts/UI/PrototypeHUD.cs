using SBATokyo.Prototype.Core;
using SBATokyo.Prototype.Gameplay;
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
        [SerializeField] private Text warningText;
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
                stateText.text = gameManager.IsGameOver
                    ? "GAME OVER | Press R to restart"
                    : $"State: {gameManager.CurrentState} | Surface: {gameManager.CurrentSurfaceType}";
            }

            if (comboHintText != null)
            {
                comboHintText.text = gameManager.IsGameOver
                    ? "Prototype reset ready"
                    : gameManager.LastRuleMessage;
            }

            if (diagnosticsText != null)
            {
                diagnosticsText.text =
                    $"Controls: A/D lane | S push | Space jump | Enter tap | F trick | Window: {gameManager.LandingWindowRemaining:0.00}s | Speed Ratio: {gameManager.CurrentSpeed / gameManager.MaxSpeed:0.00}";
            }

            if (warningText != null)
            {
                warningText.text = "Traffic clear";
                warningText.color = new Color(0.2f, 0.24f, 0.28f);
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
