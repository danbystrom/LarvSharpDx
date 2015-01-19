using System;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing cubes.
    /// </summary>
    public class CubePrimitive<T> : GeometricPrimitive<T> where T: struct , IEquatable<T>
    {
        public delegate T CreateVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate);

        /// <summary>
        /// Constructs a new cube primitive, with the specified size.
        /// </summary>
        public CubePrimitive(GraphicsDevice graphicsDevice, CreateVertex createVertex, float size, bool swap = false)
        {
            // A cube has six faces, each one pointing in a different direction.
            Vector3[] normals =
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
            };

            // Create each face in turn.
            foreach (var normal in normals)
            {
                // Get two vectors perpendicular to the face normal and to each other.
                var side1 = new Vector3(normal.Y, normal.Z, normal.X);
                var side2 = Vector3.Cross(normal, side1);

                // Two triangles per face.
                addTriangle(CurrentVertex + 0, CurrentVertex + 1, CurrentVertex + 2, swap);
                addTriangle(CurrentVertex + 0, CurrentVertex + 2, CurrentVertex + 3, swap);

                // Four vertices per face.
                addVertex(createVertex((normal - side1 - side2)*size/2, normal, new Vector2(0, 0)));
                addVertex(createVertex((normal - side1 + side2)*size/2, normal, new Vector2(0, 1)));
                addVertex(createVertex((normal + side1 + side2)*size/2, normal, new Vector2(1, 1)));
                addVertex(createVertex((normal + side1 - side2)*size/2, normal, new Vector2(1, 0)));
            }

            initializePrimitive(graphicsDevice);
        }

    }

}
