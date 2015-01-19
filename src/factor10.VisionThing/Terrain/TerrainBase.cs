using System;
using System.Diagnostics;
using System.Linq;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Objects;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Terrain
{
    public class TerrainBase : VDrawable
    {
        public const int Side = TerrainPlane.SquareSize;
        public const int HalfSide = Side/2;

        public readonly VisionContent VContent;

        public GroundMap GroundMap { get; protected set; }

        public Matrix World;
        private Vector3 _position;

        protected readonly Texture2D[] Textures = new Texture2D[9];

        protected Texture2D HeightsMap;
        protected Texture2D WeightsMap;
        protected Texture2D NormalsMap;

        protected TerrainSlice[] _slices;

        public int GroundExtentX { get; private set; }
        public int GroundExtentZ { get; private set; }

        public TerrainBase(VisionContent vContent)
            : base(vContent.TerrainPlane.Effect)
        {
            VContent = vContent;
            Effect.Parameters["Ambient"].SetValue(1.0f);
        }

        protected void initialize(GroundMap groundMap)
        {
            initialize(groundMap, groundMap.CreateWeigthsMap(), groundMap.CreateNormalsMap(ref World));
        }

        protected void initialize(GroundMap groundMap, WeightsMap weights, ColorSurface normals)
        {
            GroundMap = groundMap;

            Debug.Assert((groundMap.Width%TerrainPlane.SquareSize) == 0 && (groundMap.Height%TerrainPlane.SquareSize) == 0);

            _position = World.TranslationVector;

            Textures[0] = Textures[0] ?? VContent.Load<Texture2D>("terraintextures/sand");
            Textures[1] = Textures[1] ?? VContent.Load<Texture2D>("terraintextures/sahara");
            Textures[2] = Textures[2] ?? VContent.Load<Texture2D>("terraintextures/grass");
            Textures[3] = Textures[3] ?? VContent.Load<Texture2D>("terraintextures/rock");
            Textures[4] = Textures[4] ?? VContent.Load<Texture2D>("terraintextures/snow");
            Textures[5] = Textures[5] ?? VContent.Load<Texture2D>("terraintextures/stones");
            Textures[6] = Textures[6] ?? VContent.Load<Texture2D>("terraintextures/dirtground");
            Textures[7] = Textures[7] ?? VContent.Load<Texture2D>("terraintextures/path");
            Textures[8] = Textures[8] ?? VContent.Load<Texture2D>("terraintextures/wheatfield");

            HeightsMap = groundMap.CreateHeightsTexture(Effect.GraphicsDevice);
            WeightsMap = weights.CreateTexture2D(Effect.GraphicsDevice);
            NormalsMap = normals.CreateTexture2D(Effect.GraphicsDevice);

            var slicesW = groundMap.Width/Side;
            var slicesH = groundMap.Height/Side;
            //TODO - this is wrong - I guess...
            var sliceFracX = 1f/slicesW;
            var sliceFracY = 1f/slicesH;
            _slices = new TerrainSlice[slicesW*slicesH];

            var gurka = Vector3.TransformNormal(new Vector3(HalfSide, 0, HalfSide), World);
            var radius = Math.Max(gurka.X, gurka.Z) * (float)Math.Sqrt(2);

            var i = 0;
            for (var y = 0; y < slicesH; y++)
                for (var x = 0; x < slicesW; x++)
                {
                    var world = Matrix.Translation(Side*x, 0, Side*y)*World;
                    _slices[i++] = new TerrainSlice
                    {
                        TexOffsetAndScale = new Vector4(x*sliceFracX, y*sliceFracY, sliceFracX, sliceFracY),
                        World = world,
                        BoundingSphere = new BoundingSphere(world.TranslationVector + gurka, radius)
                    };
                }
            BoundingSphere = new BoundingSphere(
                _position + new Vector3(groundMap.Width, 0, groundMap.Height)/2,
                (float) Math.Sqrt(groundMap.Width*groundMap.Width + groundMap.Height*groundMap.Height)/2);

            GroundExtentX = slicesW;
            GroundExtentZ = slicesH;
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            var anyPartIsVisible = _slices.Aggregate(false,
                (current, slice) => current | (slice.Visible = camera.BoundingFrustum.Contains(slice.BoundingSphere) != ContainmentType.Disjoint));

            if (!anyPartIsVisible)
                return false;

            for (var i = 0; i < 9; i++)
            {
                var ep = Effect.Parameters["Texture" + (char)(48 + i)];
                if (ep != null)
                    ep.SetResource(Textures[i]);
            }

            Effect.Parameters["HeightsMap"].SetResource(HeightsMap);
            Effect.Parameters["NormalsMap"].SetResource(NormalsMap);
            Effect.Parameters["WeightsMap"].SetResource(WeightsMap);

            camera.UpdateEffect(Effect);

            foreach (var slice in _slices.Where(slice => slice.Visible))
            {
                Effect.Parameters["TexOffsetAndScale"].SetValue(slice.TexOffsetAndScale);
                VContent.TerrainPlane.Draw(camera, slice.World, drawingReason);
            }

            return true;
        }

        protected class TerrainSlice
        {
            public Vector4 TexOffsetAndScale;
            public Matrix World;
            public BoundingSphere BoundingSphere;
            public bool Visible;
        }

        public bool HitTest(Ray ray, out Vector3 hit, out Vector3 normal)
        {
            var world = World;
            world.Invert();
            if (!GroundMap.HitTest(world, ray, out hit, out normal))
                return false;
            hit = Vector3.TransformCoordinate(hit, World);
            normal = Vector3.TransformCoordinate(normal, World);
            return true;
        }

    }

}
