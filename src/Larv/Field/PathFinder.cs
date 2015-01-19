using Larv.Util;
using SharpDX;

namespace Larv.Field
{
    public class PathFinder
    {
        public readonly int[,,] Distance;
        public PlayingField PlayingField;

        public PathFinder(PlayingField pf, Whereabouts home)
        {
            PlayingField = pf;
            Distance = new int[pf.TheField.GetUpperBound(0)+1, pf.TheField.GetUpperBound(1)+1, pf.TheField.GetUpperBound(2)+1];
            Explore(home.Floor, home.Location, 1, Direction.None);
        }

        public void Explore(int floor, Point fromLoc, int distance, Direction direction)
        {
            var toLoc = fromLoc.Add(direction.DirectionAsPoint());
            if (!PlayingField.CanMoveHere(ref floor, fromLoc, toLoc, true))
                return;

            var here = Distance[floor, toLoc.Y, toLoc.X];
            if (here != 0 && here < distance)
                return; // already know shorter path

            Distance[floor, toLoc.Y, toLoc.X] = distance;

            foreach (var newDirection in Direction.AllDirections)
                Explore(floor, toLoc, distance + 1, newDirection);
        }

        private int getDistance(int floor, Point fromLoc, Direction direction)
        {
            var toLoc = fromLoc.Add(direction.DirectionAsPoint());
            return PlayingField.CanMoveHere(ref floor, fromLoc, toLoc, true)
                ? Distance[floor, toLoc.Y, toLoc.X]
                : int.MaxValue;
        }

        private void testDistance(int floor, Point fromLoc, ref Direction bestDirection, ref int bestDistance, Direction direction)
        {
            var here = getDistance(floor, fromLoc, direction);
            if (here >= bestDistance)
                return;
            bestDirection = direction;
            bestDistance = here;
        }

        public int GetDistance(Whereabouts whereabouts)
        {
            return Distance[whereabouts.Floor, whereabouts.Location.Y, whereabouts.Location.X];
        }

        public Direction WayHome(Whereabouts whereabouts, bool canTurnAround)
        {
            if (getDistance(whereabouts.Floor, whereabouts.Location, Direction.None) <= 2)
                return Direction.None; // is home!

            var backward = whereabouts.Direction.Turn(RelativeDirection.Backward);
            var bestDirection = backward;
            var bestDistance = int.MaxValue;
            foreach (var direction in Direction.AllDirections)
                if (canTurnAround || direction != backward)
                    testDistance(whereabouts.Floor, whereabouts.Location, ref bestDirection, ref bestDistance, direction);
            return bestDirection;
        }

    }

}


