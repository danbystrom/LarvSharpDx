using System;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using Larv.Field;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace Larv.Serpent
{
    public class TerrainWalker : VDrawable //, IPosition
    {
        private readonly IVDrawable _sphere;
        private readonly Texture2D _texture;

        private readonly Ground _ground;

        private Vector2 _position;

//        public Vector3 Position { get { return _position; } }

        public TerrainWalker(
            LarvContent lcontent,
            IVEffect effect,
            Ground ground)
            : base(effect)
        {
            _sphere = lcontent.Sphere;

            _texture = lcontent.Load<Texture2D>(@"textures/eggshell");
            _ground = ground;
            _position = new Vector2(20, 8);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);
            _position.Y += 0.01f*(float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);

            Effect.Texture = _texture;

            var winv = _ground.World;
            winv.Invert();
            Vector3 gspace;
            var position = new Vector3(_position.X, 0, _position.Y);
            Vector3.TransformCoordinate(ref position, ref winv, out gspace);

            for (var z = 6; z < 7; z++)
                for (var x = 6; x < 7; x++)
                {
                    var g = new Vector3(gspace.X + x/3f, 0, gspace.Z + z/3f);
                    g.Y = _ground.GroundMap.GetExactHeight(g.X, g.Z);
                    Vector3 p;
                    Vector3.TransformCoordinate(ref g, ref _ground.World, out p);
                    Effect.World = Matrix.Scaling(0.05f) * Matrix.Translation(p);
                    var q = (g.X - (int) g.X + g.Z - (int) g.Z)/2;
                    Effect.DiffuseColor = new Vector4(q, q, q, 1);
                    _sphere.Draw(Effect);
                }

            return true;
        }

    }

}
