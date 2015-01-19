using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using factor10.VisionThing;
using Larv.Util;

namespace Larv.Field
{
    public class PlayingFieldInfo
    {
        public PlayingFieldSquare[,,] PlayingField;
        public Whereabouts PlayerSerpentStart;
        public Whereabouts EnemySerpentStart;

        public int Floors { get { return PlayingField.GetUpperBound(0) + 1; } }
        public int Height { get { return PlayingField.GetUpperBound(1) + 1; } }
        public int WIdth { get { return PlayingField.GetUpperBound(2) + 1; } }
    }

    public class PlayingFieldsDecoder
    {
        public static PlayingFieldInfo[] Create(IEnumerable<string> description)
        {
            var playingFields = new List<PlayingFieldInfo>();
           var cleaned = description.Select(_ => _.Trim()).Where(_ => _.Length > 0).ToArray();
            for (var i = 0; i < cleaned.Length; )
                decodeLevel(ref i, playingFields, cleaned);
            return playingFields.ToArray();
        }

        private static void decodeLevel(ref int i, List<PlayingFieldInfo> playingFields, string[] description)
        {
            var rex = new Regex(@"^\*SCENE(\d+),(\d+)x(\d+)x(\d+)\*$");
            var split = rex.Split(description[i]);
            if (split.Length != 6)
                throw new Exception();

            var scene = int.Parse(split[1]);
            var height = int.Parse(split[2]);
            var width = int.Parse(split[3]);
            var floors = int.Parse(split[4]);

            if (scene - 1 != playingFields.Count)
                throw new Exception();

            var fields = new List<string[]>();
            for (var f = 0; f < floors; f++)
            {
                if (f != 0)
                {
                    var expected = "*Floor{0}*".Fmt(f + 1);
                    if (string.Compare(description[i], expected, StringComparison.OrdinalIgnoreCase) != 0)
                        throw new Exception("Expected \"{0}\" but was \"{1}\"".Fmt(expected, description[i]));
                }
                i++;
                fields.Add(getFloor(i, description, width, height));
                i += height;
            }

            var pfi = new PlayingFieldInfo();
            pfi.PlayingField = PlayingFieldBuilder.Create(fields, width, height, ref pfi.PlayerSerpentStart, ref pfi.EnemySerpentStart);
            playingFields.Add(pfi);
        }

        private static string[] getFloor(int i, string[] description, int width, int height)
        {
            var result = new string[height];
            for (var j = 0; j < height; j++)
            {
                var row = description[i+j];
                if (row[0] != '\"' || row.Last() != '\"' || row.Length != width + 2)
                    throw new Exception("Malformed playingfield row: " + row);
                result[j] = row.Substring(1, width);
            }
            return result;
        }
    }

}
