using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using Larv.Field;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;

namespace Larv.Serpent
{
    public class TerrainWalker : VDrawable, IPosition
    {
        private readonly IVDrawable _sphere;
        private readonly Texture2D _texture;

        private readonly Ground _ground;

        private Vector3 _position1;

        private Vector3 _position2;
        private Vector3 _position3;
        private Vector3 _position4;
        private Vector3 _position5;
        public Vector3 Position { get { return _position1; } }

        public TerrainWalker(
            LarvContent lcontent,
            IVEffect effect,
            Ground ground)
            : base(effect)
        {
            _sphere = lcontent.Sphere;

            _texture = lcontent.Load<Texture2D>(@"models/frogskin");
            _ground = ground;
            _position1 = new Vector3(11, 0, -3);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            _position1.Z += 0.2f*(float) gameTime.ElapsedGameTime.TotalSeconds;

            var winv = _ground.World;
            winv.Invert();
            Vector3 gspace;
            Vector3.TransformCoordinate(ref _position1, ref winv, out gspace);

            gspace.Y = _ground.GroundMap.GetExactHeight(gspace.X, gspace.Z);

            Vector3.TransformCoordinate(ref gspace, ref _ground.World, out _position1);
            _position1.Y = Math.Max(0, _position1.Y) + 0.1f;

            gspace.X = (int) gspace.X;
            gspace.Z = (int)gspace.Z;
            gspace.Y = _ground.GroundMap[(int)gspace.X - 1, (int)gspace.Z - 1];
            Vector3.TransformCoordinate(ref gspace, ref _ground.World, out _position2);
            _position2.Y = Math.Max(0, _position2.Y) + 0.1f;

            gspace.Z += 1;
            gspace.Y = _ground.GroundMap[(int)gspace.X - 1, (int)gspace.Z - 1];
            Vector3.TransformCoordinate(ref gspace, ref _ground.World, out _position3);
            _position3.Y = Math.Max(0, _position3.Y) + 0.1f;

            gspace.X += 1;
            gspace.Y = _ground.GroundMap[(int)gspace.X - 1, (int)gspace.Z - 1];
            Vector3.TransformCoordinate(ref gspace, ref _ground.World, out _position4);
            _position4.Y = Math.Max(0, _position4.Y) + 0.1f;

            gspace.Z -= 1;
            gspace.Y = _ground.GroundMap[(int)gspace.X - 1, (int)gspace.Z - 1];
            Vector3.TransformCoordinate(ref gspace, ref _ground.World, out _position5);
            _position5.Y = Math.Max(0, _position5.Y) + 0.1f;

        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);

            Effect.Texture = _texture;
            Effect.DiffuseColor = Vector4.One;
            Effect.World = Matrix.Scaling(0.1f) * Matrix.Translation(_position1);
            _sphere.Draw(Effect);

            Effect.DiffuseColor = Vector4.Zero;
            Effect.World = Matrix.Scaling(0.1f) * Matrix.Translation(_position2);
            _sphere.Draw(Effect);
            Effect.World = Matrix.Scaling(0.1f) * Matrix.Translation(_position3);
            _sphere.Draw(Effect);
            Effect.World = Matrix.Scaling(0.1f) * Matrix.Translation(_position4);
            _sphere.Draw(Effect);
            Effect.World = Matrix.Scaling(0.1f) * Matrix.Translation(_position5);
            _sphere.Draw(Effect);

            return true;
        }

    }

}
