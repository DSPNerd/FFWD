﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using PressPlay.FFWD;
#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace PressPlay.FFWD
{
    public static class Input
    {
        private static MouseState lastMouseState;
        private static MouseState currentMouseState;

#if WINDOWS_PHONE
        private static Vector2 lastTap;
#endif

        internal static void Initialize()
        {
#if WINDOWS_PHONE
            TouchPanel.EnabledGestures = TouchPanel.EnabledGestures | 
                                         GestureType.Tap;
#endif
        }

        public static void Update()
        {
            lastMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

#if WINDOWS_PHONE
            samples.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                samples.Add(TouchPanel.ReadGesture());
            }
            if (HasSample(GestureType.Tap))
            {
                lastTap = GetSample(GestureType.Tap).First().Position;
            }
#endif
        }

#if WINDOWS_PHONE
        private static List<GestureSample> samples = new List<GestureSample>();
        public static IEnumerable<GestureSample> GetSample(GestureType type)
        {
            for (int i = 0; i < samples.Count; i++)
            {
                if ((samples[i].GestureType & type) != 0)
                {
                    yield return samples[i];
                }
            }
            yield break;
        }

        public static bool HasSample(GestureType type)
        {
            for (int i = 0; i < samples.Count; i++)
            {
                if ((samples[i].GestureType & type) != 0)
                {
                    return true;
                }
            }
            return false;
        }
#endif

        public static Vector2 mousePosition
        {
            get
            {
#if WINDOWS_PHONE
                return lastTap;
#else
                return new Vector2(currentMouseState.X, currentMouseState.Y);
#endif
            }
        }

        public static float GetAxis(string axisName)
        {
            return 0.0f;
        }

        public static bool GetButton(string buttonName)
        {
            return false;
        }

        public static bool GetButtonUp(string buttonName)
        {
            return false;
        }

        public static bool GetButtonDown(string buttonName)
        {
            return false;
        }

        public static bool GetMouseButton(int button)
        {
#if WINDOWS_PHONE
            return HasSample(GestureType.Tap);
#else
            switch (button)
            {
                case 0:
                    return currentMouseState.LeftButton == ButtonState.Pressed;
                case 1:
                    return currentMouseState.MiddleButton == ButtonState.Pressed;
                case 2:
                    return currentMouseState.RightButton == ButtonState.Pressed;
            }
            return false;
#endif
        }

        public static bool GetMouseButtonDown(int button)
        {
#if WINDOWS_PHONE
            return HasSample(GestureType.Tap);
            //switch (button)
            //{
            //    case 0:
            //        return (currentMouseState.LeftButton == ButtonState.Pressed);
            //    case 1:
            //        return (currentMouseState.MiddleButton == ButtonState.Pressed);
            //    case 2:
            //        return (currentMouseState.RightButton == ButtonState.Pressed);
            //}
#else
            switch (button)
            {
                case 0:
                    return (currentMouseState.LeftButton == ButtonState.Pressed) && (lastMouseState.LeftButton == ButtonState.Released);
                case 1:
                    return (currentMouseState.MiddleButton == ButtonState.Pressed) && (lastMouseState.MiddleButton == ButtonState.Released);
                case 2:
                    return (currentMouseState.RightButton == ButtonState.Pressed) && (lastMouseState.RightButton == ButtonState.Released);
            }
#endif
            return false;
        }

        public static bool GetMouseButtonUp(int button)
        {
#if WINDOWS_PHONE
            return !HasSample(GestureType.Tap);
#else
            switch (button)
            {
                case 0:
                    return (currentMouseState.LeftButton == ButtonState.Released) && (lastMouseState.LeftButton == ButtonState.Pressed);
                case 1:
                    return (currentMouseState.MiddleButton == ButtonState.Released) && (lastMouseState.MiddleButton == ButtonState.Pressed);
                case 2:
                    return (currentMouseState.RightButton == ButtonState.Released) && (lastMouseState.RightButton == ButtonState.Pressed);
            }
            return false;
#endif
        }
    }
}
