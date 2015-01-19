using Larv.Util;

namespace Larv.Field
{
    public enum PlayingFieldSquareType
    {
        None,
        Flat,
        Slope,
        Portal
    }

    public struct PlayingFieldSquare
    {
        public readonly PlayingFieldSquareType PlayingFieldSquareType;
        public readonly int Elevation;
        public readonly Direction SlopeDirection;
        public readonly Direction Restricted;

        public PlayingFieldSquare(
            PlayingFieldSquareType playingFieldSquareType,
            int elevation,
            Direction slopeDirection,
            Direction restrictedDirection)
        {
            PlayingFieldSquareType = playingFieldSquareType;
            Elevation = elevation;
            SlopeDirection = slopeDirection;
            Restricted = restrictedDirection;
        }

        public static PlayingFieldSquare CreateFlat(int elevation, DirectionValue restrict = DirectionValue.None)
        {
            return new PlayingFieldSquare(
                PlayingFieldSquareType.Flat,
                elevation,
                Direction.None,
                restrict);
        }

        public int[] Corners
        {
            get
            {
                switch (PlayingFieldSquareType)
                {
                    case PlayingFieldSquareType.Flat:
                        return new[] {Elevation, Elevation, Elevation, Elevation};
                    case PlayingFieldSquareType.Slope:
                    case PlayingFieldSquareType.Portal:
                        if (SlopeDirection == Direction.East)
                            return new[] {Elevation, Elevation, Elevation + 1, Elevation + 1};
                        if (SlopeDirection == Direction.West)
                            return new[] {Elevation + 1, Elevation + 1, Elevation, Elevation};
                        if (SlopeDirection == Direction.North)
                            return new[] {Elevation + 1, Elevation, Elevation + 1, Elevation};
                        if (SlopeDirection == Direction.South)
                            return new[] {Elevation, Elevation + 1, Elevation, Elevation + 1};
                        return null;
                    default:
                        return null;
                }
            }
        }

        public bool IsNone
        {
            get { return PlayingFieldSquareType == PlayingFieldSquareType.None; }
        }

        public bool IsPortal
        {
            get { return PlayingFieldSquareType == PlayingFieldSquareType.Portal; }
        }

        public bool IsSlope
        {
            get { return PlayingFieldSquareType == PlayingFieldSquareType.Slope; }
        }

        public bool IsFlat
        {
            get { return PlayingFieldSquareType == PlayingFieldSquareType.Flat; }
        }

        public int TopElevation
        {
            get
            {
                if ( PlayingFieldSquareType == PlayingFieldSquareType.Slope || PlayingFieldSquareType == PlayingFieldSquareType.Portal )
                    return Elevation + 1;
                return Elevation;
            }
        }

    }

}
