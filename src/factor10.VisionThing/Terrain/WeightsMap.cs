using SharpDX;
using System;
using System.Linq;

namespace factor10.VisionThing.Terrain
{
    public class WeightsMap : Mt9Surface
    {

        public WeightsMap(int width, int height)
            : base(width, height)
        {
        }

        public WeightsMap(Sculptable<float> ground, float[] levels = null)
            : base(ground.Width, ground.Height)
        {
            if (levels == null)
                levels = new[] {0, 0.33f, 0.76f, 1};

            if(levels.Length!=4)
                throw new Exception();

            var maxHeight = ground.Values.Max();
            levels = levels.Select(_ => _*maxHeight).ToArray();

            //var test = Enumerable.Range(0, (int) maxHeight + 1).Select(val =>
            //{
            //    var t = levels.Select(_ => MathUtil.Clamp(1.0f - Math.Abs(val - _) / maxHeight, 0, 1)).ToArray();
            //    var maxT = t.Max() + 0.0000001;
            //    t = t.Select(_ => (float)Math.Pow(_ / maxT, 10)).ToArray();
            //    var tot = 1 / (t.Sum() + 0.0000001f);
            //    return t.Select(_ => _*tot).ToArray();
            //}).ToArray();

            for (var i = 0; i < ground.Values.Length; i++)
            {
                var val = ground.Values[i];
                var t = levels.Select(_ => MathUtil.Clamp(1.0f - Math.Abs(val - _)/maxHeight, 0, 1)).ToArray();
                var maxT = t.Max() + 0.0000001;
                t = t.Select(_ => (float)Math.Pow(_ / maxT, 10)).ToArray();
                var tot = 1 / (t.Sum() + 0.0000001f);
                Values[i].B = t[0] * tot;
                Values[i].C = t[1] * tot;
                Values[i].D = t[2] * tot;
                Values[i].E = t[3] * tot;
            }
        }

    }

}
