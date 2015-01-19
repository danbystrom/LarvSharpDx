using System;
using System.Collections.Generic;
using System.Linq;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using Larv.Field;
using Larv.Util;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace Larv.Serpent
{
    public enum SerpentStatus
    {
        Alive,
        Ghost,
        Finished
    }

    public interface ITakeDirection
    {
        RelativeDirection TakeDirection(BaseSerpent serpent);
        RelativeDirection TakeDelayedDirection(BaseSerpent serpent);
        bool CanOverrideRestrictedDirections(BaseSerpent serpent);
    }

    public abstract class BaseSerpent : VDrawable, IPosition
    {
        public const float HeadSize = 0.5f;
        public const float SegmentSize = 0.4f;

        public SerpentStatus SerpentStatus;
        public ITakeDirection DirectionTaker;

        protected PlayingField PlayingField;

        protected Whereabouts _whereabouts = new Whereabouts();
        public Direction HeadDirection { get; protected set; }

        protected double _fractionAngle;

        protected readonly IVDrawable _sphere;
        protected readonly Texture2D _serpentSkin;
        protected readonly Texture2D _serpentHeadSkin;
        protected readonly Texture2D _serpentBump;
        protected readonly Texture2D _eggSkin;

        protected SerpentTailSegment _tail;
        protected int _serpentLength;

        private readonly Dictionary<Direction, Matrix> _headRotation = new Dictionary<Direction, Matrix>();

        protected abstract void takeDirection(bool delayed);

        private int _pendingEatenSegments = 6;
        private const int SegmentEatTreshold = 7;

        private float _layingEgg = -1;
        private const float TimeForLayingEggProcess = 10;
        private Matrix _eggWorld;

        private float _ascendToHeaven;

        public bool IsLonger { get; protected set; }

        private readonly IVEffect _textureEffect;

        protected BaseSerpent(
            LarvContent lcontent,
            PlayingField playingField,
            Texture2D serpentSkin,
            Texture2D serpentHeadSkin,
            Texture2D serpentBump,
            Texture2D eggSkin)
            : base(lcontent.BumpEffect)
        {
            Restart(playingField, playingField.EnemyWhereaboutsStart);
            _sphere = lcontent.Sphere;
            _serpentSkin = serpentSkin;
            _serpentHeadSkin = serpentHeadSkin;
            _serpentBump = serpentBump;
            _eggSkin = eggSkin;
            _textureEffect = lcontent.TextureEffect;

            _headRotation.Add(Direction.East, Matrix.RotationY(MathUtil.Pi));
            _headRotation.Add(Direction.West, Matrix.Identity);
            _headRotation.Add(Direction.North, Matrix.RotationY(MathUtil.PiOverTwo));
            _headRotation.Add(Direction.South, Matrix.RotationY(-MathUtil.PiOverTwo));
        }

        public void Restart(PlayingField playingField, Whereabouts whereabouts)
        {
            PlayingField = playingField;
            _whereabouts = whereabouts;
            HeadDirection = _whereabouts.Direction;
            _tail = new SerpentTailSegment(PlayingField, _whereabouts);
            _serpentLength = 1;
            _ascendToHeaven = 0;
            _layingEgg = -1;
            _pendingEatenSegments = 6;
        }

        protected virtual float ModifySpeed()
        {
            return 1f;
        }

        protected bool TakeDirection()
        {
            return DirectionTaker != null &&
                   TryMove(HeadDirection.Turn(DirectionTaker.TakeDirection(this)), DirectionTaker.CanOverrideRestrictedDirections(this));
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            switch (SerpentStatus)
            {
                case SerpentStatus.Ghost:
                    _ascendToHeaven += (float) gameTime.ElapsedGameTime.TotalSeconds;
                    if (_ascendToHeaven > 4)
                        SerpentStatus = SerpentStatus.Finished;
                    return;
                case SerpentStatus.Finished:
                    return;
            }

            var lengthSpeed = Math.Max(0.001f, (11 - _serpentLength)/10f);
            var speed = (float) gameTime.ElapsedGameTime.TotalSeconds*5.0f*lengthSpeed*ModifySpeed();

            if (_whereabouts.Direction != Direction.None)
            {
                _fractionAngle += speed;
                if (_fractionAngle >= 1)
                {
                    _fractionAngle = 0;
                    _whereabouts.Location = _whereabouts.NextLocation;
                    takeDirection(false);
                } else if (_fractionAngle < 0.7f)
                    takeDirection(true);
                _whereabouts.Fraction = (float) Math.Sin(_fractionAngle*MathUtil.PiOverTwo);
            }
            else
                takeDirection(false);

            if (_tail != null)
                _tail.Update(speed, _whereabouts);

            if (_whereabouts.Direction != Direction.None)
                HeadDirection = _whereabouts.Direction;

            if (_layingEgg >= 0)
                _layingEgg += (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        protected bool TryMove(Direction dir, bool ignoreRestriction = false)
        {
            if (dir == Direction.None)
            {
                _whereabouts.Direction = Direction.None;
                return true;
            }
            var possibleLocationTo = _whereabouts.Location.Add(dir);
            if (!PlayingField.CanMoveHere(ref _whereabouts.Floor, _whereabouts.Location, possibleLocationTo, ignoreRestriction))
                return false;
            _whereabouts.Direction = dir;
            _tail.AddPathToWalk(_whereabouts);
            return true;
        }

        private static Vector3 wormTwist(ref float slinger)
        {
            slinger += 1.5f;
            return new Vector3((float) Math.Sin(slinger)*0.2f, 0, (float) Math.Sin(slinger)*0.2f);
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            var p = Position;

            var slinger = p.X + p.Z;
            p += wormTwist(ref slinger);

            camera.UpdateEffect(Effect);

            var worlds = new List<Matrix>
            {
                _headRotation[HeadDirection]*
                Matrix.Scaling(HeadSize)*
                Matrix.Translation(p.X, HeadSize + p.Y + _ascendToHeaven, p.Z)
            };

            // p is the the loc of the last segement - which is the head on the first round
            var segment = _tail;
            while (true)
            {
                var p2 = segment.Position + wormTwist(ref slinger);
                worlds.Add(
                    Matrix.Scaling(SegmentSize)*
                    Matrix.Translation(
                        (p.X + p2.X)/2,
                        SegmentSize + (p.Y + p2.Y)/2 + _ascendToHeaven,
                        (p.Z + p2.Z)/2));
                worlds.Add(
                    Matrix.Scaling(SegmentSize)*
                    Matrix.Translation(
                        p2.X,
                        SegmentSize + p2.Y + _ascendToHeaven,
                        p2.Z));
                p = p2;
                if (segment.Next == null)
                    break;
                segment = segment.Next;
            }

            if (_pendingEatenSegments <= SegmentEatTreshold/2)
                worlds.RemoveAt(worlds.Count - 1);

            if (_layingEgg > 0 && worlds.Count >= 3)
            {
                Effect.Texture = _eggSkin;
                _eggWorld = worlds.Last();
                Egg.Draw(_textureEffect, _eggSkin, TintColor(), _sphere, _eggWorld, segment.Whereabouts.Direction);

                //move the last two spheres so that they slowly dissolves into the serpent
                var factor = MathUtil.Clamp(_layingEgg/TimeForLayingEggProcess - 0.5f, 0, 1);
                var world1 = worlds.Last();
                worlds.RemoveAt(worlds.Count - 1);
                var world2 = worlds.Last();
                worlds.RemoveAt(worlds.Count - 1);
                var world3 = worlds.Last();
                world2.TranslationVector = Vector3.Lerp(world2.TranslationVector, world3.TranslationVector, factor);
                world1.TranslationVector = Vector3.Lerp(world1.TranslationVector, world2.TranslationVector, factor);
                worlds.Add(world2);
                worlds.Add(world1);
            }

            Effect.Parameters["BumpMap"].SetResource(_serpentBump);
            Effect.DiffuseColor = TintColor();

            Effect.Texture = _serpentHeadSkin;
            Effect.World = worlds.First();
            _sphere.Draw(Effect);

            Effect.Texture = _serpentSkin;
            foreach (var world in worlds.Skip(1))
            {
                Effect.World = world;
                _sphere.Draw(Effect);
            }

            Effect.DiffuseColor = Vector4.One;
            return true;
        }

        protected virtual Vector4 TintColor()
        {
            return Vector4.Zero;
        }

        protected float AlphaValue()
        {
            return SerpentStatus == SerpentStatus.Ghost
                ? MathUtil.Clamp(0.8f - _ascendToHeaven/5, 0, 1)
                : 1;
        }

        public Vector3 Position
        {
            get { return _whereabouts.GetPosition(PlayingField); }
        }

        public Whereabouts Whereabouts
        {
            get { return _whereabouts; }
        }

        private void grow(int length)
        {
            _pendingEatenSegments += length;
            for (var count = _pendingEatenSegments/SegmentEatTreshold; count > 0; count--)
                AddTail();
            _pendingEatenSegments %= SegmentEatTreshold;
        }

        public bool EatAt(BaseSerpent other, out int eatenSegments)
        {
            eatenSegments = 0;
            if (SerpentStatus != SerpentStatus.Alive || other.SerpentStatus != SerpentStatus.Alive)
                return false;
            IsLonger = _serpentLength >= other._serpentLength;
            if (SerpentStatus != SerpentStatus.Alive)
                return false;

            if (Vector3.DistanceSquared(Position, other.Position) < 0.8f)
            {
                if (other._serpentLength > _serpentLength)
                    return false;
                grow(other._serpentLength + 1);
                eatenSegments = other._serpentLength;
                return true;
            }

            for (var tail = other._tail; tail != null; tail = tail.Next)
                if (this.DistanceSquared(tail) < 0.2f)
                {
                    if (tail == other._tail)
                    {
                        grow(other._serpentLength + 1);
                        eatenSegments = other._serpentLength;
                        return true;
                    }
                    eatenSegments++;
                    grow(other.removeTail(tail));
                    return false;
                }
            return false;
        }

        private int removeTail(SerpentTailSegment tail)
        {
            _pendingEatenSegments = SegmentEatTreshold - 1;
            if (tail != _tail && tail != null)
            {
                var length = 1;
                for (var test = _tail; test != null; test = test.Next, length++)
                    if (test.Next == tail)
                    {
                        test.Next = null;
                        var removedSegments = _serpentLength - length;
                        _serpentLength = length;
                        return removedSegments;
                    }
            }
            throw new Exception("No tail to remove");
        }

        public IPosition RemoveTailWhenLevelComplete()
        {
            if (_tail.Next == null)
                return null;
            var tail = _tail;
            while (tail.Next.Next != null)
                tail = tail.Next;
            var result = tail.Next;
            tail.Next = null;
            return result;
        }

        public void AddTail()
        {
            var tail = _tail;
            while (tail.Next != null)
                tail = tail.Next;
            tail.Next = new SerpentTailSegment(PlayingField, tail.Whereabouts);
            _serpentLength++;
            System.Diagnostics.Debug.Assert(_serpentLength < 20);
        }

        public Egg TimeToLayEgg()
        {
            if (_layingEgg < TimeForLayingEggProcess)
                return null;
            _layingEgg = -1;

            var segment = _tail;
            while (segment.Next != null)
                segment = segment.Next;
            if (segment != _tail)
                removeTail(segment);
            else
                SerpentStatus = SerpentStatus.Ghost;
            return this is PlayerSerpent
                ? new Egg(_textureEffect, _sphere, _eggSkin, TintColor(), _eggWorld, segment.Whereabouts, float.MaxValue)
                : new Egg(_textureEffect, _sphere, _eggSkin, EnemySerpent.ColorWhenLonger, _eggWorld, segment.Whereabouts, 20);
        }

        public Vector3 LookAtPosition
        {
            get
            {
                var d = _whereabouts.Direction.DirectionAsPoint();
                return new Vector3(
                    _whereabouts.Location.X + d.X * (float)_fractionAngle,
                    PlayingField.GetElevation(_whereabouts),
                    _whereabouts.Location.Y + d.Y * (float)_fractionAngle);
            }
        }

        public bool IsPregnant
        {
            get { return _layingEgg >= 0; }
            set
            {
                if (value)
                {
                    if (_layingEgg < 0)
                        _layingEgg = 0;
                }
                else
                    _layingEgg = -1;
            }
        }

        public int Length
        {
            get
            {
                var length = 0;
                for (var segment = _tail; segment.Next != null; segment = segment.Next)
                    length++;
                return length;
            }
        }

        public bool EatFrogOrEgg(IPosition thing)
        {
            if (thing == null || Vector3.DistanceSquared(Position, thing.Position) > 0.4f)
                return false;
            grow(2);
            return true;
        }

    }

}
