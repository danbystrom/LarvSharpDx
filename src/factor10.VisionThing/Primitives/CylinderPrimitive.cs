using System;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing cylinders.
    /// </summary>
    public class CylinderPrimitive<T> : GeometricPrimitive<T> where T : struct, IEquatable<T>
    {
        public delegate T CreateVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 txCoor);

        /// <summary>
        /// Constructs a new cylinder primitive, using default settings.
        /// </summary>
        public CylinderPrimitive(GraphicsDevice graphicsDevice, CreateVertex createVertex, bool swap=false)
            : this(graphicsDevice, createVertex, 1, 1, 32, swap)
        {
        }

        /// <summary>
        /// Constructs a new cylinder primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public CylinderPrimitive(
            GraphicsDevice graphicsDevice,
            CreateVertex createVertex,
            float height,
            float diameter, 
            int ringTessellation,
            bool swap = false)
        {
            if (ringTessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            height /= 2;
            var radius = diameter/2;
            var t2 = ringTessellation*2;

            // Create a ring of triangles around the outside of the cylinder.
            for (var i = 0; i < ringTessellation; i++)
            {
                var normal = getCircleVector(i, ringTessellation);

                var txX = 1 - (float) i/ringTessellation;
                var tangent = new Vector3(-normal.Z, 0, normal.X);
                addVertex(createVertex(normal * radius + Vector3.Up * height, normal, tangent, new Vector2(txX, 0)));
                addVertex(createVertex(normal * radius + Vector3.Down * height, normal, tangent, new Vector2(txX, 1)));

                addTriangle(i*2, i*2 + 1, (i*2 + 2)%t2, swap);
                addTriangle(i*2 + 1, (i*2 + 3)%t2, (i*2 + 2)%t2, swap);
            }

            // Create flat triangle fan caps to seal the top and bottom.
            CreateCap(createVertex, ringTessellation, height, radius, Vector3.Up, swap);
            CreateCap(createVertex, ringTessellation, height, radius, Vector3.Down, swap);

            initializePrimitive(graphicsDevice);
        }


        /// <summary>
        /// Helper method creates a triangle fan to close the ends of the cylinder.
        /// </summary>
        private void CreateCap(CreateVertex createVertex, int tessellation, float height, float radius, Vector3 normal, bool swap)
        {
            // Create cap indices.
            for (var i = 0; i < tessellation - 2; i++)
                addTriangle(CurrentVertex, CurrentVertex + (i + 2)%tessellation, CurrentVertex + (i + 1)%tessellation, swap ^ normal.Y > 0);

            // Create cap vertices.
            for (var i = 0; i < tessellation; i++)
            {
                var position = getCircleVector(i, tessellation)*radius + normal*height;
                addVertex(createVertex(position, normal, Vector3.Zero, Vector2.Zero));
            }
        }

        private static Vector3 getCircleVector(int i, int tessellation)
        {
            var angle = i*MathUtil.TwoPi/tessellation;
            var dx = (float) Math.Cos(angle);
            var dz = (float) Math.Sin(angle);
            return new Vector3(dx, 0, dz);
        }

    }

}
