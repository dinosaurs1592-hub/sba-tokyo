using SBATokyo.Core;
using UnityEngine;

namespace SBATokyo.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class RunnerController : MonoBehaviour
    {
        [SerializeField] private SBAGameManager gameManager;
        [SerializeField] private SBAInputManager inputManager;
        [SerializeField] private float laneWidth = 3f;
        [SerializeField] private float laneChangeSpeed = 10f;
        [SerializeField] private float forwardSpeedMultiplier = 1f;
        [SerializeField] private float groundStickVelocity = -2f;
        [SerializeField] private float jumpVelocity = 8f;
        [SerializeField] private float groundProbeDistance = 1.3f;

        private CharacterController cc;
        private int currentLane;
        private Vector3 moveVelocity;
        private float verticalVelocity;
        private float startY;
        private bool wasGrounded;
        private float timeSinceLanding;
        private bool landingWindowOpen;
        private RoadSurface currentSurface;
        private LandingActionType bufferedAction;
        private float bufferedActionAge = float.MaxValue;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            startY = transform.position.y;
        }

        private void OnEnable()
        {
            if (inputManager == null) return;
            inputManager.LaneChangeRequested += ChangeLane;
            inputManager.PushRequested       += Push;
            inputManager.RestartRequested    += RestartRun;
            inputManager.TapRequested        += Tap;
            inputManager.JumpRequested       += Jump;
            inputManager.RailTrickRequested  += RailTrick;
        }

        private void OnDisable()
        {
            if (inputManager == null) return;
            inputManager.LaneChangeRequested -= ChangeLane;
            inputManager.PushRequested       -= Push;
            inputManager.RestartRequested    -= RestartRun;
            inputManager.TapRequested        -= Tap;
            inputManager.JumpRequested       -= Jump;
            inputManager.RailTrickRequested  -= RailTrick;
        }

        private void Update()
        {
            if (gameManager == null || gameManager.IsGameOver) return;

            float targetX = currentLane * laneWidth;
            float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
            bool grounded = cc.isGrounded || ProbeGround();

            UpdateSurfaceState(grounded);

            moveVelocity.x = (newX - transform.position.x) / Time.deltaTime;
            verticalVelocity = grounded
                ? groundStickVelocity
                : verticalVelocity + Physics.gravity.y * Time.deltaTime;
            moveVelocity.y = verticalVelocity;
            moveVelocity.z = gameManager.CurrentSpeed * forwardSpeedMultiplier;

            cc.Move(moveVelocity * Time.deltaTime);
            wasGrounded = grounded;
        }

        private void ChangeLane(int dir)
        {
            if (gameManager == null || gameManager.IsGameOver) return;
            currentLane = Mathf.Clamp(currentLane + dir, -1, 1);
            BufferAction(LandingActionType.Swipe);
        }

        private void Push()       => gameManager?.ApplyPush();
        private void Tap()        => BufferAction(LandingActionType.Tap);

        private void Jump()
        {
            if (gameManager == null || gameManager.IsGameOver) return;
            if (cc.isGrounded || landingWindowOpen)
            {
                verticalVelocity = jumpVelocity;
                landingWindowOpen = false;
                gameManager.SetState(PlayerState.Airborne);
                BufferAction(LandingActionType.Jump);
            }
        }

        private void RailTrick()
        {
            if (currentSurface == null || currentSurface.SurfaceType != SurfaceType.Rail) return;
            BufferAction(LandingActionType.RailTrick);
            gameManager?.RegisterRailTrick();
        }

        private void RestartRun()
        {
            if (gameManager == null) return;
            currentLane = 0;
            verticalVelocity = 0f;
            cc.enabled = false;
            transform.position = new Vector3(0f, startY, 0f);
            cc.enabled = true;
            bufferedAction = LandingActionType.None;
            bufferedActionAge = float.MaxValue;
            timeSinceLanding = 0f;
            landingWindowOpen = false;
            gameManager.ResetRun();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.GetComponent<Obstacle>() != null)
                gameManager?.TriggerGameOver();
        }

        private bool ProbeGround()
        {
            var origin = transform.position + Vector3.up * 0.2f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundProbeDistance))
            {
                currentSurface = hit.collider.GetComponent<RoadSurface>();
                return currentSurface != null;
            }
            currentSurface = null;
            return false;
        }

        private void UpdateSurfaceState(bool grounded)
        {
            bufferedActionAge += Time.deltaTime;

            if (!grounded)
            {
                landingWindowOpen = false;
                gameManager.SetSpeedDecayPaused(false);
                gameManager.SetState(PlayerState.Airborne);
                return;
            }

            if (!wasGrounded && grounded)
            {
                landingWindowOpen = true;
                timeSinceLanding = 0f;
                EvaluateLanding();
            }
            else if (landingWindowOpen)
            {
                timeSinceLanding += Time.deltaTime;
                if (timeSinceLanding > gameManager.LandingWindowDuration)
                {
                    landingWindowOpen = false;
                    bufferedAction = LandingActionType.None;
                }
            }

            if (currentSurface != null && currentSurface.SurfaceType == SurfaceType.Rail)
            {
                gameManager.SetState(PlayerState.Grinding);
                gameManager.SetSpeedDecayPaused(true);
                return;
            }

            gameManager.SetSpeedDecayPaused(false);
            gameManager.SetState(landingWindowOpen ? PlayerState.LandingWindow : PlayerState.Running);
        }

        private void EvaluateLanding()
        {
            var st = currentSurface == null ? SurfaceType.Ground : currentSurface.SurfaceType;
            gameManager.RegisterLandingResult(st, bufferedAction, bufferedActionAge);
            bufferedAction = LandingActionType.None;
            bufferedActionAge = float.MaxValue;
        }

        private void BufferAction(LandingActionType action)
        {
            bufferedAction = action;
            bufferedActionAge = 0f;
        }
    }
}
