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
    public struct VertexPosition : IEquatable<VertexPosition>
    {
        public static readonly int Size = sizeof(float) * 3 * 1;

        [VertexElement("SV_Position")]
        public readonly Vector3 Position;

        public VertexPosition(Vector3 position)
        {
            Position = position;
        }

        public bool Equals(VertexPosition other)
        {
            return Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VertexPosition && Equals((VertexPosition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(VertexPosition left, VertexPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexPosition left, VertexPosition right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}", Position);
        }

    }

}
