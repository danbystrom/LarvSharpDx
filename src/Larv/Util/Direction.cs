using SharpDX;

namespace Larv.Util
{
    public enum RelativeDirection
    {
        None,
        Left,
        Backward,
        Right,
        Forward
    }

    public enum DirectionValue
    {
        None = 0,
        South = 1,
        West = 2,
        North = 3,
        East = 4
    }

    public struct Direction
    {
        public static readonly Direction[] AllDirections;

        public static readonly Direction None;
        public static readonly Direction South;
        public static readonly Direction West;
        public static readonly Direction North;
        public static readonly Direction East;

        private readonly DirectionValue _dir;

        static Direction()
        {
            None = new Direction(DirectionValue.None);
            South = new Direction(DirectionValue.South);
            West = new Direction(DirectionValue.West);
            North = new Direction(DirectionValue.North);
            East = new Direction(DirectionValue.East);
            AllDirections = new[] {South, West, North, East};
        }

        private Direction(DirectionValue dir)
        {
            _dir = dir;
        }

        public static implicit operator Direction(DirectionValue dir)
        {
            return new Direction(dir);
        }

        public static implicit operator DirectionValue(Direction direction)
        {
            return direction._dir;
        }

        public Direction Turn(RelativeDirection rd)
        {
            if (_dir == 0 || rd == RelativeDirection.None)
                return None;
            var dir = 1 + (((int) _dir + (int) rd - 1) & 3);
            return new Direction((DirectionValue) dir);
        }

        public Direction Right
        {
            get { return Turn(RelativeDirection.Right); }
        }

        public Direction Left
        {
            get { return Turn(RelativeDirection.Left); }
        }

        public Direction Opposite
        {
            get { return Turn(RelativeDirection.Backward); }
        }

        public bool IsNorthSouth
        {
            get { return _dir == DirectionValue.North || _dir == DirectionValue.South; }    
        }

        public bool IsEastWest
        {
            get { return _dir == DirectionValue.East || _dir == DirectionValue.West; }
        }

        public Point DirectionAsPoint()
        {
            return new[]
            {
                new Point(0, 0),
                new Point(0, -1),
                new Point(-1, 0),
                new Point(0, 1),
                new Point(1, 0)
            }[(int) _dir];
        }

        public Vector2 DirectionAsVector2()
        {
            var p = DirectionAsPoint();
            return new Vector2(p.X, p.Y);
        }

        public Vector3 DirectionAsVector3()
        {
            var p = DirectionAsPoint();
            return new Vector3(p.X, 0, p.Y);
        }

        public RelativeDirection GetRelativeDirection(Direction goingTo)
        {
            if(goingTo==None)
                return RelativeDirection.None;
            if (this == goingTo)
                return RelativeDirection.Forward;
            if (Turn(RelativeDirection.Left) == goingTo)
                return RelativeDirection.Left;
            if (Turn(RelativeDirection.Right) == goingTo)
                return RelativeDirection.Right;
            return RelativeDirection.Backward;
        }

        public static bool operator ==(Direction d1, Direction d2)
        {
            return d1._dir == d2._dir;
        }

        public static bool operator !=(Direction d1, Direction d2)
        {
            return d1._dir != d2._dir;
        }

        public override bool Equals(object obj)
        {
            return (obj is Direction) && ((Direction) obj)._dir == _dir;
        }

        public override int GetHashCode()
        {
            return (int) _dir;
        }

        public override string ToString()
        {
            return new[] {"None", "South", "West", "North", "East"}[(int) _dir];
        }

        public static Direction FromPoints(Point currentLocation, Point newLocation)
        {
            if (currentLocation == newLocation)
                return None;
            if (currentLocation.X == newLocation.X)
                return currentLocation.Y < newLocation.Y ? North : South;
            if (currentLocation.Y == newLocation.Y)
                return currentLocation.X < newLocation.X ? East : West;
            return None;
        }
    }

}
