using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SBATokyo.Prototype.Core
{
    public class PrototypeInputManager : MonoBehaviour
    {
        [SerializeField] private float minSwipeDistance = 60f;
        [SerializeField] private bool enableKeyboardFallback = true;
        [SerializeField] private bool enableMouseSwipeInEditor = true;

        public event Action<int> LaneChangeRequested;
        public event Action PushRequested;
        public event Action RestartRequested;
        public event Action TapRequested;
        public event Action JumpRequested;
        public event Action RailTrickRequested;

        private Vector2 touchStart;
        private bool trackingTouch;
        private Vector2 pointerStart;
        private bool trackingPointer;

        private void Update()
        {
            HandleKeyboard();
            HandleTouch();
            HandleMousePointer();
        }

        private void HandleKeyboard()
        {
            if (!enableKeyboardFallback)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (WasPressedThisFrame(keyboard.leftArrowKey) || WasPressedThisFrame(keyboard.aKey))
            {
                LaneChangeRequested?.Invoke(-1);
            }

            if (WasPressedThisFrame(keyboard.rightArrowKey) || WasPressedThisFrame(keyboard.dKey))
            {
                LaneChangeRequested?.Invoke(1);
            }

            if (WasPressedThisFrame(keyboard.downArrowKey) || WasPressedThisFrame(keyboard.sKey))
            {
                PushRequested?.Invoke();
            }

            if (WasPressedThisFrame(keyboard.spaceKey) || WasPressedThisFrame(keyboard.wKey) || WasPressedThisFrame(keyboard.upArrowKey))
            {
                JumpRequested?.Invoke();
            }

            if (WasPressedThisFrame(keyboard.fKey))
            {
                RailTrickRequested?.Invoke();
            }

            if (WasPressedThisFrame(keyboard.enterKey))
            {
                TapRequested?.Invoke();
            }

            if (WasPressedThisFrame(keyboard.rKey))
            {
                RestartRequested?.Invoke();
            }
        }

        private void HandleTouch()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return;
            }

            var touch = touchscreen.primaryTouch;
            if (!touch.press.isPressed && !trackingTouch)
            {
                return;
            }

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchStart = touch.position.ReadValue();
                trackingTouch = true;
                return;
            }

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                trackingTouch = false;
                return;
            }

            if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Ended || !trackingTouch)
            {
                return;
            }

            trackingTouch = false;
            Vector2 delta = touch.position.ReadValue() - touchStart;

            if (delta.magnitude < minSwipeDistance)
            {
                return;
            }

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                LaneChangeRequested?.Invoke(delta.x > 0f ? 1 : -1);
                return;
            }

            if (delta.y < 0f)
            {
                PushRequested?.Invoke();
            }
        }

        private void HandleMousePointer()
        {
            if (!enableMouseSwipeInEditor)
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                pointerStart = mouse.position.ReadValue();
                trackingPointer = true;
                TapRequested?.Invoke();
                return;
            }

            if (!trackingPointer || !mouse.leftButton.wasReleasedThisFrame)
            {
                return;
            }

            trackingPointer = false;
            Vector2 delta = mouse.position.ReadValue() - pointerStart;

            if (delta.magnitude < minSwipeDistance)
            {
                return;
            }

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                LaneChangeRequested?.Invoke(delta.x > 0f ? 1 : -1);
                return;
            }

            if (delta.y < 0f)
            {
                PushRequested?.Invoke();
                return;
            }

            if (delta.y > 0f)
            {
                JumpRequested?.Invoke();
            }
        }

        private static bool WasPressedThisFrame(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            return key != null && key.wasPressedThisFrame;
        }
    }
}
