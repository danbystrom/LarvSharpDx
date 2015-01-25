using System;
using System.Collections.Generic;
using System.Linq;
using factor10.VisionThing.Effects;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace factor10.VisionThing.Primitives
{
    public interface IGeometricPrimitive
    {
        void Draw(IVEffect effect, int lod = 0);
    }

    public class PositionNormalTangentTexture
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector2 TextureCoordinate;

        public PositionNormalTangentTexture(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
            Tangent = tangent;
            TextureCoordinate = textureCoordinate;
        }
    }

    public abstract class GeometricPrimitive<T> : IVDrawable, IGeometricPrimitive where T : struct, IEquatable<T>
    {

        public delegate T CreateVertex(PositionNormalTangentTexture positionNormalTangentTexture);

        private List<T> _vertices = new List<T>();
        private List<List<uint>> _indicesOfLods = new List<List<uint>>();
        private List<uint> _indices;

        private Buffer<T> _vertexBuffer;
        private Buffer[] _indexBuffers;
        private VertexInputLayout _vertexInputLayout;

        protected GeometricPrimitive()
        {
            AddLevelOfDetail();
        }

        protected void AddVertex(T vertex)
        {
            _vertices.Add(vertex);
        }

        protected void AddVertex(CreateVertex createVertex, Vector3 position, Vector3 normal, Vector3 tangent, Vector2 textureCoordinate)
        {
            _vertices.Add(createVertex(new PositionNormalTangentTexture(position, normal, tangent, textureCoordinate)));
        }

        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");
            _indices.Add((ushort) index);
        }

        protected void AddTriangle(int i0, int i1, int i2, bool swap = false)
        {
            AddIndex(i0);
            AddIndex(swap ? i2 : i1);
            AddIndex(swap ? i1 : i2);
        }

        protected void AddLevelOfDetail()
        {
            if (_indices != null && !_indices.Any())
                return;
            _indices = new List<uint>();
            _indicesOfLods.Add(_indices);
        }

        protected void AddNullLevelOfDetail()
        {
            _indicesOfLods.Insert(_indicesOfLods.Count - 1, null);
        }

        protected int CurrentVertex
        {
            get { return _vertices.Count; }
        }

        protected void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            _vertexBuffer = Buffer.Vertex.New(graphicsDevice, _vertices.ToArray());
            _vertexInputLayout = VertexInputLayout.FromBuffer(0, _vertexBuffer);

            _indexBuffers = new Buffer[_indicesOfLods.Count];
            for (var i = 0; i < _indexBuffers.Length; i++)
                _indexBuffers[i] = createIndexBuffer(graphicsDevice, _indicesOfLods[i]);

            _vertices = null;
            _indices = null;
            _indicesOfLods = null;
        }

        private Buffer createIndexBuffer(GraphicsDevice graphicsDevice, List<uint> indices)
        {
            if (indices == null)
                return null;
            return _vertices.Count < 65536
                ? (Buffer) Buffer.Index.New(graphicsDevice, indices.ConvertAll(x => (ushort) x).ToArray())
                : Buffer.Index.New(graphicsDevice, indices.ToArray());
        }

        ~GeometricPrimitive()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }
            if (_indexBuffers != null)
            {
                foreach (var idx in _indexBuffers)
                    idx.Dispose();
                _indexBuffers = null;
            }
        }

        public void Draw(IVEffect effect, int lod = 0)
        {
            DrawSubset(effect, _indexBuffers[lod].ElementCount, _indexBuffers[lod]);
        }

        protected void DrawSubset(IVEffect effect, int elementCount, Buffer indexBuffer = null)
        {
            var graphicsDevice = effect.GraphicsDevice;
            indexBuffer = indexBuffer ?? _indexBuffers[0];

            effect.ForEachPass(() =>
            {
                graphicsDevice.SetVertexBuffer(_vertexBuffer);
                graphicsDevice.SetVertexInputLayout(_vertexInputLayout);
                graphicsDevice.SetIndexBuffer(indexBuffer, false);

                graphicsDevice.DrawIndexed(PrimitiveType.TriangleList, elementCount);
                VisionContent.RenderedTriangles += elementCount;
            });
        }

    }

}
