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
    public struct VertexPositionNormal : IEquatable<VertexPositionNormal>
    {
        public static readonly int Size = sizeof(float) * 3 * 2;

        [VertexElement("SV_Position")]
        public readonly Vector3 Position;

        [VertexElement("NORMAL")]
        public readonly Vector3 Normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        public bool Equals(VertexPositionNormal other)
        {
            return Position.Equals(other.Position) && Normal.Equals(other.Normal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VertexPositionNormal && Equals((VertexPositionNormal)obj);
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

        public static bool operator ==(VertexPositionNormal left, VertexPositionNormal right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexPositionNormal left, VertexPositionNormal right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Normal: {1}", Position, Normal);
        }

    }

}
