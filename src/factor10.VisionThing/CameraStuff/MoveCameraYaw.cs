using System;
using SharpDX;

namespace factor10.VisionThing.CameraStuff
{
    public class MoveCameraYaw : MoveCameraBase
    {
        private readonly Vector3 _fromPosition;
        private readonly Vector3 _toPosition;
        private readonly float _fromYaw;
        private readonly float _toYaw;
        private readonly float _fromPitch;
        private readonly float _toPitch;

        public MoveCameraYaw(Camera camera, MovementTime time, Vector3 toPosition, Vector3 toLookAt)
            : base(camera)
        {
            _fromPosition = camera.Position;
            _fromYaw = camera.Yaw;
            _fromPitch = camera.Pitch;

            _toPosition = toPosition;
            _toYaw = (float) Math.Atan2(toPosition.X - toLookAt.X, toPosition.Z - toLookAt.Z);
            _toPitch = -(float)Math.Asin((_toPosition.Y - toLookAt.Y) / Vector3.Distance(_toPosition, toLookAt));

            var angle = _fromYaw - _toYaw;
            if (angle > MathUtil.Pi)
                _fromYaw -= MathUtil.TwoPi;
            else if (angle < -MathUtil.Pi)
                _fromYaw += MathUtil.TwoPi;

            EndTime = time.GetTotalTime(Vector3.Distance(_fromPosition, _toPosition));
        }

        protected Vector3 GetLookAtDirection(float factor)
        {
            var rotation = Matrix.RotationYawPitchRoll(
                MathUtil.Lerp(_fromYaw, _toYaw, factor),
                MathUtil.Lerp(_fromPitch, _toPitch, factor),
                0);
            return Vector3.TransformCoordinate(Vector3.ForwardRH, rotation);
        }

        protected override bool MoveAround()
        {
            var factor = MathUtil.SmoothStep(Math.Min(1, ElapsedTime/EndTime));

            var pos = Vector3.Lerp(_fromPosition, _toPosition, factor);

            Camera.Update(
                pos,
                pos + GetLookAtDirection(factor));

            return ElapsedTime < EndTime;
        }

    }

}
