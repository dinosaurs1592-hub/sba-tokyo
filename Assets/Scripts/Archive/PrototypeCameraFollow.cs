using UnityEngine;
using SBATokyo.Prototype.Core;

namespace SBATokyo.Prototype.Gameplay
{
    public class PrototypeCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private PrototypeGameManager gameManager;
        [SerializeField] private Vector3 offset = new Vector3(0f, 2.1f, -5.2f);
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.2f, 20f);
        [SerializeField] private float followSmoothness = 10f;
        [SerializeField] private float baseFieldOfView = 64f;
        [SerializeField] private float maxFieldOfView = 82f;
        [SerializeField] private float speedShakeAmplitude = 0.05f;
        [SerializeField] private float speedShakeFrequency = 13f;

        private Camera attachedCamera;

        private void Awake()
        {
            attachedCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float speedRatio = gameManager == null || gameManager.MaxSpeed <= 0f
                ? 0f
                : Mathf.Clamp01(gameManager.CurrentSpeed / gameManager.MaxSpeed);

            float shake = Mathf.Sin(Time.time * speedShakeFrequency) * speedShakeAmplitude * speedRatio;
            float forwardLift = Mathf.Lerp(0f, 0.55f, speedRatio);
            Vector3 desiredPosition = target.position + offset + new Vector3(0f, shake, 0f);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSmoothness * Time.deltaTime));

            Vector3 lookTarget = target.position + lookOffset + new Vector3(0f, forwardLift, 0f);
            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

            if (attachedCamera != null)
            {
                attachedCamera.fieldOfView = Mathf.Lerp(baseFieldOfView, maxFieldOfView, speedRatio);
            }
        }
    }
}
