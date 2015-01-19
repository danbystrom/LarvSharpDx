using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Terrain
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TerrainVertex : IEquatable<TerrainVertex>
    {
        public static readonly int Size = sizeof (float)*(3 + 2);

        [VertexElement("SV_Position")]
        public readonly Vector3 Position;

        [VertexElement("TEXCOORD0")]
        public readonly Vector2 TexCoord;

        //[VertexElement("BLENDWEIGHT")]
        //public readonly float NormCoordX;


        public TerrainVertex(Vector3 position, Vector2 texCoord, float normCoordX)
        {
            Position = position;
            TexCoord = texCoord;
            //NormCoordX = normCoordX;
        }

        public bool Equals(TerrainVertex other)
        {
            return Position.Equals(other.Position) && TexCoord.Equals(other.TexCoord);
            //&& NormCoordX.Equals(other.NormCoordX);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ArcVertex && Equals((ArcVertex) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode*397) ^ TexCoord.GetHashCode();
//                hashCode = (hashCode*397) ^ NormCoordX.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(TerrainVertex left, TerrainVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TerrainVertex left, TerrainVertex right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, TexCoord: {1}, NormCoordX: {2}", Position, TexCoord, 0);
        }
    }

}
