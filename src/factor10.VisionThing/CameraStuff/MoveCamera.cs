using System;
using System.Linq;
using SharpDX;

namespace factor10.VisionThing.CameraStuff
{
    public class MoveCamera : MoveCameraBase
    {
        private readonly Func<Vector3> _toLookAt;

        private readonly Vector3[] _path;

        public MoveCamera(Camera camera, MovementTime time, Vector3 toPosition, Func<Vector3> toLookAt)
            : base(camera)
        {
            _path = new[] {Camera.Position, toPosition};

            EndTime = !time.UnitsPerSecond
                ? time.Time
                : (0.1f + pathLength())/time.Time;

            _toLookAt = toLookAt;
        }

        private float pathLength()
        {
            var length = 0f;
            var v = _path.First();
            for (var i = 1; i < _path.Length; i++)
                length += Vector3.Distance(v, v = _path[i]);
            return length;
        }

        private Vector3 getPointOnPath(float x)
        {
            if (x < 0)
                return _path.First();
            if (x >= 1)
                return _path.Last();
            var step = 1f/(_path.Length - 1);
            var idx = (int) (x/step);
            var frac = (x - idx*step)/step;
            return Vector3.Lerp(_path[idx], _path[idx + 1], frac);
        }

        protected override bool MoveAround()
        {
            var timeFactor = Math.Min(1, ElapsedTime/EndTime);

            var posFactor = MathUtil.SmootherStep(timeFactor);
            var lookFactor = Math.Min(1, timeFactor*1.2f);  // zoom in on "look at" faster

            Camera.Update(
                getPointOnPath(posFactor),
                Vector3.Lerp(FromLookAt, _toLookAt(), lookFactor));

            return ElapsedTime < EndTime;
        }

    }

}
