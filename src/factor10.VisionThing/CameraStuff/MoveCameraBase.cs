using System;
using SharpDX;
using SharpDX.Toolkit;

namespace factor10.VisionThing.CameraStuff
{
    public struct MovementTime
    {
        public readonly bool UnitsPerSecond;
        public readonly float Time;
        public readonly float MinimumTime;
        public MovementTime(float time, bool unitsPerSecond = false, float minimumTime = 0.5f)
        {
            Time = time;
            UnitsPerSecond = unitsPerSecond;
            MinimumTime = minimumTime;
        }

        public float GetTotalTime(float units)
        {
            return Math.Max(!UnitsPerSecond ? Time : units/Time, MinimumTime);
        }
    }

    public static class MovementTimeExtensionMethods
    {
        public static MovementTime Time(this float time)
        {
            return new MovementTime(time);
        }
        public static MovementTime UnitsPerSecond(this float time, float minimumTime = 0.5f)
        {
            return new MovementTime(time, true, minimumTime);
        }
    }

    public abstract class MoveCameraBase : IDurable
    {
        protected readonly Camera Camera;

        protected readonly Vector3 FromLookAt;

        protected float EndTime;
        public float ElapsedTime { get; private set; }

        protected MoveCameraBase(Camera camera)
        {
            Camera = camera;
            FromLookAt = camera.Target;
        }

        public bool Move(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ElapsedTime += dt;
            return MoveAround();
        }

        public bool Do(float absolutTime)
        {
            ElapsedTime = absolutTime;
            return MoveAround();
        }

        protected abstract bool MoveAround();
    }

}
