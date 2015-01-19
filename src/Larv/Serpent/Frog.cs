using System.Data;
using System.Linq;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using factor10.VisionThing.Util;
using Larv.Field;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;

namespace Larv.Serpent
{
    public class Frog : VDrawable, IPosition
    {
        private readonly Model _model;
        private readonly Matrix _modelRotation;
        private readonly Texture2D _texture;

        private readonly Serpents _serpents;

        private readonly Ground _ground;

        private Vector3 _position;
        public Vector3 Position { get { return _position; } }

        private Matrix _rotation;
        private float _currentAngle;

        private static readonly Random Rnd = new Random();

        private readonly SequentialToDo _actions = new SequentialToDo();

        public Frog(
            LarvContent lcontent,
            IVEffect effect,
            Serpents serpents)
            : base(effect)
        {
            _model = lcontent.Load<Model>(@"Models/frog");
            _modelRotation = Matrix.RotationY(MathUtil.Pi)*Matrix.Scaling(0.1f);

            _texture = lcontent.Load<Texture2D>(@"models/frogskin");
            _serpents = serpents;
            _ground = lcontent.Ground;
            Restart();
        }

        public void Restart()
        {
            _actions.Clear();
            switch (Rnd.Next(4))
            {
                case 0:
                    _position = new Vector3(_serpents.PlayingField.MiddleX, 0, -2);
                    break;
                case 1:
                    _position = new Vector3(_serpents.PlayingField.MiddleX, 0, _serpents.PlayingField.Height + 1);
                    break;
                case 2:
                    _position = new Vector3(-2, 0, _serpents.PlayingField.MiddleY);
                    break;
                default:
                    _position = new Vector3(_serpents.PlayingField.Width + 1, 0, _serpents.PlayingField.MiddleY);
                    break;
            }
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            if (_actions.Do(gameTime))
                return;

            // has run out of actions - create new commands

            Vector3 toPosition, normal;
            float angle;
            if (!findNewPosition(out toPosition, out normal, out angle))
                return;

            var fromPosition = _position;

            var currentNormal = _rotation.Up;
            var shortDelay = Rnd.NextFloat(0.3f, 1.1f);
            _actions.AddOneShot(shortDelay, () => _rotation = currentNormal.AlignObjectToNormal(MathUtil.Lerp(_currentAngle, angle, 0.33f)));
            _actions.AddOneShot(shortDelay, () => _rotation = currentNormal.AlignObjectToNormal(MathUtil.Lerp(_currentAngle, angle, 0.66f)));
            _actions.AddOneShot(shortDelay, () => _rotation = currentNormal.AlignObjectToNormal(_currentAngle = angle));
            _actions.AddWait(shortDelay);
            _actions.AddWhile(time =>
            {
                var factor = Math.Min(time/0.2f, 0.5f);
                _position = Vector3.Lerp(fromPosition, toPosition, factor);
                _position.Y += factor;
                if (factor < 0.5f)
                    return true;
                _rotation = normal.AlignObjectToNormal(angle);
                return false;
            });
            _actions.AddWhile(time =>
            {
                var factor = Math.Min(0.5f + time/0.2f, 1);
                _position = Vector3.Lerp(fromPosition, toPosition, factor);
                _position.Y -= (1 - factor);
                return factor < 1;
            });
            _actions.AddWait(Rnd.NextFloat(3, 6));
        }

        private bool findNewPosition(out Vector3 position, out Vector3 normal, out float angle)
        {
            // this is not even called once a second - performance is not an issue here

            try
            {
                var eggs = _serpents.EnemyEggs.Select(_ => _.Position).ToList();
                if (_serpents.PlayerEgg != null)
                    eggs.Add(_serpents.PlayerEgg.Position);
                eggs.Sort((x, y) => Vector3.DistanceSquared(_position, x).CompareTo(Vector3.DistanceSquared(_position, y)));
                var closeToEgg = eggs.Any() && Vector3.DistanceSquared(_position, eggs.First()) < 4;

                eggs.Add(new Vector3(_serpents.PlayingField.MiddleX, 0, _serpents.PlayingField.MiddleY));
                var goodDirection = Vector3.Normalize(eggs.FirstOrDefault() - _position);

                var winv = _ground.World;
                winv.Invert();
                Vector3 gspaceCurrent;
                Vector3.TransformCoordinate(ref _position, ref winv, out gspaceCurrent);

                for (var i = 0; i < 100; i++)
                {
                    var dx = Rnd.NextFloat(1, 2)*(Rnd.NextDouble() < 0.5 ? -1 : 1);
                    var dz = Rnd.NextFloat(1, 2)*(Rnd.NextDouble() < 0.5 ? -1 : 1);

                    var toPosition = closeToEgg
                        ? eggs.First()
                        : Position + new Vector3(dx, 0, dz) + goodDirection*Rnd.NextFloat(0.9f, 1.5f);

                    Vector3 gspaceTo;
                    Vector3.TransformCoordinate(ref toPosition, ref winv, out gspaceTo);
                    normal = _ground.GroundMap.GetNormal((int) gspaceTo.X, (int) gspaceTo.Z, ref _ground.World);
                    if (normal.Y < 0.5f)
                        continue;

                    var pos0 = _ground.GroundMap.GetExactHeight(gspaceTo.X - 0.1f, gspaceTo.Z - 0.1f);
                    var pos1 = _ground.GroundMap.GetExactHeight(gspaceTo.X - 0.1f, gspaceTo.Z + 0.1f);
                    var pos2 = _ground.GroundMap.GetExactHeight(gspaceTo.X + 0.1f, gspaceTo.Z - 0.1f);
                    var pos3 = _ground.GroundMap.GetExactHeight(gspaceTo.X + 0.1f, gspaceTo.Z + 0.1f);
                    gspaceTo.Y = Math.Min(Math.Min(pos0, pos1), Math.Min(pos2, pos3));
                        // _ground.GroundMap.GetExactHeight(gspaceTo.X, gspaceTo.Z);

                    // now we have a new position for the frog
                    Vector3.TransformCoordinate(ref gspaceTo, ref _ground.World, out position);
                    position.Y = Math.Max(0, position.Y);

                    float minpe, maxpe;
                    _serpents.PlayingField.GetSurroundingElevation((int)(toPosition.X + 0.5f), (int)(toPosition.Z + 0.5f), out minpe, out maxpe);
                    if (maxpe > 0 && maxpe < position.Y + 0.75f)
                        continue;

                    angle = (float)Math.Atan2(position.X - _position.X, position.Z - _position.Z);
                    return true;
                }
            }
            catch
            {
            }

            angle = 0;
            position = normal = Vector3.Zero;
            return false;
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);
            var t = _modelRotation*_rotation*Matrix.Translation(_position);

            Effect.World = t;
            Effect.Texture = _texture;
            Effect.DiffuseColor = Vector4.One;
            _model.Draw(Effect.GraphicsDevice, t, camera.View, camera.Projection, Effect.Effect);

            return true;
        }

    }

}
