using System;
using System.Collections.Generic;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing
{
    public class ShadowMap : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;

        public readonly List<VDrawable> ShadowCastingObjects = new List<VDrawable>();
        public readonly Camera Camera;
        public Camera RealCamera { get; private set; }

        public readonly RenderTarget2D ShadowDepthTarget;
        public readonly DepthStencilBuffer DepthStencilTarget;

        // Depth texture parameters
        public int ShadowNearPlane;
        public int ShadowFarPlane;
        public float ShadowMult = 0.75f;

        private readonly SpriteBatch _spriteBatch;
        private readonly RenderTarget2D _shadowBlurTarg;
        private readonly IVEffect _shadowBlurEffect;

        public ShadowMap(
            VisionContent vContent,
            int width,
            int height,
            int nearPlane = 1,
            int farPlane = 200)
        {
            _graphicsDevice = vContent.GraphicsDevice;

            ShadowDepthTarget = RenderTarget2D.New(_graphicsDevice, width, height, PixelFormat.R16G16.Float);
            DepthStencilTarget = DepthStencilBuffer.New(_graphicsDevice, width, height, DepthFormat.Depth16);

            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _shadowBlurEffect = vContent.LoadEffect("Effects/Blur");
            _shadowBlurEffect.Parameters["dx"].SetValue(1f/width);
            _shadowBlurEffect.Parameters["dy"].SetValue(1f/height);
            _shadowBlurTarg = RenderTarget2D.New(_graphicsDevice, width, height, PixelFormat.R16G16.Float);

            ShadowNearPlane = nearPlane;
            ShadowFarPlane = farPlane;
            Camera = new Camera(
                new Vector2(width, height),
                Vector3.Zero,
                Vector3.Up,
                ShadowNearPlane,
                ShadowFarPlane);
            UpdateProjection(60, 60);
        }

        public void UpdateProjection(int width, int height, int? near = null, int? far = null)
        {
            Camera.Projection = Matrix.OrthoRH(
                width,
                height,
                near.GetValueOrDefault(ShadowNearPlane),
                far.GetValueOrDefault(ShadowFarPlane));
        }

        public void Draw(Camera camera)
        {
            RealCamera = camera;

            _graphicsDevice.SetRenderTargets(DepthStencilTarget, ShadowDepthTarget);
            _graphicsDevice.Clear(Color.White); // Clear the render target to 1 (infinite depth)
            foreach (var obj in ShadowCastingObjects)
                obj.Draw(Camera, DrawingReason.ShadowDepthMap, this);

            blurShadow(_shadowBlurTarg, ShadowDepthTarget);
            blurShadow(ShadowDepthTarget, _shadowBlurTarg);

            _graphicsDevice.SetDepthStencilState(_graphicsDevice.DepthStencilStates.Default);
            _graphicsDevice.SetBlendState(_graphicsDevice.BlendStates.Opaque);
            _graphicsDevice.SetRenderTargets(_graphicsDevice.DepthStencilBuffer, _graphicsDevice.BackBuffer);
        }

        private void blurShadow(RenderTarget2D to, RenderTarget2D from)
        {
            _graphicsDevice.SetRenderTargets(to);
            _graphicsDevice.Clear(Color.Black);
            _shadowBlurEffect.Apply();
            _spriteBatch.Begin(SpriteSortMode.Deferred,  _shadowBlurEffect.Effect);
            _spriteBatch.Draw(from, Vector2.Zero, Color.White);
            _spriteBatch.End();
            _shadowBlurEffect.Texture = null;
        }

        public void Dispose()
        {
            ShadowDepthTarget.Dispose();
            DepthStencilTarget.Dispose();
            _spriteBatch.Dispose();
            _shadowBlurTarg.Dispose();
            _shadowBlurEffect.Dispose();
        }

    }

}
