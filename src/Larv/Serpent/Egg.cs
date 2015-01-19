using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using Larv.Util;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace Larv.Serpent
{
    public class Egg : VDrawable, IPosition
    {
        private static readonly Matrix NorthSouthScale = Matrix.Scaling(0.6f, 0.6f, 0.8f);
        private static readonly Matrix EastWestScale = Matrix.Scaling(0.8f, 0.6f, 0.6f);

        private readonly IVDrawable _sphere;
        private readonly Texture2D _eggSkin;
        private readonly Vector4 _diffuseColor;
        private readonly Matrix _world;

        public readonly Whereabouts Whereabouts;

        private float _timeToHatch;

        public Egg(
            IVEffect effect,
            IVDrawable sphere,
            Texture2D eggSkin,
            Vector4 diffuseColor,
            Matrix world,
            Whereabouts whereabouts,
            float timeToHatch)
            : base(effect)
        {
            _sphere = sphere;
            _eggSkin = eggSkin;
            _diffuseColor = diffuseColor;
            _world = world * Matrix.Translation(0, -0.2f, 0);
            Whereabouts = whereabouts;
            _timeToHatch = timeToHatch;
        }

        public Vector3 Position
        {
            get { return _world.TranslationVector; }    
        }

        public virtual void Update(GameTime gameTime)
        {
            _timeToHatch -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public static void Draw(
            IVEffect effect,
            Texture2D skin,
            Vector4 diffuseColor,
            IVDrawable sphere,
            Matrix translation,
            Direction direction)
        {
            var t = direction.IsNorthSouth ? NorthSouthScale : EastWestScale;
            var off = direction.DirectionAsVector3()*-0.3f;
            t *= Matrix.Translation(off);

            effect.World = t*translation;
            effect.Texture = skin;
            effect.DiffuseColor = diffuseColor;
            sphere.Draw(effect);
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);
            Draw(Effect, _eggSkin, _diffuseColor, _sphere, _world, Whereabouts.Direction);
            return true;
        }

        public bool TimeToHatch()
        {
            return _timeToHatch < 0;
        }

    }

}
