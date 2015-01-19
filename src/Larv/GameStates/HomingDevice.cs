using Larv.Field;
using Larv.Serpent;
using Larv.Util;

namespace Larv.GameStates
{
    internal class HomingDevice : ITakeDirection
    {
        public PathFinder PlayerPathFinder { get; private set; }
        public PathFinder EnemyPathFinder { get; private set; }
        public bool CanTurnAround;

        public static HomingDevice Attach(Serpents serpents, bool player = true, bool enemies = true)
        {
            var hd = new HomingDevice(serpents);
            if (player)
                serpents.PlayerSerpent.DirectionTaker = hd;
            if (enemies)
                serpents.Enemies.ForEach(_ => _.DirectionTaker = hd);
            return hd;
        }

        public HomingDevice(Serpents serpents)
        {
            PlayerPathFinder = new PathFinder(serpents.PlayingField, serpents.PlayingField.PlayerWhereaboutsStart);
            EnemyPathFinder = new PathFinder(serpents.PlayingField, serpents.PlayingField.EnemyWhereaboutsStart);
        }

        RelativeDirection ITakeDirection.TakeDelayedDirection(BaseSerpent serpent)
        {
            return RelativeDirection.Forward;
        }

        public RelativeDirection TakeDirection(BaseSerpent serpent)
        {
            var pathFinder = serpent is PlayerSerpent ? PlayerPathFinder : EnemyPathFinder;
            var whereabouts = serpent.Whereabouts;
            whereabouts.Direction = serpent.HeadDirection;
            var direction = pathFinder.WayHome(whereabouts, CanTurnAround);
            return serpent.HeadDirection.GetRelativeDirection(direction);
        }

        public bool CanOverrideRestrictedDirections(BaseSerpent serpent)
        {
            return true;
        }

    }

}
