using System;
using System.Linq;

namespace Larv.Hof
{
    public class HallOfFame
    {
        public class Entry
        {
            public string Name;
            public int Score;
            public DateTime When;

            public Entry()
            {
            }

            public Entry(string name, int score, DateTime? when = null)
            {
                Name = name;
                Score = score;
                When = when.GetValueOrDefault(DateTime.Now);
            }
        }

        public Entry[] Entries =
        {
            new Entry("LARV! by Dan Byström", 10000),
            new Entry("", 5000),
            new Entry("Play with [Left], [Right] and [Down]", 4500),
            new Entry("", 4000),
            new Entry("LARV! is Open Source Software, press", 3500),
            new Entry("[F1] to visit web site and learn more.", 3000),
            new Entry("", 2500),
            new Entry("LARV! is a remake of the classic game", 2000),
            new Entry("Serpentine from 1982 by David Snider,", 1500),
            new Entry("published by Brøderbund.", 1000)
        };

        public bool HasMadeIt(int score)
        {
            return score > Entries.Last().Score;
        }

        public int Insert(Entry entry)
        {
            if (!HasMadeIt(entry.Score))
                return -1;
            var index = Entries.Length - 1;
            for (; index >= 1 && Entries[index - 1].Score < entry.Score; index--)
                Entries[index ] = Entries[index - 1];
            Entries[index] = entry;
            return index;
        }

    }

}
