﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PressPlay.FFWD
{
    /// <summary>
    /// Math library with float versions of the methods in the normal Math library.
    /// Better suited for games.
    /// </summary>
    public static class Mathf
    {
        public const float PI = MathHelper.Pi;
        public const float Infinity = float.PositiveInfinity;
        public const float NegativeInfinity = float.NegativeInfinity;
        public const float Rad2Deg = 360 / MathHelper.TwoPi;
        public const float Deg2Rad = MathHelper.TwoPi / 360;

        public static int Sign(float value)
        {
            return Math.Sign(value);
        }

        public static float Cos(float value)
        {
            

            return (float)Math.Cos(value);
        }

        public static float Sin(float value)
        {
            return (float)Math.Sin(value);
        }

        public static float Pow(float x, float y)
        {
            return (float)Math.Pow(x, y);
        }

        public static float Sqrt(float x)
        {
            return (float)Math.Sqrt(x);
        }

        public static float Abs(float x)
        {
            return (float)Math.Abs(x);
        }

        public static float Asin(float x)
        {
            return (float)Math.Asin(x);
        }

        public static float Min(float x, float y)
        {
            return Math.Min(x, y);
        }

        public static int Min(int x, int y)
        {
            return Math.Min(x, y);
        }

        public static float Max(float x, float y)
        {
            return Math.Max(x, y);
        }

        public static int Max(int x, int y)
        {
            return Math.Max(x, y);
        }

        public static float Atan2(float x, float y)
        {
            return (float)Math.Atan2(x, y);
        }

        public static float Clamp(float value, float min, float max){
            return MathHelper.Clamp(value, min, max);
        }

        public static int Clamp(int value, int min, int max)
        {
            return (int)MathHelper.Clamp(value, min, max);
        }

        public static float Clamp01(float value)
        {
            if (value > 1)
            {
                return 1f;
            }
            else if (value < 0)
            {
                return 0f;
            }

            return value;            
        }

        public static int CeilToInt(float f)
        {
            return (int)Math.Ceiling((double)f);
        }

        public static int FloorToInt(float f)
        {
            return (int)Math.Floor((double)f);
        }

        public static float Floor(float f)
        {
            return (float)Math.Floor((double)f);
        }

        //TODO Implement Lerp function
        public static float Lerp(float from, float to, float t)
        {
            return MathHelper.Lerp(from, to, t);
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime)
        {
            return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, Mathf.Infinity, Time.deltaTime);
        }

        //TODO Implement SmoothDamp function
        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime){
            throw new NotImplementedException("SmoothDamp is not implemented!");
        }

        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime)
        {
            return Mathf.SmoothDampAngle(current, target, ref currentVelocity, smoothTime, Mathf.Infinity, Time.deltaTime);
        }

        //TODO Implement SmoothDampAngle function
        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            throw new NotImplementedException("SmoothDamp is not implemented!");
        }
    }
}
