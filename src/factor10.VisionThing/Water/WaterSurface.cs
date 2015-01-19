using System;
using System.Collections.Generic;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using factor10.VisionThing.Primitives;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit.Input;

namespace factor10.VisionThing.Water
{

    public struct InitInfo
    {
        public Effect Fx;
        public Vector3 LightDirection;
        public int SquareSize;
        public float dx;
        public float dz;
        public Texture waveMap0;
        public Texture waveMap1;
        public Texture dmap0;
        public Texture dmap1;
        public Vector2 waveBumpMapVelocity0;
        public Vector2 waveBumpMapVelocity1;
        public Vector2 waveDispMapVelocity0;
        public Vector2 waveDispMapVelocity1;
        public Vector2 scaleHeights;
        public float texScale;
        public Texture Checker;
    }

    public class WaterSurface
    {
        // Offset of normal maps for scrolling (vary as a function of time)
        private Vector2 _waveBumpMapOffset0;
        private Vector2 _waveBumpMapOffset1;
        private readonly Vector2 _waveBumpMapVelocity0;
        private readonly Vector2 _waveBumpMapVelocity1;

        // Offset of displacement maps for scrolling (vary as a function of time)
        private Vector2 _waveDispMapOffset0;
        private Vector2 _waveDispMapOffset1;
        private readonly Vector2 _waveDispMapVelocity0;
        private readonly Vector2 _waveDispMapVelocity1;

        private readonly PlanePrimitive<WaterVertex> _hiPolyPlane;
        private readonly PlanePrimitive<WaterVertex> _lakePlane;

        public readonly RenderTarget2D _reflectionTarget;

        public readonly List<VDrawable> ReflectedObjects = new List<VDrawable>();
        private readonly Camera _reflectionCamera;

        public IVEffect Effect;

        public WaterSurface(
            GraphicsDevice graphicsDevice,
            InitInfo initInfo)
        {
            Effect = new VisionEffect(initInfo.Fx, graphicsDevice.SamplerStates.LinearClamp)
            {
                SunlightDirection = VisionContent.SunlightDirection - new Vector3(0, VisionContent.SunlightDirection.Y/2, 0)
            };

            _waveBumpMapVelocity0 = initInfo.waveBumpMapVelocity0;
            _waveBumpMapVelocity1 = initInfo.waveBumpMapVelocity1;
            _waveDispMapVelocity0 = initInfo.waveDispMapVelocity0;
            _waveDispMapVelocity1 = initInfo.waveDispMapVelocity1;

            _hiPolyPlane = generatePlane(graphicsDevice, initInfo.SquareSize, initInfo.dx, initInfo.dz,
                                         initInfo.texScale);

            _lakePlane = generatePlane(graphicsDevice, 1, initInfo.SquareSize*512, initInfo.SquareSize*512, initInfo.texScale*512);

            buildFx(initInfo);

            var targetWidth = graphicsDevice.BackBuffer.Width;
            var targetHeight = graphicsDevice.BackBuffer.Height;
            _reflectionTarget = RenderTarget2D.New(
                graphicsDevice,
                targetWidth,
                targetHeight*11/10, //compensate for displaced waves
                graphicsDevice.BackBuffer.Format);

            _reflectionCamera = new Camera(
                new Vector2(targetWidth, targetHeight),
                null,
                null,
                null,
                Vector3.Zero,
                Vector3.Up);

            Update(10);
        }

        private PlanePrimitive<WaterVertex> generatePlane(GraphicsDevice graphicsDevice, int squareSize, float dx, float dz, float texScale)
        {
            return new PlanePrimitive<WaterVertex>(
                graphicsDevice,
                (x, y, width, height) =>
                    new WaterVertex(new Vector3(2*x*dx, 0, 2*y*dz), new Vector2(x/squareSize, y/squareSize), new Vector2(x/squareSize, y/squareSize)*texScale),
                squareSize,
                squareSize,
                6);
        }

        public void Update(float dt)
        {
            // Update texture coordinate offsets.  These offsets are added to the
            // texture coordinates in the vertex shader to animate them.
            _waveBumpMapOffset0 += _waveBumpMapVelocity0*dt;
            _waveBumpMapOffset1 += _waveBumpMapVelocity1*dt;
            _waveDispMapOffset0 += _waveDispMapVelocity0*dt;
            _waveDispMapOffset1 += _waveDispMapVelocity1*dt;

            // Textures repeat every 1.0 unit, so reset back down to zero
            // so the coordinates do not grow too large.
            wrap(ref _waveBumpMapOffset0);
            wrap(ref _waveBumpMapOffset1);
            wrap(ref _waveDispMapOffset0);
            wrap(ref _waveDispMapOffset1);
        }

        private void wrap(ref Vector2 vec)
        {
            if (vec.X >= 4.0f || vec.X <= -4.0f)
                vec.X -= 4 * Math.Sign(vec.X);
            if (vec.Y >= 200.0f || vec.Y <= -4.0f)
                vec.Y -= 4*Math.Sign(vec.Y);
        }

