using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Larv.Field;
using Larv.Util;
using NUnit.Framework;
using SharpDX;

namespace Larv.Tests
{
    [TestFixture]
    public class FloorTests
    {
        [Test]
        public void Test()
        {
            var z = LoadPlayingField.Load();
            var lc = new LarvContent(null, null, z);
            var pf = new PlayingField(lc, null, 4);
            var w = new Whereabouts(pf, 1, new Point(10, 12), Direction.East);
            var lastElevation = pf.GetElevation(w);
            for(var i=0;i<35;i++)
            {
                w.Fraction += 0.2f;
                if (w.Fraction > 0.999f)
                    w.Realign();
                System.Diagnostics.Debug.Print("{0} {1} {2:0.0} {3:0.00} {4:0.00} {5} {6} {7}", w.Location, w.Floor, w.Fraction, pf.GetElevation(w),
                    pf.GetElevation(w) - lastElevation,
                    pf.FieldValue(w.Floor - 1, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor + 1, w.Location).PlayingFieldSquareType);
                lastElevation = pf.GetElevation(w);
            }

            System.Diagnostics.Debug.Print("");
            w = new Whereabouts(pf, 1, new Point(12, 12), Direction.East);
            w.GoToNextLocation();
            System.Diagnostics.Debug.Print("{0} {1} {2:0.0} {3:0.00} {4} {5} {6}", w.Location, w.Floor, w.Fraction, pf.GetElevation(w),
                pf.FieldValue(w.Floor - 1, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor + 1, w.Location).PlayingFieldSquareType);
            w.Fraction = 0.2f;
            w.Realign();
            w.GoToNextLocation();
            System.Diagnostics.Debug.Print("{0} {1} {2:0.0} {3:0.00} {4} {5} {6}", w.Location, w.Floor, w.Fraction, pf.GetElevation(w),
                pf.FieldValue(w.Floor - 1, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor + 1, w.Location).PlayingFieldSquareType);
            w.Fraction += 0.2f;
            w.Realign();
            w.GoToNextLocation();
            System.Diagnostics.Debug.Print("{0} {1} {2:0.0} {3:0.00} {4} {5} {6}", w.Location, w.Floor, w.Fraction, pf.GetElevation(w),
                pf.FieldValue(w.Floor - 1, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor + 1, w.Location).PlayingFieldSquareType);

        }

        [Test]
        public void Test2()
        {
            var z = LoadPlayingField.Load();
            var lc = new LarvContent(null, null, z);
            var pf = new PlayingField(lc, null, 0);
            var w = new Whereabouts(pf, 0, new Point(24, 10), Direction.West);
            for (var i = 0; i < 35; i++)
            {
                w.Fraction += 0.2f;
                w.Realign();
                System.Diagnostics.Debug.Print("{7} {0} {1} {2:0.0} {3:0.00} {4} {5} {6}", w.Location, w.Floor, w.Fraction, pf.GetElevation(w),
                    pf.FieldValue(w.Floor - 1, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor, w.Location).PlayingFieldSquareType, pf.FieldValue(w.Floor + 1, w.Location).PlayingFieldSquareType,
                    i);
            }
        }

    }

}
