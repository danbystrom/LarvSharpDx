using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using Larv.Util;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit.Input;

namespace Larv.Field
{
    public class CaveModel : VDrawable
    {
        private readonly Model _caveModel;
        private readonly Model _gratingModel;

        public Matrix CaveWorld;
        public Matrix GratingWorld;

        public bool OpenDoor = false;
        private float _angle = MathUtil.Pi * 3/4;

        private readonly Texture2D _texture;
        private readonly Texture2D _gratingTexture;

        private readonly Matrix _gratingPart1;
        private Matrix _gratingPart3;

        public CaveModel(LarvContent lcontent)
            : base(lcontent.TextureEffect)
        {
            _caveModel = lcontent.Load<Model>("models/cave");
            _texture = lcontent.Load<Texture2D>("textures/cave");
            _gratingModel = lcontent.Load<Model>("models/grating");
            _gratingTexture = lcontent.Load<Texture2D>("textures/black");
            _gratingPart1 = Matrix.Translation(0.08f, 0, -0.02f)*Matrix.Scaling(0.5f, 0.7f, 0.4f);
        }

        public void SetPosition(Whereabouts whereabouts, PlayingField playingField)
        {
            SetPosition(whereabouts.GetPosition(playingField), whereabouts.Direction);
        }

        public void SetPosition(Vector3 position, Direction direction)
        {
            CaveWorld = Matrix.Scaling(0.9f, 0.7f, 0.5f)
                        *Matrix.Translation(5, 0.3f, -0.5f)
                        *Matrix.RotationY(MathUtil.PiOverTwo)
                        *Matrix.Translation(position);
            _gratingPart3 = Matrix.Translation(0.41f, 0.2f, -4.7f) * Matrix.Translation(position);
        }

        private void updateGratingWorld(float dangle)
        {
            _angle = MathUtil.Clamp(_angle + dangle, MathUtil.Pi, MathUtil.Pi*3/2);
            GratingWorld = _gratingPart1*Matrix.RotationY(_angle)*_gratingPart3;
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            if (OpenDoor && _angle > MathUtil.Pi)
                updateGratingWorld(-(float) gameTime.ElapsedGameTime.TotalSeconds);
            if (!OpenDoor && _angle < MathUtil.Pi*3/2)
                updateGratingWorld((float)gameTime.ElapsedGameTime.TotalSeconds);

            OpenDoor ^= camera.KeyboardState.IsKeyPressed(Keys.G);
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);
            Effect.DiffuseColor = new Vector4(0.6f, 0.6f, 0.6f, 1);
            Effect.Texture = _texture;
            _caveModel.Draw(Effect.GraphicsDevice, CaveWorld, camera.View, camera.Projection, Effect.Effect);
            Effect.Texture = _gratingTexture;
            _gratingModel.Draw(Effect.GraphicsDevice, GratingWorld, camera.View, camera.Projection, Effect.Effect);
            return true;
        }
        
    }

}
