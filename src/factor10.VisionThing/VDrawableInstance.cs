using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using SharpDX;

namespace factor10.VisionThing
{
    public class VDrawableInstance : VDrawable
    {
        public readonly IVDrawable Thing;
        public Matrix World;

        public VDrawableInstance(IVEffect effect, IVDrawable thing, Matrix world )
            : base(effect)
        {
            Thing = thing;
            World = world;
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);
            Effect.World = World;
            Thing.Draw(Effect);
            return true;
        }
    }

}
