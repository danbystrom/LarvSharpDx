using System;
using factor10.VisionThing.CameraStuff;
using Larv.Field;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace Larv.Serpent
{
    public class EnemySerpent : BaseSerpent
    {
        public static readonly Vector4 ColorWhenLonger = new Vector4(1.2f, 0.7f, 0.7f, 1);
        public static readonly Vector4 ColorWhenShorter = new Vector4(0.8f, 1.4f, 0.8f, 1);

        private static readonly Random Rnd = new Random();
        private float _delayBeforeStart;

        public EnemySerpent(
            LarvContent lcontent,
            PlayingField playingField,
            float delayBeforeStart,
            int length)
            : base(
                lcontent,
                playingField,
                lcontent.Load<Texture2D>(@"Textures\snakeskin"),
                lcontent.Load<Texture2D>(@"Textures\snakeskinhead"),
                lcontent.Load<Texture2D>(@"Textures\snakeskinmap"),
                lcontent.Load<Texture2D>(@"Textures\eggshell"))
        {
            _delayBeforeStart = delayBeforeStart;
            for (var i = 0; i < length; i++)
                AddTail();
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            if (_delayBeforeStart <= 0)
                base.Update(camera, gameTime);
            else
                _delayBeforeStart -= (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        protected override void takeDirection(bool delayed)
        {
            if (delayed)
                return;

            if (SerpentStatus != SerpentStatus.Alive)
            {
                TryMove(HeadDirection);
                return;
            }

            if (TakeDirection())
                return;

            if (Rnd.NextDouble() < 0.33 && TryMove(HeadDirection.Left))
                return;
            if (Rnd.NextDouble() < 0.66 && TryMove(HeadDirection.Right))
                return;
            if (TryMove(HeadDirection))
                return;

            if (Rnd.NextDouble() < 0.5 && TryMove(HeadDirection.Left))
                return;
            if (TryMove(HeadDirection.Right))
                return;
            if (TryMove(HeadDirection.Left))
                return;
            TryMove(HeadDirection.Opposite);
        }

        protected override Vector4 TintColor()
        {
            if (SerpentStatus != SerpentStatus.Alive)
                return new Vector4(1.1f, 1.1f, 0.4f, AlphaValue());
            return IsLonger ? ColorWhenLonger : ColorWhenShorter;
        }

    }

}
