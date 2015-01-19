using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Primitives
{
    /// <summary>
    /// Custom vertex type for vertices that have just a
    /// position and a normal, without any texture coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTangentTexture : IEquatable<VertexPositionNormalTangentTexture>
    {
        public static readonly int Size = sizeof(float) * 3 * 2;

        [VertexElement("SV_Position")]
        public readonly Vector3 Position;
        [VertexElement("NORMAL")]
        public readonly Vector3 Normal;
        [VertexElement("TANGENT")]
        public readonly Vector3 Tangent;
        [VertexElement("TEXCOORD0")]
        public Vector2 TextureCoordinate;

        public VertexPositionNormalTangentTexture(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
            Tangent = tangent;
            TextureCoordinate = textureCoordinate;
        }

        public bool Equals(VertexPositionNormalTangentTexture other)
        {
            return Position.Equals(other.Position) && Normal.Equals(other.Normal) && Tangent.Equals(other.Tangent) && TextureCoordinate.Equals(other.TextureCoordinate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is VertexPositionNormalTangentTexture && Equals((VertexPositionNormalTangentTexture)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(VertexPositionNormalTangentTexture left, VertexPositionNormalTangentTexture right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexPositionNormalTangentTexture left, VertexPositionNormalTangentTexture right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Normal: {1}", Position, Normal);
        }

    }

}
