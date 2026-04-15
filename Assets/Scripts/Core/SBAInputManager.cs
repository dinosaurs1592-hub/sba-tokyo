using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SBATokyo.Core
{
    public class SBAInputManager : MonoBehaviour
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
            if (!enableKeyboardFallback) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            if (Pressed(kb.leftArrowKey)  || Pressed(kb.aKey)) LaneChangeRequested?.Invoke(-1);
            if (Pressed(kb.rightArrowKey) || Pressed(kb.dKey)) LaneChangeRequested?.Invoke(1);
            if (Pressed(kb.downArrowKey)  || Pressed(kb.sKey)) PushRequested?.Invoke();
            if (Pressed(kb.spaceKey) || Pressed(kb.wKey) || Pressed(kb.upArrowKey)) JumpRequested?.Invoke();
            if (Pressed(kb.fKey))     RailTrickRequested?.Invoke();
            if (Pressed(kb.enterKey)) TapRequested?.Invoke();
            if (Pressed(kb.rKey))     RestartRequested?.Invoke();
        }

        private void HandleTouch()
        {
            var ts = Touchscreen.current;
            if (ts == null) return;

            var touch = ts.primaryTouch;
            var phase = touch.phase.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchStart = touch.position.ReadValue();
                trackingTouch = true;
                return;
            }
            if (phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                trackingTouch = false;
                return;
            }
            if (phase != UnityEngine.InputSystem.TouchPhase.Ended || !trackingTouch) return;

            trackingTouch = false;
            Vector2 delta = touch.position.ReadValue() - touchStart;
            if (delta.magnitude < minSwipeDistance) return;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                LaneChangeRequested?.Invoke(delta.x > 0f ? 1 : -1);
            else if (delta.y < 0f)
                PushRequested?.Invoke();
        }

        private void HandleMousePointer()
        {
            if (!enableMouseSwipeInEditor) return;
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                pointerStart = mouse.position.ReadValue();
                trackingPointer = true;
                TapRequested?.Invoke();
                return;
            }
            if (!trackingPointer || !mouse.leftButton.wasReleasedThisFrame) return;

            trackingPointer = false;
            Vector2 delta = mouse.position.ReadValue() - pointerStart;
            if (delta.magnitude < minSwipeDistance) return;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                LaneChangeRequested?.Invoke(delta.x > 0f ? 1 : -1);
            else if (delta.y < 0f)
                PushRequested?.Invoke();
            else if (delta.y > 0f)
                JumpRequested?.Invoke();
        }

        private static bool Pressed(UnityEngine.InputSystem.Controls.KeyControl k) =>
            k != null && k.wasPressedThisFrame;
    }
}
