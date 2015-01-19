using factor10.VisionThing;
using SharpDX;

namespace Larv.Hof
{
    public class HofPainter
    {
        public readonly LarvContent LContent;

        public HofPainter(LarvContent lcontent)
        {
            LContent = lcontent;
        }

        public void Paint(Color color, int highlightIndex = 0, bool cursor = false)
        {
            float fsize = 1.4f;
            var w = LContent.GraphicsDevice.BackBuffer.Width;
            var u = w/60;
            LContent.DrawString("Hall of Fame", new Vector2(w*0.5f, u*2), fsize*2, 0.5f, color);

            for (var i = 0; i < 10; i++)
            {
                var entry = LContent.HallOfFame.Entries[i];
                var y = u*(7 + i*2);
                var sz = fsize;
                if (i == 0)
                {
                    y -= u/2;
                    sz *= 1.2f;
                }
                var name = entry.Name;
                if (i==highlightIndex && cursor)
                    name += "_";
                LContent.DrawString("{0}.".Fmt(i + 1), new Vector2(u*5, y), sz, 1, color);
                LContent.DrawString("{0:0}".Fmt(entry.Score), new Vector2(u*14, y), sz, 1, color);
                LContent.DrawString(name, new Vector2(u*17, y), sz, 0, color);
                LContent.DrawString(entry.When.ToShortDateString(), new Vector2(u*57, y), sz, 1, color);
            }

        }

    }

}
