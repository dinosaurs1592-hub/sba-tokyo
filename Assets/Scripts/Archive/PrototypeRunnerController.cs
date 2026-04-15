using SBATokyo.Prototype.Core;
using UnityEngine;

namespace SBATokyo.Prototype.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PrototypeRunnerController : MonoBehaviour
    {
        [SerializeField] private PrototypeGameManager gameManager;
        [SerializeField] private PrototypeInputManager inputManager;
        [SerializeField] private float laneWidth = 3f;
        [SerializeField] private float laneChangeSpeed = 10f;
        [SerializeField] private float forwardSpeedMultiplier = 1f;
        [SerializeField] private float groundStickVelocity = -2f;
        [SerializeField] private float jumpVelocity = 8f;
        [SerializeField] private float groundProbeDistance = 1.3f;

        private CharacterController characterController;
        private int currentLane;
        private Vector3 moveVelocity;
        private float verticalVelocity;
        private float startY;
        private bool wasGrounded;
        private float timeSinceLanding;
        private bool landingWindowOpen;
        private PrototypeSurface currentSurface;
        private PrototypeLandingActionType bufferedLandingAction;
        private float bufferedLandingActionAge = float.MaxValue;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            startY = transform.position.y;
        }

        private void OnEnable()
        {
            if (inputManager == null)
            {
                return;
            }

            inputManager.LaneChangeRequested += ChangeLane;
            inputManager.PushRequested += Push;
            inputManager.RestartRequested += RestartRun;
            inputManager.TapRequested += Tap;
            inputManager.JumpRequested += Jump;
            inputManager.RailTrickRequested += RailTrick;
        }

        private void OnDisable()
        {
            if (inputManager == null)
            {
                return;
            }

            inputManager.LaneChangeRequested -= ChangeLane;
            inputManager.PushRequested -= Push;
            inputManager.RestartRequested -= RestartRun;
            inputManager.TapRequested -= Tap;
            inputManager.JumpRequested -= Jump;
            inputManager.RailTrickRequested -= RailTrick;
        }

        private void Update()
        {
            if (gameManager == null || gameManager.IsGameOver)
            {
                return;
            }

            float targetX = currentLane * laneWidth;
            float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
            float forwardSpeed = gameManager.CurrentSpeed * forwardSpeedMultiplier;
            bool grounded = characterController.isGrounded || ProbeGround();
            UpdateSurfaceState(grounded);

            moveVelocity.x = (newX - transform.position.x) / Time.deltaTime;
            verticalVelocity = grounded ? groundStickVelocity : verticalVelocity + Physics.gravity.y * Time.deltaTime;
            moveVelocity.y = verticalVelocity;
            moveVelocity.z = forwardSpeed;

            characterController.Move(moveVelocity * Time.deltaTime);
            wasGrounded = grounded;
        }

        private void ChangeLane(int direction)
        {
            if (gameManager == null || gameManager.IsGameOver)
            {
                return;
            }

            currentLane = Mathf.Clamp(currentLane + direction, -1, 1);
            BufferLandingAction(PrototypeLandingActionType.Swipe);
        }

        private void Push()
        {
            gameManager?.ApplyPush();
        }

        private void Tap()
        {
            BufferLandingAction(PrototypeLandingActionType.Tap);
        }

        private void Jump()
        {
            if (gameManager == null || gameManager.IsGameOver)
            {
                return;
            }

            if (characterController.isGrounded || landingWindowOpen)
            {
                verticalVelocity = jumpVelocity;
                landingWindowOpen = false;
                gameManager.SetState(PlayerState.Airborne);
                BufferLandingAction(PrototypeLandingActionType.Jump);
            }
        }

        private void RailTrick()
        {
            if (currentSurface == null || currentSurface.SurfaceType != PrototypeSurfaceType.Rail)
            {
                return;
            }

            BufferLandingAction(PrototypeLandingActionType.RailTrick);
            gameManager?.RegisterRailTrick();
        }

        private void RestartRun()
        {
            if (gameManager == null)
            {
                return;
            }

            currentLane = 0;
            verticalVelocity = 0f;
            characterController.enabled = false;
            transform.position = new Vector3(0f, startY, 0f);
            characterController.enabled = true;
            bufferedLandingAction = PrototypeLandingActionType.None;
            bufferedLandingActionAge = float.MaxValue;
            timeSinceLanding = 0f;
            landingWindowOpen = false;
            gameManager.ResetRun();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.GetComponent<PrototypeObstacle>() == null)
            {
                return;
            }

            gameManager?.TriggerGameOver();
        }

        private bool ProbeGround()
        {
            Vector3 origin = transform.position + Vector3.up * 0.2f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundProbeDistance))
            {
                currentSurface = hit.collider.GetComponent<PrototypeSurface>();
                return currentSurface != null;
            }

            currentSurface = null;
            return false;
        }

        private void UpdateSurfaceState(bool grounded)
        {
            bufferedLandingActionAge += Time.deltaTime;

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
                    bufferedLandingAction = PrototypeLandingActionType.None;
                }
            }

            if (currentSurface != null && currentSurface.SurfaceType == PrototypeSurfaceType.Rail)
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
            PrototypeSurfaceType surfaceType = currentSurface == null ? PrototypeSurfaceType.Ground : currentSurface.SurfaceType;
            gameManager.RegisterLandingResult(surfaceType, bufferedLandingAction, bufferedLandingActionAge);
            bufferedLandingAction = PrototypeLandingActionType.None;
            bufferedLandingActionAge = float.MaxValue;
        }

        private void BufferLandingAction(PrototypeLandingActionType actionType)
        {
            bufferedLandingAction = actionType;
            bufferedLandingActionAge = 0f;
        }
    }
}
