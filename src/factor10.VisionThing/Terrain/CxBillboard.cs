using System;
using System.Collections.Generic;
using System.Linq;
using factor10.VisionThing.CameraStuff;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace factor10.VisionThing.Terrain
{
    public class CxBillboard : VDrawable
    {
        public Matrix World;

        private Buffer<CxBillboardVertex> _vertexBuffer;
        private VertexInputLayout _vertexInputLayout;

        private readonly Texture2D _texture;
        private readonly float _billboardWidth;
        private readonly float _billboardHeight;
        private readonly float _windAmount;

        private float _time;

        private List<Tuple<Vector3, Vector3>> _items = new List<Tuple<Vector3, Vector3>>();

        public CxBillboard(
            VisionContent vContent,
            Matrix world,
            Texture2D texture,
            float width,
            float height,
            float windAmount)
            : base(vContent.LoadEffect("Billboards/CxBillboard", vContent.GraphicsDevice.SamplerStates.LinearClamp))
        {
            World = world;
            _texture = texture;
            _billboardWidth = width;
            _billboardHeight = height;
            _windAmount = windAmount;
        }

        public void GenerateTreePositions(GroundMap groundMap, ColorSurface normals)
        {
            generateTreePositions(groundMap, normals);
            CreateVertices();
        }

        public CxBillboard Add(Vector3 position, Vector3 normal)
        {
            _items.Add(new Tuple<Vector3, Vector3>(position, normal));
            return this;
        }

        public CxBillboard AddPositionsAndNormals(params Vector3[] positionsAndNormals)
        {
            for (var i = 0; i < positionsAndNormals.Length/2; i++)
                Add(positionsAndNormals[i], positionsAndNormals[i + 1]);
            return this;
        }

        public CxBillboard AddPositionsWithSameNormal(Vector3 normal, params Vector3[] positions)
        {
            foreach (var position in positions)
                Add(position, normal);
            return this;
        }

        public CxBillboard CreateVertices(bool randomize = true)
        {
            if (_items==null || !_items.Any())
                return this;

            var billboardVertices = new CxBillboardVertex[_items.Count];
            var i = 0;
            var random = new Random();
            foreach (var t in _items)
                createOne(
                    ref i,
                    billboardVertices,
                    t.Item1 + World.TranslationVector,
                    t.Item2,
                    randomize ? 0.0001f + (float) random.NextDouble() : 0.5f);
            _items = null;

            _vertexBuffer = Buffer.Vertex.New(Effect.GraphicsDevice, billboardVertices);
            _vertexInputLayout = VertexInputLayout.FromBuffer(0, _vertexBuffer);

            return this;
        }

        private void createOne(
            ref int i,
            CxBillboardVertex[] bv,
            Vector3 p,
            Vector3 n,
            float rnd)
        {
            bv[i++] = new CxBillboardVertex(p, Vector3.Normalize(n), rnd);
        }

        private void generateTreePositions(GroundMap groundMap, ColorSurface normals)
        {
            var random = new Random();

            for (var y = normals.Height - 2; y > 0; y--)
                for (var x = normals.Width - 2; x > 0; x--)
                {
                    var height = groundMap[x, y];
                    if ( height <3 || height > 5)
                        continue;
                    for (var currDetail = 0; currDetail < 5; currDetail++)
                    {
                        var rand1 = (float) random.NextDouble();
                        var rand2 = (float) random.NextDouble();
                        _items.Add(new Tuple<Vector3, Vector3>(
                                         new Vector3(
                                             x + rand1,
                                             groundMap.GetExactHeight(x, y, rand1, rand2),
                                             y + rand2),
                                         normals.AsVector3(x, y)));
                    }
                }
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            if (_vertexBuffer == null)
                return false;

            camera.UpdateEffect(Effect);
            Effect.World = World;
            Effect.Texture = _texture;

            var gd = Effect.GraphicsDevice;
            gd.SetVertexBuffer(_vertexBuffer);
            gd.SetVertexInputLayout(_vertexInputLayout);

            Effect.Parameters["WindTime"].SetValue(_time);
            Effect.Parameters["WindAmount"].SetValue(_windAmount);
            Effect.Parameters["BillboardWidth"].SetValue(_billboardWidth);
            Effect.Parameters["BillboardHeight"].SetValue(_billboardHeight);

            //pass one
            Effect.Parameters["AlphaTestDirection"].SetValue(1f);
            Effect.Effect.CurrentTechnique.Passes[0].Apply();
            gd.Draw(PrimitiveType.PointList, _vertexBuffer.ElementCount);

            if (drawingReason == DrawingReason.Normal)
            {
                //pass two
                gd.SetDepthStencilState(gd.DepthStencilStates.DepthRead);
                gd.SetBlendState(gd.BlendStates.NonPremultiplied);
                Effect.Parameters["AlphaTestDirection"].SetValue(-1f);
                Effect.Effect.CurrentTechnique.Passes[0].Apply();
                gd.Draw(PrimitiveType.PointList, _vertexBuffer.ElementCount);
                gd.SetDepthStencilState(gd.DepthStencilStates.Default);
                gd.SetBlendState(gd.BlendStates.Default);
            }

            return true;
        }

        public override void Dispose()
        {
            _vertexBuffer.Dispose();
        }

    }

}

