using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using factor10.VisionThing.Primitives;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing
{
    public class SkySphere : VDrawable
    {
        private readonly IGeometricPrimitive _sphere;

        public SkySphere(
            VisionContent vtContent,
            Texture2DBase texture)
            : base(new VisionEffect(vtContent.Load<Effect>("effects/skysphere")))
        {
            _sphere = new SpherePrimitive<VertexPosition>(vtContent.GraphicsDevice, (p, n, t, tx) => new VertexPosition(p), 20000, 10, false);
            Effect.Texture = texture;
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            if (drawingReason == DrawingReason.ShadowDepthMap)
                return false;
            camera.UpdateEffect(Effect);
            Effect.World = Matrix.Scaling(1, 0.5f, 1)*Matrix.Translation(camera.Position);
            _sphere.Draw(Effect);
            return true;
        }

    }

}
