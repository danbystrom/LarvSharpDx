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
    public class StaticBillboard : VDrawable
    {
        private readonly Matrix _world;
        private Buffer<StaticBillboardVertex> _vertexBuffer;
        private VertexInputLayout _vertexInputLayout;

        private readonly Texture2D _texture;
        private readonly float _billboardWidth;
        private readonly float _billboardHeight;

        private List<Tuple<Vector3, Vector3, Vector3>> _items = new List<Tuple<Vector3, Vector3, Vector3>>();

        public StaticBillboard(
            VisionContent vContent,
            Matrix world,
            Texture2D texture,
            float width,
            float height)
            : base(vContent.LoadEffect("Billboards/StaticBillboard", vContent.GraphicsDevice.SamplerStates.LinearClamp))
        {
            _world = world;
            _texture = texture;
            _billboardWidth = width;
            _billboardHeight = height;
        }

        public void Add(Vector3 position, Vector3 normal, Vector3 front)
        {
            _items.Add(new Tuple<Vector3, Vector3, Vector3>(position, normal, front));
        }

        public void CreateBillboardVertices()
        {
            if (_items==null || !_items.Any())
                return;

            var billboardVertices = new StaticBillboardVertex[_items.Count * 6];
            var i = 0;
            var random = new Random();
            foreach (var t in _items)
                createOne(
                    ref i,
                    billboardVertices,
                    t.Item1 + _world.TranslationVector,
                    t.Item2,
                    t.Item3);
            _items = null;

            _vertexBuffer = Buffer.Vertex.New(Effect.GraphicsDevice, billboardVertices);
            _vertexInputLayout = VertexInputLayout.FromBuffer(0, _vertexBuffer);
        }

        private void createOne(
            ref int i,
            StaticBillboardVertex[] bv,
            Vector3 p,
            Vector3 n,
            Vector3 front)
        {
            n.Normalize();
            bv[i++] = new StaticBillboardVertex(p, n, front, new Vector2(0, 0));
            bv[i++] = new StaticBillboardVertex(p, n, front, new Vector2(1, 0));
            bv[i++] = new StaticBillboardVertex(p, n, front, new Vector2(1, 1));

            bv[i++] = new StaticBillboardVertex(p, n, front, new Vector2(0, 0));
            bv[i++] = new StaticBillboardVertex(p, n, front, new Vector2(1, 1));
            bv[i++] = new StaticBillboardVertex(p, n, front, new Vector2(0, 1));
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            if (_vertexBuffer == null)
                return false;

            camera.UpdateEffect(Effect);
            Effect.World = _world;
            Effect.Texture = _texture;

            Effect.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            Effect.GraphicsDevice.SetVertexInputLayout(_vertexInputLayout);

            Effect.Parameters["BillboardWidth"].SetValue(_billboardWidth);
            Effect.Parameters["BillboardHeight"].SetValue(_billboardHeight);

            //pass one
            Effect.Parameters["AlphaTestDirection"].SetValue(1f);
            Effect.Effect.CurrentTechnique.Passes[0].Apply();
            Effect.GraphicsDevice.Draw(PrimitiveType.TriangleList, _vertexBuffer.ElementCount);

            if (drawingReason == DrawingReason.Normal)
            {
                //pass two
                Effect.GraphicsDevice.SetDepthStencilState(Effect.GraphicsDevice.DepthStencilStates.DepthRead);
                Effect.GraphicsDevice.SetBlendState(Effect.GraphicsDevice.BlendStates.NonPremultiplied);
                Effect.Parameters["AlphaTestDirection"].SetValue(-1f);
                Effect.Effect.CurrentTechnique.Passes[0].Apply();
                Effect.GraphicsDevice.Draw(PrimitiveType.TriangleList, _vertexBuffer.ElementCount);
                Effect.GraphicsDevice.SetDepthStencilState(Effect.GraphicsDevice.DepthStencilStates.Default);
                Effect.GraphicsDevice.SetBlendState(Effect.GraphicsDevice.BlendStates.Default);
            }

            return true;
        }

    }

}