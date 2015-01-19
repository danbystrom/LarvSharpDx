using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Terrain
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CxBillboardVertex : IEquatable<CxBillboardVertex>
    {
        [VertexElement("SV_Position")]
        public readonly Vector3 Position;

        [VertexElement("NORMAL0")]
        public readonly Vector3 Normal;

        [VertexElement("TEXCOORD0")]
        public readonly Vector2 Random;

        public CxBillboardVertex(Vector3 position, Vector3 normal, float random)
        {
            normal.Normalize();
            Position = position;
            Normal = normal;
            Random = new Vector2(random, 0);
        }

        public bool Equals(CxBillboardVertex other)
        {
            return Position.Equals(other.Position) && Normal.Equals(other.Normal) && Random.Equals(other.Random);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CxBillboardVertex && Equals((CxBillboardVertex) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode*397) ^ Normal.GetHashCode();
                hashCode = (hashCode*397) ^ Random.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(CxBillboardVertex left, CxBillboardVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CxBillboardVertex left, CxBillboardVertex right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Normal: {1}, Random: {2}", Position, Normal, Random);
        }

    }

}
