using System;
using factor10.VisionThing.CameraStuff;
using SharpDX;

namespace Larv.Serpent
{
    public class SerpentCamera : MoveCameraBase
    {
        public const float CameraDistanceToHeadXz = 9;
        public const float CameraDistanceToHeadY = 5;
        public const float DefaultTension = 7;

        private float _lastTime;

        private readonly BaseSerpent _serpent;

        public float MaxTension;
        public float Tension;
        public float TensionIncreaseFactor;

        public SerpentCamera(Camera camera, BaseSerpent serpent, float tension = DefaultTension, float tensionIncreaseFactor = 1, float maxTension = DefaultTension)
            : base(camera)
        {
            _serpent = serpent;
            Tension = tension;
            TensionIncreaseFactor = tensionIncreaseFactor;
            MaxTension = maxTension;
        }


        protected override bool MoveAround()
        {
            var dt = ElapsedTime - _lastTime;
            _lastTime = ElapsedTime;

            if (Tension < MaxTension)
                Tension = Math.Min(MaxTension, Tension + dt*TensionIncreaseFactor);

            var target = _serpent.LookAtPosition;
            var direction = _serpent.HeadDirection;

            var target2D = new Vector2(target.X, target.Z);
            var position2D = moveTo(
                new Vector2(Camera.Position.X, Camera.Position.Z),
                target2D,
                target2D - direction.DirectionAsVector2()*CameraDistanceToHeadXz,
                dt);

            var newPosition = new Vector3(
                position2D.X,
                target.Y + CameraDistanceToHeadY,
                position2D.Y);

            var v = dt * Tension;

            var nextPosition = Vector3.Lerp(Camera.Position, newPosition, v);
            var nextTarget = Vector3.Lerp(Camera.Target, target, v);

           //_log.Insert(0,string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", dt, v, Camera.Position, newPosition, nextPosition, Camera.Target, target, nextTarget));
           // while (_log.Count > 500)
           //     _log.RemoveAt(_log.Count - 1);

            Camera.Update(nextPosition, nextTarget);

            //if (Camera.KeyboardState.IsKeyPressed(Keys.M))
            //{
            //    var s = string.Join("\r\n", _log);
            //    s = s.Replace("X:", "");
            //    s = s.Replace("Y:", "");
            //    s = s.Replace("Z:", "");
            //    System.Diagnostics.Debug.Print(s);    
            //}

            return true;
        }

        //private static List<string> _log = new List<string>();

        private static Vector2 moveTo(
            Vector2 camera,
            Vector2 target,
            Vector2 desired,
            double elapsedTime)
        {
            var d2TargetDesired = Vector2.DistanceSquared(target, desired);
            var d2CameraDesired = Vector2.DistanceSquared(camera, desired);
            var d2TargetCamera = Vector2.DistanceSquared(target, camera);

            if (d2TargetDesired < 0.0001f || d2CameraDesired < 0.0001f || d2TargetCamera < 0.0001f)
                return desired;

            var d1 = d2TargetDesired + d2TargetCamera - d2CameraDesired;
            var d2 = Math.Sqrt(4*d2TargetDesired*d2TargetCamera);
            var div = d1/d2;

            float angle;
            if (div < -1f)
                angle = MathUtil.Pi - (float)Math.Acos(div + 2);
            else if (div > 1)
                angle = (float) Math.Acos(div - 2) - MathUtil.Pi;
            else
                angle = (float) Math.Acos(div);

            var v1 = camera - target;
            var v2 = desired - target;

            if (v1.X*v2.Y - v2.X*v1.Y > 0)
                angle = -angle;

            var angleFraction = angle*elapsedTime*10;
            //_log.Insert(0,string.Format("{0}", angleFraction));

            var cosA = (float) Math.Cos(angleFraction);
            var sinA = (float) Math.Sin(angleFraction);
            var direction = new Vector2(v1.X*cosA + v1.Y*sinA, -v1.X*sinA + v1.Y*cosA);
            direction.Normalize();
            return target + direction*v2.Length();
        }

    }

}
