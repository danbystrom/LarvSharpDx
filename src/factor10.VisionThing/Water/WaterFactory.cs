using System;
using factor10.VisionThing.CameraStuff;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Water
{
    public static class WaterFactory
    {
        public static WaterSurface Create(VisionContent vContent)
        {
            return new WaterSurface(
                vContent.GraphicsDevice,
                new InitInfo
                    {
                        Fx = vContent.Load<Effect>("effects/reflectedwater"),
                        LightDirection = VisionContent.SunlightDirectionReflectedWater,
                        SquareSize = 128,
                        dx = 0.25f,
                        dz = 0.25f,
                        dmap0 = foobar(vContent, "water/waterdmap0"),
                        dmap1 = foobar(vContent, "water/waterdmap1"),
                        waveMap0 = vContent.Load<Texture2D>("water/waterbump"),
                        waveMap1 = vContent.Load<Texture2D>("water/wave1"),
                        waveBumpMapVelocity0 = new Vector2(0.012f, 0.016f),
                        waveBumpMapVelocity1 = new Vector2(0.014f, 0.018f),
                        waveDispMapVelocity0 = new Vector2(0.012f, 0.015f),
                        waveDispMapVelocity1 = new Vector2(0.014f, 0.05f),
                        scaleHeights = new Vector2(0.7f, 1.1f),
                        texScale = 8.0f
                    });
        }

        private static Texture2D foobar(VisionContent vContent, string name)
        {
            using (var z = vContent.Load<Texture2D>(name))
            {
                var oldData = new Color[z.Width*z.Height];
                var newData = new float[z.Width*z.Height];
                z.GetData(oldData);
                for (var i = 0; i < oldData.Length; i++)
                    newData[i] = oldData[i].R/255f;

                var result = Texture2D.New(vContent.GraphicsDevice, z.Width, z.Height, PixelFormat.R32.Float);
                result.SetData(newData);
                return result;
            }

        }

        public static void DrawWaterSurfaceGrid(
            WaterSurface waterSurface,
            Camera camera,
            ShadowMap shadow,
            int nisse,
            int surfaceSize,
            int surfaceScale)
        {
            const int waterW = 64;
            const int waterH = 64;
            const int worldW = 32;
            const int worldH = 32;

            var boundingFrustum = camera.BoundingFrustum;

            waterSurface.Effect.SetShadowMapping(shadow);

            var gridStartX = (int) camera.Position.X/waterW - worldW/2;
            var gridStartY = (int) camera.Position.Z/waterH - worldH/2;

            Array.Clear(RenderedWaterPlanes, 0, RenderedWaterPlanes.Length);

            var drawDetails = camera.Position.Y < -1; // fix the sea water some time
            if (drawDetails)
                for (var y = 0; y <= worldH; y++)
                    for (var x = 0; x <= worldW; x++)
                    {
                        var pos1 = new Vector3((gridStartX + x) * waterW, 0, (gridStartY + y) * waterH);
                        var pos2 = pos1 + new Vector3(waterW, 1, waterH);
                        var bb = new BoundingBox(pos1, pos2);
                        if (boundingFrustum.Contains(bb) == ContainmentType.Disjoint)
                            continue;

                        waterSurface.Draw(
                            camera,
                            pos1,
                            Vector3.Distance(camera.Position, pos1 - new Vector3(-32, 0, -32)),
                            x % 8,
                            y % 8,
                            1 << surfaceScale);
                    }

            var raise = 0.5f + camera.Position.Y/500;
            var q = (int)camera.ZFar & ~(waterW - 1);
            var pos = new Vector3(gridStartX * waterW - q, raise, gridStartY * waterH - q);
            waterSurface.Draw(camera, pos, -1, 0, 0, 1 << surfaceScale);
        }

        public static int[] RenderedWaterPlanes = new int[6];

    }

}
