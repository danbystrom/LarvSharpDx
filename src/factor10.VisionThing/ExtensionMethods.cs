using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SharpDX;
using Point = System.Drawing.Point;

namespace factor10.VisionThing
{
    public static class ExtensionMethods
    {
        public static string ToIsoString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm");
        }

        public static List<T> AsList<T>(this T item)
        {
            return new List<T> { item };
        }

        public static T[] AsArray<T>(this T item)
        {
            return new[] { item };
        }

        public static bool IsVoid(this string x)
        {
            return string.IsNullOrEmpty(x);
        }

        public static string Fmt(this string x, params object[] p)
        {
            return string.Format(x, p);
        }

        /// <summary>
        /// Get a value out of a dictionary without risking an exception - get default instead
        /// </summary>
        public static TValue Value<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key)
        {
            if (!typeof(TKey).IsValueType)
                if (Equals(key, default(TKey)))
                    return default(TValue);
            TValue value;
            d.TryGetValue(key, out value);
            return value;
        }

        public delegate TValue CreateNewItem<out TValue>();

        public static TValue ValueOrAddNew<TKey, TValue>(
            this Dictionary<TKey, TValue> d,
            TKey key,
            CreateNewItem<TValue> createNewItem)
        {
            TValue value;
            if (!d.TryGetValue(key, out value))
                d.Add(key, value = createNewItem());
            return value;
        }

        public static TValue GetOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key) where TValue : new()
        {
            return dic.GetOrAddDefault(key, _ => new TValue());
        }

        public static TValue GetOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> newValue)
        {
            TValue value;
            if (dic.TryGetValue(key, out value))
                return value;
            value = newValue(key);
            dic.Add(key, value);
            return value;
        }

        public static float Distance(this Point ptThis, Point pnt)
        {
            return (float)Math.Sqrt(ptThis.DistanceSquared(pnt));
        }

        public static float DistanceSquared(this Point ptThis, Point pnt)
        {
            float dx = pnt.X - ptThis.X;
            float dy = pnt.Y - ptThis.Y;
            return dx * dx + dy * dy;
        }

        public static float Distance(this IPosition ptThis, IPosition pnt)
        {
            return Vector3.Distance(ptThis.Position, pnt.Position);
        }

        public static float DistanceSquared(this IPosition ptThis, IPosition pnt)
        {
            return Vector3.Distance(ptThis.Position, pnt.Position);
        }

        public static float DistanceToLine(this Point ptThis, Point lineStart, Point lineEnd)
        {
            return (float)Math.Sqrt(ptThis.DistanceToLineSquared(lineStart, lineEnd));
        }

        public static float DistanceToLineSquared(this Point ptThis, Point lineStart, Point lineEnd)
        {
            float A = ptThis.X - lineStart.X;
            float B = ptThis.Y - lineStart.Y;
            float C = lineEnd.X - lineStart.X;
            float D = lineEnd.Y - lineStart.Y;
            var dot = (A * C) + (B * D);
            var lengthSquared = C * C + D * D;
            var param = dot / lengthSquared;

            Point ptOnLine;
            if (param <= 0f)
                ptOnLine = lineStart;
            else if (param >= 1f)
                ptOnLine = lineEnd;
            else
                ptOnLine = new Point(lineStart.X + ((int)(param * C)), lineStart.Y + ((int)(param * D)));
            return ptThis.DistanceSquared(ptOnLine);
        }

        public static IEnumerable<T> SafeConcat<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1 == null)
                return list2;
            return list2 != null
                       ? list1.Concat(list2)
                       : list1;
        }

        public static IEnumerable<T> SafeConcat<T>(this IEnumerable<T> list, T element)
        {
            return SafeConcat(list, element.AsList());
        }

        public static string ToXml<T>(this T obj)
        {
            var sw = new StringWriter();
            new XmlSerializer(typeof(T)).Serialize(sw, obj);
            return sw.ToString();
        }

        public static T FromXml<T>(this string xml)
        {
            return (T)new XmlSerializer(typeof(T)).Deserialize(new StringReader(xml));
        }

        public static string Left(this object obj, int maxLength = 50)
        {
            if (obj == null)
                return "";
            var s = obj.ToString();
            if (s.Length <= maxLength)
                return s;
            return s.Substring(0, maxLength - 3) + "...";
        }

    }

}
