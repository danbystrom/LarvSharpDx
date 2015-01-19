using System;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Terrain
{
    public class Sculptable<T> where T : struct
    {
        public readonly int Width;
        public readonly int Height;

        public readonly T[] Values;

        public Sculptable(int width, int height)
        {
            Width = width;
            Height = height;
            Values = new T[Width*Height];
        }

        public Sculptable(int width, int height, T fillValue)
            : this(width, height)
        {
            for (var i = 0; i < Values.Length; i++)
                Values[i] = fillValue;
        }

        public Sculptable(Texture heightMap, Func<int, T> fx)
            : this(heightMap.Description.Width, heightMap.Description.Height)
        {
            var oldData = new Color[Width*Height];
            heightMap.GetData(oldData);

            for (var i = 0; i < Values.Length; i++)
                Values[i] = fx(oldData[i].R);
        }

        public Sculptable(int width, int height, T[] values)
        {
            if (width*height != values.Length)
                throw new Exception();
            Width = width;
            Height = height;
            Values = values;
        }

        public T this[int x, int y]
        {
            get { return Values[y*Width + x]; }
            set { Values[y*Width + x] = value; }
        }

        public void AlterValues(Func<T, T> func)
        {
            for (var i = 0; i < Values.Length; i++)
                Values[i] = func(Values[i]);
        }

        public void AlterValues(int x, int y, int w, int h, Func<int, int, T, T> func)
        {
            if (x < 0 || y < 0 || (x + w) > Width || (y + h) > Height)
            {
                //TODO throw new Exception();
                return;
            }
            for (var j = 0; j < h; j++)
            {
                var p = (y + j)*Width + x;
                for (var i = 0; i < w; i++)
                    Values[p + i] = func(i, j, Values[p + i]);
            }
        }

        public virtual Texture2D CreateTexture2D(GraphicsDevice graphicsDevice)
        {
            if (typeof (T) == typeof (Color))
                return Texture2D.New(graphicsDevice, Width, Height, PixelFormat.B8G8R8A8.UNorm, Values);
            if (typeof (T) == typeof (float))
                return Texture2D.New(graphicsDevice, Width, Height, PixelFormat.R32.Float, Values);
            throw new Exception();
        }

        public void DrawLine(int x1, int y1, int x2, int y2, int width, Func<T, int, T> fx, int winding = 0, Random rnd = null)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            if (dx == 0 && dy == 0)
                return;

            if (winding != 0 && winding*20 > Math.Sqrt(dx*dx + dy*dy))
            {
                rnd = rnd ?? new Random();
                var mx = (x1 + x2)/2 + (int) (winding*(rnd.NextDouble() - 0.5));
                var my = (y1 + y2)/2 + (int) (winding*(rnd.NextDouble() - 0.5));
                DrawLine(x1, y1, mx, my, width, fx, winding/2, rnd);
                DrawLine(mx, my, x2, y2, width, fx, winding/2, rnd);
                return;
            }

            if (Math.Abs(dx) > Math.Abs(dy))
                drawMostlyHorizontalLine(x1, y1 - width/2, dx, dy/(float) dx, width, fx);
            else
                drawMostlyVerticalLine(x1 - width/2, y1, dy, dx/(float) dy, width, fx);
        }

        private void drawMostlyHorizontalLine(int x, float y, int len, float d, int width, Func<T, int, T> fx)
        {
            var wh = width/2;
            if (len < 0)
                drawMostlyHorizontalLine(x + len, y + len*d, -len, d, width, fx);
            for (var i = 0; i < len; i++)
            {
                var p = (int) y*Width + x;
                for (var j = 0; j < width; j++)
                {
                    Values[p] = fx(Values[p], Math.Abs(j - wh));
                    p += Width;
                }
                y += d;
                x++;
            }
        }

        private void drawMostlyVerticalLine(float x, int y, int len, float d, int width, Func<T, int, T> fx)
        {
            var wh = width/2;
            if (len < 0)
                drawMostlyVerticalLine(x + len*d, y + len, -len, d, width, fx);
            else
                for (var i = 0; i < len; i++)
                {
                    var p = y*Width + (int) x;
                    for (var j = 0; j < width; j++)
                    {
                        Values[p] = fx(Values[p], Math.Abs(j - wh));
                        p++;
                    }
                    x += d;
                    y++;
                }
        }

    }

}
