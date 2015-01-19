using System;
using System.IO;
using System.IO.IsolatedStorage;
using factor10.VisionThing;

namespace Larv.Hof
{
    public class HofStorage
    {
        private const string Filename = "HallOfFame.xml";

        private HofStorage()
        {
        }

        public static void Save(HallOfFame hof)
        {
            withStorage(
                file =>
                {
                    using (var writer = new StreamWriter(file))
                    {
                        var content = hof.ToXml();
                        writer.WriteLine(content);
                        return 0;
                    }
                });
        }

        public static HallOfFame Load()
        {
            return withStorage(
                file =>
                {
                    try
                    {
                        using (var reader = new StreamReader(file))
                        {
                            var content = reader.ReadToEnd();
                            return content.FromXml<HallOfFame>();
                        }
                    }
                    catch
                    {
                        return new HallOfFame();
                    }
                });
        }

        private static T withStorage<T>(Func<IsolatedStorageFileStream, T> action)
        {
            using (
                var storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            using (var file = storage.OpenFile(Filename, FileMode.OpenOrCreate))
                return action(file);
        }

    }

}
