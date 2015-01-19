using System;
using System.Collections.Generic;
using System.Linq;
using factor10.VisionThing.Effects;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Input;

namespace factor10.VisionThing.CameraStuff
{

    public class Camera
    {
        private Matrix _view;
        private Matrix _projection;

        private bool _dirtyViewProj;
        private Matrix _viewProjection;

        public Vector3 Position { get; protected set; }
        public Vector3 Target { get; protected set; }
        public Vector3 Forward { get; protected set; }
        public Vector3 Left { get; protected set; }
        public Vector3 Up { get; set; }

        public float Yaw { get; protected set; }
        public float Pitch { get; protected set; }

        public readonly Vector2 ClientSize;

        private Vector2 _lastMousePosition;

        private readonly List<Keys> _downKeys = new List<Keys>();

        public readonly KeyboardManager KeyboardManager;
        public readonly MouseManager MouseManager;
        public readonly PointerManager PointerManager;

        public KeyboardState KeyboardState;
        public MouseState MouseState;
        public PointerState PointerState;

        public readonly float ZNear;
        public readonly float ZFar;

        public float MovingSpeed = 30;

        public Camera(
            Vector2 clientSize,
            Vector3 position,
            Vector3 target,
            float zNear = 1,
            float zFar = 20000)
        {
            ClientSize = clientSize;

            Up = Vector3.Up;
            Update(position, target);
            ZNear = zNear;
            ZFar = zFar;
            Projection = Matrix.PerspectiveFovRH(
                MathUtil.PiOverFour,
                clientSize.X / clientSize.Y,
                ZNear, ZFar);
        }

        public Camera(
            Vector2 clientSize,
            KeyboardManager keyboardManager,
            MouseManager mouseManager,
            PointerManager pointerManager,
            Vector3 position,
            Vector3 target,
            float nearPlane = 1,
            float farPlane = 20000)
            : this(clientSize, position, target, nearPlane, farPlane)
        {
            KeyboardManager = keyboardManager;
            MouseManager = mouseManager;
            PointerManager = pointerManager;
        }

        public Matrix View
        {
            get { return _view; }
            set
            {
                _view = value;
                _dirtyViewProj = true;
                _boundingFrustum = null;
            }
        }

        public Matrix Projection
        {
            get { return _projection; }
            set
            {
                _projection = value;
                _dirtyViewProj = true;
                _boundingFrustum = null;
            }
        }

        public Matrix ViewProjection
        {
            get
            {
                if (_dirtyViewProj)
                {
                    _viewProjection = View*Projection;
                    _boundingFrustum = null;
                    _dirtyViewProj = false;
                }
                return _viewProjection;
            }
        }

        public Vector3 Front
        {
            get
            {
                var front = Target - Position;
                front.Normalize();
                return front;
            }
        }

        public void Update(
            Vector3 position,
            Vector3 target)
        {
            Position = position;
            Target = target;

            View = Matrix.LookAtRH(
                Position,
                Target,
                Vector3.Up);
            Yaw = (float)Math.Atan2(position.X - target.X, position.Z - target.Z);
            Pitch = -(float)Math.Asin((position.Y - target.Y) / Vector3.Distance(position, target));
            Forward = Vector3.Normalize(target - position);
            Left = Vector3.Normalize(Vector3.Cross(Up, Forward));
        }

        private BoundingFrustum? _boundingFrustum;
        private int _lastWheelDelta;
        private float _moveMoveback;

        public BoundingFrustum BoundingFrustum
        {
            get { return (_boundingFrustum ?? (_boundingFrustum = new BoundingFrustum(ViewProjection))).Value; }
        }

        public void UpdateInputDevices()
        {
            KeyboardState = KeyboardManager.GetState();
            KeyboardState.GetDownKeys(_downKeys);
            MouseState = MouseManager.GetState();
            if(PointerManager!=null)
                PointerState = PointerManager.GetState();
        }

