using System;
using System.Collections.Generic;
using factor10.VisionThing.Effects;
using factor10.VisionThing.Terrain;
using SharpDX;
using SharpDX.Toolkit.Content;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing
{
    public class VisionContent : IDisposable
    {
        public static int RenderedTriangles;

        private static Vector3 _sunlightDirectionReflectedWater;
        private static Vector3 _sunlightDirection;

        public readonly ContentManager Content;
        public readonly GraphicsDevice GraphicsDevice;

        public readonly TerrainPlane TerrainPlane;

        public Dictionary<string, IDisposable> Disposables = new Dictionary<string, IDisposable>();
 
        static VisionContent()
        {
            SunlightDirectionReflectedWater = new Vector3(11f, -2f, -6f);
            SunlightDirection = new Vector3(11f, -7f, -6f);
        }

        public VisionContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            TerrainPlane = new TerrainPlane(this);
        }

        public static Vector3 SunlightDirectionReflectedWater
        {
            get { return _sunlightDirectionReflectedWater; }
            set
            {
                _sunlightDirectionReflectedWater = value;
                _sunlightDirectionReflectedWater.Normalize();
            }
        }

        public static Vector3 SunlightDirection
        {
            get { return _sunlightDirection; }
            set
            {
                _sunlightDirection = value;
                _sunlightDirection.Normalize();
            }
        }

        public Vector2 ClientSize
        {
            get { return new Vector2(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height); }
        }

        public T Load<T>(string name) where T: IDisposable
        {
            name = name.ToLower().Replace("\\", "/");
            IDisposable obj;
            if (Disposables.TryGetValue(name, out obj))
                return (T) obj;

            var contentObject = Content.Load<T>(name);
            Disposables.Add(name, contentObject);
            return contentObject;
        }

        public VisionEffect LoadEffect(string name, SamplerState samplerState = null)
        {
            return new VisionEffect(Load<Effect>(name), samplerState);
        }

        public virtual void Dispose()
        {
            TerrainPlane.Dispose();
            foreach (var d in Disposables.Values)
                d.Dispose();
            Disposables.Clear();
        }

    }

}
