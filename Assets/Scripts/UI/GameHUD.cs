using SBATokyo.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SBATokyo.UI
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private SBAGameManager gameManager;
        [SerializeField] private Text speedText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text diagnosticsText;
        [SerializeField] private Slider speedSlider;

        private void Update()
        {
            if (gameManager == null) return;

            if (speedText != null)
                speedText.text = $"Speed: {gameManager.CurrentSpeed:0.0}";

            if (distanceText != null)
                distanceText.text = $"Dist: {gameManager.DistanceTravelled:0}  Combo: {gameManager.ComboCount}  Score: {gameManager.Score}";

            if (stateText != null)
                stateText.text = gameManager.IsGameOver
                    ? "GAME OVER | Press R to restart"
                    : $"State: {gameManager.CurrentState} | Surface: {gameManager.CurrentSurfaceType}";

            if (hintText != null)
                hintText.text = gameManager.LastRuleMessage;

            if (diagnosticsText != null)
                diagnosticsText.text =
                    $"A/D lane  S push  Space jump  Enter tap  F trick  |  Window: {gameManager.LandingWindowRemaining:0.00}s";

            if (speedSlider != null)
            {
                speedSlider.minValue = 0f;
                speedSlider.maxValue = gameManager.MaxSpeed;
                speedSlider.value = gameManager.CurrentSpeed;
            }
        }
    }
}
