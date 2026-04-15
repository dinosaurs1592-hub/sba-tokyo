using System;
using SBATokyo.Prototype.Gameplay;
using UnityEngine;

namespace SBATokyo.Prototype.Core
{
    public class PrototypeGameManager : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float startSpeed = 8f;
        [SerializeField] private float maxSpeed = 20f;
        [SerializeField] private float pushAmount = 2f;
        [SerializeField] private float idleDecayDelay = 1f;
        [SerializeField] private float speedDecayPerSecond = 2f;
        [SerializeField] private float landingWindowDuration = 0.15f;
        [SerializeField] private float perfectLandingWindow = 0.08f;
        [SerializeField] private float speedBonus = 1.5f;
        [SerializeField] private int railTrickScore = 150;

        public float CurrentSpeed { get; private set; }
        public float MaxSpeed => maxSpeed;
        public float DistanceTravelled { get; private set; }
        public bool IsGameOver { get; private set; }
        public PlayerState CurrentState { get; private set; } = PlayerState.Running;
        public int ComboCount { get; private set; }
        public int Score { get; private set; }
        public float LandingWindowDuration => landingWindowDuration;

        public event Action RunReset;
        public event Action GameOverTriggered;

        private float lastPushTime;
        private bool speedDecayPaused;

        private void Awake()
        {
            ResetRun();
        }

        private void Update()
        {
            if (IsGameOver)
            {
                return;
            }

            CurrentState = PlayerState.Running;
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
            if (IsGameOver)
            {
                return;
            }

            CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + pushAmount);
            lastPushTime = Time.time;
        }

        public void SetState(PlayerState state)
        {
            if (!IsGameOver)
            {
                CurrentState = state;
            }
        }

        public void SetSpeedDecayPaused(bool paused)
        {
            speedDecayPaused = paused;
        }

        public void RegisterLandingResult(PrototypeSurfaceType surfaceType, PrototypeLandingActionType actionType, float elapsedSinceLanding)
        {
            bool withinWindow = elapsedSinceLanding <= landingWindowDuration;
            bool perfectTiming = elapsedSinceLanding <= perfectLandingWindow;

            if (!withinWindow)
            {
                ComboCount = 0;
                return;
            }

            bool success = surfaceType switch
            {
                PrototypeSurfaceType.Ground => actionType == PrototypeLandingActionType.Tap || actionType == PrototypeLandingActionType.Swipe || actionType == PrototypeLandingActionType.Jump,
                PrototypeSurfaceType.Object => actionType == PrototypeLandingActionType.Tap,
                PrototypeSurfaceType.Rail => actionType == PrototypeLandingActionType.Tap || actionType == PrototypeLandingActionType.Swipe || actionType == PrototypeLandingActionType.Jump,
                _ => false,
            };

            if (!success)
            {
                ComboCount = 0;
                return;
            }

            ComboCount += 1;
            Score += perfectTiming ? 200 : 100;

            if (perfectTiming)
            {
                CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + speedBonus);
            }
        }

        public void RegisterRailTrick()
        {
            ComboCount += 1;
            Score += railTrickScore;
        }

        public void TriggerGameOver()
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            CurrentSpeed = 0f;
            CurrentState = PlayerState.GameOver;
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
            lastPushTime = Time.time;
            RunReset?.Invoke();
        }
    }
}
