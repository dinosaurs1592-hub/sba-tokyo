using System;
using SBATokyo.Gameplay;
using UnityEngine;

namespace SBATokyo.Core
{
    public class SBAGameManager : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float startSpeed = 24f;
        [SerializeField] private float maxSpeed = 52f;
        [SerializeField] private float pushAmount = 6f;
        [SerializeField] private float idleDecayDelay = 1.2f;
        [SerializeField] private float speedDecayPerSecond = 3.4f;

        [Header("Landing")]
        [SerializeField] private float landingWindowDuration = 0.15f;
        [SerializeField] private float perfectLandingWindow = 0.08f;
        [SerializeField] private float speedBonus = 4f;

        [Header("Scoring")]
        [SerializeField] private int railTrickScore = 150;
        [SerializeField] private float railTrickCooldown = 0.45f;

        public float CurrentSpeed { get; private set; }
        public float MaxSpeed => maxSpeed;
        public float DistanceTravelled { get; private set; }
        public bool IsGameOver { get; private set; }
        public PlayerState CurrentState { get; private set; } = PlayerState.Running;
        public int ComboCount { get; private set; }
        public int Score { get; private set; }
        public float LandingWindowDuration => landingWindowDuration;
        public float LandingWindowRemaining { get; private set; }
        public SurfaceType CurrentSurfaceType { get; private set; } = SurfaceType.Ground;
        public string LastRuleMessage { get; private set; } = "Run the course.";

        public event Action RunReset;
        public event Action GameOverTriggered;

        private float lastPushTime;
        private float lastRailTrickTime = float.NegativeInfinity;
        private bool speedDecayPaused;

        private void Awake()
        {
            ResetRun();
        }

        private void Update()
        {
            if (IsGameOver) return;

            DistanceTravelled += CurrentSpeed * Time.deltaTime;

            if (!speedDecayPaused && Time.time - lastPushTime >= idleDecayDelay)
            {
                CurrentSpeed = Mathf.Max(0f, CurrentSpeed - speedDecayPerSecond * Time.deltaTime);
            }

            if (CurrentSpeed <= 0f)
            {
                TriggerGameOver();
            }
        }

        public void ApplyPush()
        {
            if (IsGameOver) return;
            CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + pushAmount);
            lastPushTime = Time.time;
        }

        public void SetState(PlayerState state)
        {
            if (!IsGameOver) CurrentState = state;
        }

        public void SetSpeedDecayPaused(bool paused)
        {
            speedDecayPaused = paused;
        }

        public void BeginLandingWindow(SurfaceType surfaceType)
        {
            CurrentSurfaceType = surfaceType;
            CurrentState = PlayerState.LandingWindow;
            LandingWindowRemaining = landingWindowDuration;
            LastRuleMessage = surfaceType switch
            {
                SurfaceType.Object => "Object ahead. Tap on landing.",
                SurfaceType.Rail   => "Rail landing. Swipe, tap, or jump. Press F while grinding.",
                _                  => "Ground landing. Tap, swipe, or jump on landing.",
            };
        }

        public void UpdateLandingWindow(float elapsed)
        {
            LandingWindowRemaining = Mathf.Max(0f, landingWindowDuration - elapsed);
        }

        public void ExpireLandingWindow(SurfaceType surfaceType)
        {
            CurrentSurfaceType = surfaceType;
            LandingWindowRemaining = 0f;
            ComboCount = 0;
            LastRuleMessage = surfaceType == SurfaceType.Object
                ? "Object landing missed. Tap was required."
                : $"{SurfaceLabel(surfaceType)} landing missed. Combo reset.";
        }

        public void RegisterLandingResult(SurfaceType surfaceType, LandingActionType actionType, float elapsedSinceLanding)
        {
            bool withinWindow = elapsedSinceLanding <= landingWindowDuration;
            bool perfectTiming = elapsedSinceLanding <= perfectLandingWindow;
            CurrentSurfaceType = surfaceType;
            LandingWindowRemaining = 0f;

            if (!withinWindow)
            {
                ComboCount = 0;
                LastRuleMessage = $"{SurfaceLabel(surfaceType)} missed. Combo reset.";
                return;
            }

            bool success = surfaceType switch
            {
                SurfaceType.Ground => actionType is LandingActionType.Tap or LandingActionType.Swipe or LandingActionType.Jump,
                SurfaceType.Object => actionType == LandingActionType.Tap,
                SurfaceType.Rail   => actionType is LandingActionType.Tap or LandingActionType.Swipe or LandingActionType.Jump,
                _ => false,
            };

            if (!success)
            {
                ComboCount = 0;
                LastRuleMessage = surfaceType == SurfaceType.Object
                    ? "Object landing failed. Tap only."
                    : $"{SurfaceLabel(surfaceType)} landing failed.";
                return;
            }

            ComboCount += 1;
            Score += perfectTiming ? 200 : 100;
            if (perfectTiming) CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + speedBonus);

            LastRuleMessage = $"{(perfectTiming ? "Perfect" : "Good")} {SurfaceLabel(surfaceType)} landing.";
        }

        public void RegisterRailTrick()
        {
            if (Time.time - lastRailTrickTime < railTrickCooldown)
            {
                LastRuleMessage = "Rail trick cooling down.";
                return;
            }

            lastRailTrickTime = Time.time;
            ComboCount += 1;
            Score += railTrickScore;
            LastRuleMessage = $"Rail trick! +{railTrickScore}";
        }

        public void TriggerGameOver()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            CurrentSpeed = 0f;
            CurrentState = PlayerState.GameOver;
            LandingWindowRemaining = 0f;
            LastRuleMessage = "Game over. Press R to restart.";
            GameOverTriggered?.Invoke();
        }

        public void ResetRun()
        {
            CurrentSpeed = startSpeed;
            DistanceTravelled = 0f;
            IsGameOver = false;
            CurrentState = PlayerState.Running;
            ComboCount = 0;
            Score = 0;
            speedDecayPaused = false;
            LandingWindowRemaining = 0f;
            CurrentSurfaceType = SurfaceType.Ground;
            LastRuleMessage = "Run the course. Tap, swipe, or jump on landing.";
            lastPushTime = Time.time;
            lastRailTrickTime = float.NegativeInfinity;
            RunReset?.Invoke();
        }

        private static string SurfaceLabel(SurfaceType t) => t switch
        {
            SurfaceType.Object => "Object",
            SurfaceType.Rail   => "Rail",
            _                  => "Ground",
        };
    }
}
