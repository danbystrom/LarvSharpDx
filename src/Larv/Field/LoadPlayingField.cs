using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Larv.Field
{
    public static class LoadPlayingField
    {
        public static IEnumerable<string> Load()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Larv.PlayingFields.txt"))
            using (var fs = new StreamReader(stream))
                while (!fs.EndOfStream)
                    yield return fs.ReadLine();
        }

    }
}