        public void Draw(Camera camera, Vector3 pos, float distance, int x, int y, int lakeTextureTransformation)
        {
            var world = Matrix.Translation(pos);
            Effect.World = world;
            Effect.View = camera.View;
            Effect.Projection = camera.Projection;
            _mhWorldInv.SetValue(Matrix.Invert(world));
            _mhCameraPosition.SetValue(camera.Position);

            _mhWaveBumpMapOffset0.SetValue(_waveBumpMapOffset0);
            _mhWaveBumpMapOffset1.SetValue(_waveBumpMapOffset1);
            _mhWaveDispMapOffset0.SetValue(_waveDispMapOffset0);
            _mhWaveDispMapOffset1.SetValue(_waveDispMapOffset1);

            if ( distance < 0 )
            {
                WaterFactory.RenderedWaterPlanes[5]++;
                Effect.Parameters["LakeTextureTransformation"].SetValue(new Vector4(0, 0, lakeTextureTransformation, lakeTextureTransformation));
                Effect.Effect.CurrentTechnique = Effect.Effect.Techniques[1];
                _lakePlane.Draw(Effect);
                return;
            }

            Effect.Parameters["LakeTextureTransformation"].SetValue(new Vector4(-x, -y, 2, 2));
            Effect.Effect.CurrentTechnique = Effect.Effect.Techniques[0];

            if (distance < 80)
            {
                _hiPolyPlane.Draw(Effect);
                WaterFactory.RenderedWaterPlanes[0]++;
            }
            else if (distance < 160)
            {
                WaterFactory.RenderedWaterPlanes[1]++;
                world *= Matrix.Translation(0, -0.10f, 0);
                Effect.World = world;
                _mhWorldInv.SetValue(Matrix.Invert(world));
                _hiPolyPlane.Draw(Effect, 1);
            }
            else if (distance < 400)
            {
                WaterFactory.RenderedWaterPlanes[2]++;
                world *= Matrix.Translation(0, -0.20f, 0);
                Effect.World = world;
                _mhWorldInv.SetValue(Matrix.Invert(world));
                _hiPolyPlane.Draw(Effect, 3);
            }
            else
            {
                var lod = 5;
                if (distance > 800)
                {
                    WaterFactory.RenderedWaterPlanes[4]++;
                    Effect.Effect.CurrentTechnique = Effect.Effect.Techniques[1];
                    lod = 5;
                }
                else
                    WaterFactory.RenderedWaterPlanes[3]++;
                world *= Matrix.Translation(0, -0.30f, 0);
                Effect.World = world;
                _mhWorldInv.SetValue(Matrix.Invert(world));
                _hiPolyPlane.Draw(Effect, lod);
            }

        }


        private EffectParameter _mhWorldInv;
        private EffectParameter _mhCameraPosition;
        private EffectParameter _mhWaveBumpMapOffset0;
        private EffectParameter _mhWaveBumpMapOffset1;
        private EffectParameter _mhWaveDispMapOffset0;
        private EffectParameter _mhWaveDispMapOffset1;

        private EffectParameter _reflectedView;
        private EffectParameter _reflectedMap;

        private void buildFx(InitInfo initInfo)
        {
            var p = Effect.Effect.Parameters;

            p["MirrorSampler"].SetResource(Effect.GraphicsDevice.SamplerStates.PointMirror);

            _mhWorldInv = p["WorldInv"];
            _mhCameraPosition = p["CameraPosition"];
            _mhWaveBumpMapOffset0 = p["WaveNMapOffset0"];
            _mhWaveBumpMapOffset1 = p["WaveNMapOffset1"];
            _mhWaveDispMapOffset0 = p["WaveDMapOffset0"];
            _mhWaveDispMapOffset1 = p["WaveDMapOffset1"];
            _reflectedView = p["ReflectedView"];
            _reflectedMap = p["ReflectedMap"];

            p["BumpMap0"].SetResource(initInfo.waveMap0);
            p["BumpMap1"].SetResource(initInfo.waveMap1);

            p["WaveDispMap0"].SetResource(initInfo.dmap0);
            p["WaveDispMap1"].SetResource(initInfo.dmap1);
            p["ScaleHeights"].SetValue(initInfo.scaleHeights);
            p["GridStepSizeL"].SetValue(new Vector2(initInfo.dx, initInfo.dz));
            p["WaveHeight"].SetValue(0.3f * 2);
        }

        public void RenderReflection(Camera camera)
        {
            const float waterMeshPositionY = 0.75f; //experimenting with this

            // Reflect the camera's properties across the water plane
            var reflectedCameraPosition = camera.Position;
            reflectedCameraPosition.Y = -reflectedCameraPosition.Y + 1 + waterMeshPositionY * 2;
            var reflectedCameraTarget = camera.Target;
            reflectedCameraTarget.Y = -reflectedCameraTarget.Y + 1 + waterMeshPositionY * 2;
 
            _reflectionCamera.Update(
                reflectedCameraPosition,
                reflectedCameraTarget);

            Effect.GraphicsDevice.SetRenderTargets(Effect.GraphicsDevice.DepthStencilBuffer, _reflectionTarget);
            Effect.GraphicsDevice.Clear(Color.CornflowerBlue);

            var clipPlane = new Vector4(0, 1, 0, -waterMeshPositionY);
            foreach (var cd in ReflectedObjects)
                cd.DrawReflection(clipPlane, _reflectionCamera);

            Effect.GraphicsDevice.SetRenderTargets(Effect.GraphicsDevice.DepthStencilBuffer, Effect.GraphicsDevice.BackBuffer);

            _reflectedView.SetValue(_reflectionCamera.View);
            _reflectedMap.SetResource(_reflectionTarget);
        }

    }

}