        public void UpdateFreeFlyingCamera(GameTime gameTime)
        {
            var step = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var mouseWheelChanged = MouseState.WheelDelta != _lastWheelDelta;

            if (_moveMoveback > 0)
            {
                _moveMoveback -= step;
                if(_moveMoveback<=0)
                    MouseManager.SetPosition(_lastMousePosition);
            }

            if (!MouseState.LeftButton.Down && !MouseState.RightButton.Down && !mouseWheelChanged && !_downKeys.Any())
                return;

            var mousePos = new Vector2(MouseState.X, MouseState.Y);
            if (MouseState.LeftButton.Pressed || MouseState.RightButton.Pressed)
                _lastMousePosition = mousePos;

            var delta = (_lastMousePosition - mousePos)*ClientSize*step*5;

            if (mouseWheelChanged)
            {
                delta.Y += (MouseState.WheelDelta - _lastWheelDelta)*1.0f;
                _lastWheelDelta = MouseState.WheelDelta;
            }

            var pos = Position;

            //if (PointerState.Points.Any())
            //    foreach(var point in PointerState.Points)
            //        switch (point.EventType)
            //        {
            //            case PointerEventType.Pressed:
            //                _lastPointerPosition = point.Position;
            //                break;
            //            case PointerEventType.Moved:
            //                delta += _lastMousePosition - point.Position;
            //                Yaw += MathUtil.DegreesToRadians(delta.X*0.50f);
            //                Pitch += MathUtil.DegreesToRadians(delta.Y*0.50f);
            //                _lastPointerPosition = point.Position;
            //                break;
            //        }
            //else
            if (MouseState.LeftButton.Down)
            {
                Yaw += MathUtil.DegreesToRadians(delta.X*0.50f);
                Pitch += MathUtil.DegreesToRadians(delta.Y*0.50f);
                if(_moveMoveback<=0)
                    _moveMoveback = 0.05f;
            }
            else if (MouseState.RightButton.Down || mouseWheelChanged)
            {
                pos -= Forward*delta.Y;
                pos += Left*delta.X;
                if (_moveMoveback <= 0)
                    _moveMoveback = 0.05f;
            }

            var rotStep = step*1.5f;
            step *= MovingSpeed;
            if (_downKeys.Contains(Keys.Shift))
                step *= 5;

            if (_downKeys.Contains(Keys.R))
                pos.Y += step;
            if (KeyboardState.IsKeyDown(Keys.F))
                pos.Y -= step;
            if (KeyboardState.IsKeyDown(Keys.A))
                pos += Left*step;
            if (KeyboardState.IsKeyDown(Keys.D))
                pos -= Left*step;
            if (KeyboardState.IsKeyDown(Keys.W))
                pos += Forward*step;
            if (KeyboardState.IsKeyDown(Keys.S))
                pos -= Forward*step;
            if (KeyboardState.IsKeyDown(Keys.Left))
                Yaw += rotStep;
            if (KeyboardState.IsKeyDown(Keys.Right))
                Yaw -= rotStep;
            if (KeyboardState.IsKeyDown(Keys.Up))
                Pitch += rotStep;
            if (KeyboardState.IsKeyDown(Keys.Down))
                Pitch -= rotStep;

            var rotation = Matrix.RotationYawPitchRoll(Yaw, Pitch, 0);
            Update(
                pos,
                pos + Vector3.TransformCoordinate(Vector3.ForwardRH*10, rotation));
        }

        public void UpdateEffect(IVEffect effect)
        {
            effect.View = View;
            effect.Projection = Projection;
            effect.CameraPosition = Position;
        }

        public Ray GetPickingRay()
        {
            var viewProj = View * Projection;

            var mouseNearVector = new Vector3(MouseState.X, MouseState.Y, ZNear);
            Vector3 pointNear;
            Vector3.Unproject(ref mouseNearVector, 0, 0, 1, 1, ZNear, ZFar, ref viewProj, out pointNear);

            var mouseFarVector = new Vector3(MouseState.X, MouseState.Y, ZFar);
            Vector3 pointFar;
            Vector3.Unproject(ref mouseFarVector, 0, 0, 1, 1, ZNear, ZFar, ref viewProj, out pointFar);

            return new Ray(pointNear, Vector3.Normalize(pointFar - pointNear));
        }

    }

}
