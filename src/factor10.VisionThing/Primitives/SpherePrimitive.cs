using System;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing spheres.
    /// </summary>
    public class SpherePrimitive<T> : GeometricPrimitive<T> where T : struct, IEquatable<T>
    {
        public delegate T CreateVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 textureCoordinate);

        /// <summary>
        /// Constructs a new sphere primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public SpherePrimitive(
            GraphicsDevice graphicsDevice,
            CreateVertex createVertex,
            float diameter = 1,
            int tessellation = 16,
            bool swap = true)
        {
            swap ^= true;

            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            var stackCount = tessellation;
            var sliceCount = tessellation*2;

            var radius = diameter/2;

            // Start with a single vertex at the bottom of the sphere.
            addVertex(createVertex(Vector3.Down*radius, Vector3.Down, Vector3.BackwardLH, new Vector2(0.5f, 1)));

            // Create rings of vertices at progressively higher latitudes.
            for (var i = 1; i < stackCount; i++)
            {
                var latitude = calculateLatitude(i, stackCount)*MathUtil.Pi - MathUtil.PiOverTwo;

                var dy = (float) Math.Sin(latitude);
                var dxz = (float) Math.Cos(latitude);
                // Create a single ring of vertices at this latitude.
                for (var j = 0; j <= sliceCount; j++)
                {
                    var longitude = j*MathUtil.TwoPi/sliceCount;
                    var dx = (float) Math.Cos(longitude)*dxz;
                    var dz = (float) Math.Sin(longitude)*dxz;
                    var normal = new Vector3(dx, dy, dz);
                    //var textureCoordinate = new Vector2(
                    //    0.5f + (float) Math.Atan2(dz, dx)/MathUtil.Pi,
                    //    txy);
                    var textureCoordinate = new Vector2(
                        j/(float) sliceCount,
                        1 - i/(float) stackCount);
                    var tangent = new Vector3(
                        -radius*(float) Math.Sin(longitude)*dxz,
                        0,
                        radius*(float) Math.Cos(longitude)*dxz);

                    addVertex(createVertex(normal*radius, normal, tangent, textureCoordinate));
                }
            }

            // Finish with a single vertex at the top of the sphere.
            addVertex(createVertex(Vector3.Up * radius, Vector3.Up, Vector3.ForwardLH, new Vector2(0.5f, 0)));

            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (var i = 1; i <= sliceCount; i++)
                addTriangle(0, i, i + 1, !swap);

            // Fill the sphere body with triangles joining each pair of latitude rings.
            var baseIndex = 1;
            var ringVertexCount = sliceCount + 1;
            for (var i = 0; i < stackCount - 2; i++)
                for (var j = 0; j < sliceCount; j++)
                {
                    addTriangle(
                        baseIndex + i * ringVertexCount+j,
                        baseIndex+i*ringVertexCount+j+1,
                        baseIndex+(i+1)*ringVertexCount+j,
                        swap);
                    addTriangle(
                        baseIndex + (i + 1)*ringVertexCount + j,
                        baseIndex + i*ringVertexCount + j + 1,
                        baseIndex + (i + 1)*ringVertexCount + j + 1,
                        swap);
                }

            // Create a fan connecting the top vertex to the top latitude ring.
            baseIndex = CurrentVertex - 1 - ringVertexCount;
            for (var i = 0; i < sliceCount; i++)
                addTriangle(CurrentVertex - 1, baseIndex + i, baseIndex + i + 1, swap);

            initializePrimitive(graphicsDevice);
        }

        // since the texture becomes distorted at the poles, I make the stacks at the poles much thinner
        private static float calculateLatitude(int i, int stackCount)
        {
            var dx = 0.25f/stackCount;
            if (i == stackCount - 1)
                return 1 - dx;
            return (i - 1)*(1 - 2*dx)/(stackCount - 2) + dx;
        }

    }

}
