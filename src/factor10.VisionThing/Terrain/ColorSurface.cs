using factor10.VisionThing.Terrain;
using SharpDX;

namespace factor10.VisionThing
{
    public class ColorSurface : Sculptable<Color>
    {

        public ColorSurface(int width, int height)
            : base(width, height)
        {
        }

        public ColorSurface(int width, int height, Color[] surface)
            : base(width,height,surface)
        {
        }

        public Vector3 AsVector3(int x, int y)
        {
            var c = Values[y*Width + x];
            return new Vector3(c.R/255f, c.G/255f, c.B/255f);
        }

        public Color GetExact(int x, int y, float fracx, float fracy)
        {
            var topHeight = Color.Lerp(
                this[x, y],
                this[x + 1, y],
                fracx);

            var bottomHeight = Color.Lerp(
                this[x, y + 1],
                this[x + 1, y + 1],
                fracx);

            return Color.Lerp(topHeight, bottomHeight, fracy);
        }

        public Color GetExact(float x, float y)
        {
            var ix = (int)x;
            var iy = (int)y;
            return GetExact(ix, iy, x - ix, y - iy);
        }

    }

}
