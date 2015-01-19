using System;
using System.Collections.Generic;
using System.Linq;
using factor10.VisionThing.Effects;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace factor10.VisionThing.Primitives
{
    public interface IGeometricPrimitive
    {
        void Draw(IVEffect effect, int lod = 0);
    }

    public abstract class GeometricPrimitive<T> : IDisposable, IVDrawable, IGeometricPrimitive where T : struct, IEquatable<T>
    {
        private List<T> _vertices = new List<T>();
        private List<List<uint>> _indicesOfLods = new List<List<uint>>();
        private List<uint> _indices;

        private Buffer<T> _vertexBuffer;
        private Buffer[] _indexBuffers;
        private VertexInputLayout _vertexInputLayout;

        protected GeometricPrimitive()
        {
            addLevelOfDetail();
        }

        protected void addVertex(T vertex)
        {
            _vertices.Add(vertex);
        }

        protected void addIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");
            _indices.Add((ushort) index);
        }

        protected void addTriangle(int i0, int i1, int i2, bool swap=false)
        {
            addIndex(i0);
            addIndex(swap ? i2 : i1);
            addIndex(swap ? i1 : i2);
        }

        protected void addLevelOfDetail()
        {
            if (_indices != null && !_indices.Any())
                return;
            _indices = new List<uint>();
            _indicesOfLods.Add(_indices);
        }

        protected void addNullLevelOfDetail()
        {
            _indicesOfLods.Insert(_indicesOfLods.Count - 1, null);
        }

        protected int CurrentVertex
        {
            get { return _vertices.Count; }
        }

        protected void initializePrimitive(GraphicsDevice graphicsDevice)
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
            var graphicsDevice = effect.GraphicsDevice;

            foreach (var effectPass in effect.Effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                graphicsDevice.SetVertexBuffer(_vertexBuffer);
                graphicsDevice.SetVertexInputLayout(_vertexInputLayout);
                graphicsDevice.SetIndexBuffer(_indexBuffers[lod], false);

                graphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _indexBuffers[lod].ElementCount);
                VisionContent.RenderedTriangles += _indexBuffers[lod].ElementCount;
            }
        }

        public void Draw(BasicEffect effect, int lod = 0)
        {
            var graphicsDevice = effect.GraphicsDevice;

            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.SetIndexBuffer(_indexBuffers[lod], false);

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _indexBuffers[lod].ElementCount);
                VisionContent.RenderedTriangles += _indexBuffers[lod].ElementCount;
            }

        }

    }

}
