using System.Linq;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace Larv.Field
{
    public class Windmill : VDrawable
    {
        private readonly Model _model;

        public readonly Matrix World;

        private readonly Texture2D _texture;

        private readonly ModelBone _animatedBone;
        private readonly Matrix _originalBoneTransformation;

        private float _angle;

        public Windmill(LarvContent lContent, Vector3 location)
            : base(lContent.TextureEffect)
        {
            World = Matrix.Scaling(0.004f)*Matrix.RotationY(1)*Matrix.Translation(location);
            _model = lContent.Load<Model>("models/windmillf");
            _texture = lContent.Load<Texture2D>("models/windmill_diffuse");

            _animatedBone = _model.Meshes.Single(mesh => mesh.Name == "Fan").ParentBone;
            _originalBoneTransformation = Matrix.Translation(0, 850, 0);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            _angle += (float) gameTime.ElapsedGameTime.TotalSeconds;
            if (_angle > MathUtil.TwoPi)
                _angle -= MathUtil.TwoPi;
            _animatedBone.Transform = Matrix.RotationZ(_angle)*_originalBoneTransformation;
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);

            if (drawingReason != DrawingReason.ShadowDepthMap)
                Effect.Texture = _texture;
            _model.Draw(Effect.GraphicsDevice, World, camera.View, camera.Projection, Effect.Effect);

            return true;
        }

    }

}
