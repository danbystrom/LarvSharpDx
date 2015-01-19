using System;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing toruses.
    /// </summary>
    public class TorusPrimitive<T> : GeometricPrimitive<T> where T : struct, IEquatable<T>
    {
        public delegate T CreateVertex(Vector3 position, Vector3 normal);

        /// <summary>
        /// Constructs a new torus primitive, using default settings.
        /// </summary>
        //public TorusPrimitive(GraphicsDevice graphicsDevice)
        //    : this(graphicsDevice, 1, 0.333f, 32)
        //{
        //}

        /// <summary>
        /// Constructs a new torus primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public TorusPrimitive(
            GraphicsDevice graphicsDevice,
            CreateVertex createVertex,
            float diameter,
            float thickness,
            int tessellation)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            // First we loop around the main ring of the torus.
            for (var i = 0; i < tessellation; i++)
            {
                var outerAngle = i * MathUtil.TwoPi / tessellation;

                // Create a transform matrix that will align geometry to
                // slice perpendicularly though the current ring position.
                var transform = Matrix.Translation(diameter / 2, 0, 0) *
                                   Matrix.RotationY(outerAngle);

                // Now we loop along the other axis, around the side of the tube.
                for (var j = 0; j < tessellation; j++)
                {
                    var innerAngle = j * MathUtil.TwoPi / tessellation;

                    var dx = (float)Math.Cos(innerAngle);
                    var dy = (float)Math.Sin(innerAngle);

                    // Create a vertex.
                    var normal = new Vector3(dx, dy, 0);
                    var position = normal * thickness / 2;

                    position = Vector3.TransformCoordinate(position, transform);
                    normal = Vector3.TransformNormal(normal, transform);

                    addVertex(createVertex(position, normal));

                    // And create indices for two triangles.
                    var nextI = (i + 1) % tessellation;
                    var nextJ = (j + 1) % tessellation;

                    addIndex(i * tessellation + j);
                    addIndex(i * tessellation + nextJ);
                    addIndex(nextI * tessellation + j);

                    addIndex(i * tessellation + nextJ);
                    addIndex(nextI * tessellation + nextJ);
                    addIndex(nextI * tessellation + j);
                }
            }

            initializePrimitive(graphicsDevice);
        }
    }
}
