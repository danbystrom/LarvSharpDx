using System;
using Larv.Field;
using SharpDX;

namespace Larv.Util
{
    public struct Whereabouts
    {
        public int Floor;
        private Point _location;
        public Direction Direction;
        public float Fraction;

        public PlayingField PlayingField;

        public Whereabouts(PlayingField pf, int floor, Point location, Direction direction )
        {
            PlayingField = pf;
            _location = location;
            Direction = direction;
            Fraction = 0;
            Floor = floor;
        }

        public Point Location
        {
            get { return _location; }
            set
            {
                if(PlayingField.FieldValue(Floor,value).IsNone)
                    throw new Exception();
                _location = value;
            }
        }

        public void GoToNextLocation()
        {
            if(!goToNextLocation(NextLocation))
                throw new Exception();
        }

        public bool GoToNextLocationSafe()
        {
            return goToNextLocation(NextLocation);
        }

        private bool goToNextLocation(Point nextLocation)
        {
            var thisSquare = PlayingField.FieldValue(Floor, Location);
            var nextSquare = PlayingField.FieldValue(Floor, nextLocation);
            if (nextSquare.IsNone)
            {
                // floor transition?
                if (thisSquare.IsSlope && PlayingField.FieldValue(Floor + 1, nextLocation).IsPortal)
                    Floor++;
                else if (thisSquare.IsPortal && PlayingField.FieldValue(Floor - 1, nextLocation).IsSlope)
                    Floor--;
                else
                    return false;
            }
            _location = nextLocation;
            return true;
        }

        public Point NextLocation
        {
            get { return _location.Add(Direction.DirectionAsPoint());  }    
        }

        public Vector3 GetPosition()
        {
            var d = Direction.DirectionAsPoint();
            return new Vector3(
                _location.X + d.X * Fraction,
                PlayingField.GetElevation(this),
                _location.Y + d.Y * Fraction);
        }

        public int LocationDistanceSquared(Whereabouts w)
        {
            var x = _location.X - w._location.X;
            var y = _location.Y - w._location.Y;
            return x*x + y*y;
        }

        public void Realign()
        {
            for (; Fraction > 0.999; Fraction -= 1)
                GoToNextLocation();
            for (; Fraction < -0.001; Fraction += 1)
                goToNextLocation(_location.Add(Direction.Opposite.DirectionAsPoint()));
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2:0.0000},{3}", _location.X, _location.Y, Fraction, Direction);
        }

    }

}
