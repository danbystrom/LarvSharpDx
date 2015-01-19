using Larv.Field;
using SharpDX;

namespace Larv.Util
{
    public struct Whereabouts
    {
        public int Floor;
        public Point Location;
        public Direction Direction;
        public float Fraction;

        public Whereabouts( int floor, Point location, Direction direction )
        {
            Floor = floor;
            Location = location;
            Direction = direction;
            Fraction = 0;
        }

        public Point NextLocation
        {
            get { return Location.Add(Direction.DirectionAsPoint());  }    
        }

        public Vector3 GetPosition(PlayingField pf)
        {
            var d = Direction.DirectionAsPoint();
            return new Vector3(
                Location.X + d.X * Fraction,
                pf.GetElevation(this),
                Location.Y + d.Y * Fraction);
        }

        public int LocationDistanceSquared(Whereabouts w)
        {
            var x = Location.X - w.Location.X;
            var y = Location.Y - w.Location.Y;
            return x*x + y*y;
        }

        public void Realign()
        {
            for (; Fraction > 1; Fraction -= 1)
                Location = NextLocation;
            for (; Fraction < 0; Fraction += 1)
                Location = Location.Add(Direction.Opposite.DirectionAsPoint());
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2:0.0000},{3}", Location.X, Location.Y, Fraction, Direction);
        }

    }

}
