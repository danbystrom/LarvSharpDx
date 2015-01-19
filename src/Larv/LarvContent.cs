using System.Collections;
using System.Collections.Generic;
using factor10.VisionThing;
using factor10.VisionThing.Effects;
using factor10.VisionThing.Primitives;
using Larv.Field;
using Larv.Hof;
using SharpDX;
using SharpDX.Toolkit.Content;
using SharpDX.Toolkit.Graphics;
using System;

namespace Larv
{
    public class LarvContent : VisionContent
    {
        public readonly SpriteBatch SpriteBatch;
        public readonly SpriteFont Font;
        public readonly VisionEffect SignTextEffect;
        public readonly VisionEffect TextureEffect;
        public readonly VisionEffect BumpEffect;
        public readonly SkySphere Sky;
        public readonly IVDrawable Sphere;
        public readonly ShadowMap ShadowMap;

        public readonly Ground Ground;
        public readonly HallOfFame HallOfFame;
        public readonly PlayingFieldInfo[] PlayingFieldInfos;

        public LarvContent(GraphicsDevice graphicsDevice, ContentManager content, IEnumerable<string> sceneDescription)
            : base(graphicsDevice, content)
        {
            SpriteBatch = new SpriteBatch(graphicsDevice);
            Font = Load<SpriteFont>("fonts/BlackCastle");
            SignTextEffect = LoadEffect("effects/signtexteffect");
            TextureEffect = LoadEffect("effects/simpletextureeffect");
            BumpEffect = LoadEffect("effects/simplebumpeffect");
            Sphere = new SpherePrimitive<VertexPositionNormalTangentTexture>(GraphicsDevice,
                (p, n, t, tx) => new VertexPositionNormalTangentTexture(p, n, t, tx), 2, 10);
            Sky = new SkySphere(this, Load<TextureCube>(@"Textures\clouds"));
            Ground = new Ground(this);
            ShadowMap = new ShadowMap(this, 800, 800, 1, 50);
            ShadowMap.UpdateProjection(50, 30);
            HallOfFame = HofStorage.Load();
            PlayingFieldInfos = PlayingFieldsDecoder.Create(sceneDescription);
        }

        public float FontScaleRatio
        {
            get { return GraphicsDevice.BackBuffer.Width/1920f; }
        }

        public override void Dispose()
        {
            base.Dispose();
            SpriteBatch.Dispose();
            Sphere.Dispose();
            Ground.Dispose();
            ShadowMap.Dispose();
        }

        private class EndSpriteBatch : IDisposable
        {
            public SpriteBatch SpriteBatch;
            public void Dispose()
            {
                SpriteBatch.End();
            }
        }

        public IDisposable UsingSpriteBatch()
        {
            SpriteBatch.Begin(SpriteSortMode.Deferred, GraphicsDevice.BlendStates.NonPremultiplied);
            return new EndSpriteBatch {SpriteBatch = SpriteBatch};
        }

        public void DrawString(string text, Vector2 pos, float size, float align = 0, Color? color = null)
        {
            var c = color.GetValueOrDefault(Color.LightYellow);
            var valign = new Vector2(Font.MeasureString(text).X*align, 0);
            size *= FontScaleRatio;
            SpriteBatch.DrawString(
                Font,
                text,
                pos + Vector2.One,
                Color.Black*(c.A/255f),
                0,
                valign,
                size,
                SpriteEffects.None,
                0);
            SpriteBatch.DrawString(
                Font,
                text,
                pos,
                c,
                0,
                valign,
                size,
                SpriteEffects.None,
                0);
        }

    }

}
