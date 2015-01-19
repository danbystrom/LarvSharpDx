using System;
using System.Collections.Generic;
using Larv.Util;
using SharpDX;

namespace Larv.Field
{
    public class PlayingFieldBuilder
    {
        private readonly PlayingFieldSquare[,,] _field;
        private int _floor;

        public PlayingFieldBuilder(PlayingFieldSquare[,,] field)
        {
            _field = field;
        }

        public static PlayingFieldSquare[,,] Create(
            IList<string[]> fields,
            int width,
            int height,
            ref Whereabouts playerWhereaboutsStart,
            ref Whereabouts enemyWhereaboutsStart)
        {
            var result = new PlayingFieldSquare[fields.Count, height, width];
            var builder = new PlayingFieldBuilder(result);
            for (var i = 0; i < fields.Count; i++)
                builder.ConstructOneFloor(
                    i,
                    fields[i],
                    ref playerWhereaboutsStart,
                    ref enemyWhereaboutsStart);
            return result;
        }

        public void ConstructOneFloor(
            int floor,
            string[] field,
            ref Whereabouts playerWhereaboutsStart,
            ref Whereabouts enemyWhereaboutsStart)
        {
            _floor = floor;

            var height = field.Length;
            var width = field[0].Length;

            var expectedHeight = _field.GetUpperBound(1) + 1;
            var expectedWidth = _field.GetUpperBound(2) + 1;
            if (expectedHeight != height || expectedWidth != width)
                throw new Exception();

            while (true)
            {
                var mustRunAgain = false;
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        if (!_field[floor, y, x].IsNone)
                            continue;
                        switch (field[y][x])
                        {
                            case ' ':
                                _field[floor, y, x] = new PlayingFieldSquare();
                                break;

                            case 'A':
                                playerWhereaboutsStart = calcWhereabouts(field, 'a', floor, x, y);
                                goto case 'X';

                            case 'B':
                                enemyWhereaboutsStart = calcWhereabouts(field, 'b', floor, x, y);
                                goto case 'X';

                            case 'N':
                                _field[floor, y, x] = PlayingFieldSquare.CreateFlat(0, DirectionValue.North);
                                break;
                            case 'S':
                                _field[floor, y, x] = PlayingFieldSquare.CreateFlat(0, DirectionValue.South);
                                break;
                            case 'E':
                                _field[floor, y, x] = PlayingFieldSquare.CreateFlat(0, DirectionValue.East);
                                break;
                            case 'W':
                                _field[floor, y, x] = PlayingFieldSquare.CreateFlat(0, DirectionValue.West);
                                break;

                            case 'X':
                            case 'a':
                            case 'b':
                                _field[floor, y, x] = PlayingFieldSquare.CreateFlat(0);
                                break;
                            case 'D':
                                _field[floor, y, x] = createSlopeSquare(x, y, true);
                                mustRunAgain |= _field[floor, y, x].IsNone;
                                break;
                            case 'U':
                                _field[floor, y, x] = createSlopeSquare(x, y, false);
                                mustRunAgain |= _field[floor, y, x].IsNone;
                                break;
                        }
                    }
                if (!mustRunAgain)
                    return;
            }
        }

        private Whereabouts calcWhereabouts(string[] field, char token, int floor, int x, int y)
        {
            var direction = Direction.None;
            if (x > 0 && field[y][x - 1] == token)
                direction = Direction.West;
            else if (x < (field[0].Length - 1) && field[y][x + 1] == token)
                direction = Direction.East;
            else if (y > 0 && field[y - 1][x] == token)
                direction = Direction.South;
            else if (y < (field.Length - 1) && field[y + 1][x] == token)
                direction = Direction.North;
            return new Whereabouts(floor, new Point(x, y), direction);
        }

        private PlayingFieldSquare createSlopeSquare(int x, int y, bool doPortal)
        {
            foreach (var direction in Direction.AllDirections)
            {
                var square = getSquare(direction.DirectionAsPoint().Add(x, y));
                if (square.IsNone)
                    continue;
                if (doPortal)
                    return new PlayingFieldSquare(
                        PlayingFieldSquareType.Portal,
                        square.Elevation - 1,
                        direction,
                        Direction.None);
                return new PlayingFieldSquare(
                    PlayingFieldSquareType.Slope,
                    square.TopElevation,
                    direction.Opposite,
                    Direction.None);
            }
            return new PlayingFieldSquare();
        }

        private PlayingFieldSquare getSquare(Point p)
        {
            if (p.Y < 0 || p.Y > _field.GetUpperBound(1))
                return new PlayingFieldSquare();
            if (p.X < 0 || p.X > _field.GetUpperBound(2))
                return new PlayingFieldSquare();
            return _field[_floor, p.Y, p.X];
        }

    }

}

