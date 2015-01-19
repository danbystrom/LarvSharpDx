using System.Linq;
using factor10.VisionThing.CameraStuff;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Objects
{
    public class Bridge : VDrawable
    {
        public Matrix World;

        private readonly Model _model;
        private readonly Texture2D _texture;

        public Bridge(VisionContent vContent, Matrix world)
            : base(vContent.LoadEffect("effects/SimpleTextureEffect"))
        {
            _model = vContent.Load<Model>(@"Models/bridge");
            _texture = vContent.Load<Texture2D>("textures/bigstone");
            World = world * Matrix.Scaling(0.05f);

            foreach (var part in _model.Meshes.SelectMany(mesh => mesh.MeshParts))
                part.Effect = Effect.Effect;

        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);
            Effect.World = World;

            //if (drawingReason != DrawingReason.ShadowDepthMap)
            //    Effect.Texture = _texture;

            //Effect.Apply();
            //foreach (var mesh in _model.Meshes)
            //    mesh.Draw(Effect.GraphicsDevice);

            //Effect.World = World*Matrix.Translation(21.68f, 0, 0);
            //foreach (var mesh in _model.Meshes)
            //    mesh.Draw(Effect.GraphicsDevice);

            return true;
        }

    }

}