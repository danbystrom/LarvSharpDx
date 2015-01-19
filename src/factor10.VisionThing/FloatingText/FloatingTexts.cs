using System.Collections.Generic;
using factor10.VisionThing.CameraStuff;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.FloatingText
{
    public class FloatingTexts : VDrawable
    {
        public readonly VisionContent VContent;
        public readonly SpriteBatch SpriteBatch;
        public readonly SpriteFont Font;
        public readonly List<FloatingTextItem> Items = new List<FloatingTextItem>();

        public FloatingTexts(VisionContent vcontent, SpriteBatch spriteBatch, SpriteFont font)
            : base(vcontent.LoadEffect("effects/signtexteffect"))
        {
            VContent = vcontent;
            SpriteBatch = spriteBatch;
            Font = font;
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);
            foreach (var item in Items)
                item.Age += (float) gameTime.ElapsedGameTime.TotalSeconds;
            Items.RemoveAll(_ => _.Age > _.TimeToLive);
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            if (drawingReason != DrawingReason.Normal)
                return true;

            camera.UpdateEffect(Effect);
            foreach (var item in Items)
            {
                Effect.World = Matrix.BillboardRH(item.Target.Position + item.GetOffset(item), camera.Position, -camera.Up, camera.Front);
                Effect.DiffuseColor = item.GetColor(item);
                SpriteBatch.Begin(SpriteSortMode.Deferred, Effect.GraphicsDevice.BlendStates.NonPremultiplied, null, Effect.GraphicsDevice.DepthStencilStates.DepthRead, null, Effect.Effect);
                SpriteBatch.DrawString(Font, item.Text, Vector2.Zero, Color.Black, 0, Font.MeasureString(item.Text) / 2, item.GetSize(item), 0, 0);
                SpriteBatch.End();
            }

            Effect.GraphicsDevice.SetDepthStencilState(Effect.GraphicsDevice.DepthStencilStates.Default);
            Effect.GraphicsDevice.SetBlendState(Effect.GraphicsDevice.BlendStates.Opaque);

            return true;
        }

    }

}
