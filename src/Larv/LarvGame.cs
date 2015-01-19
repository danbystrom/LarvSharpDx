using System.Collections.Generic;
using System.IO;
using System.Linq;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Primitives;
using factor10.VisionThing.Util;
using Larv.Field;
using Larv.GameStates;
using Larv.Serpent;
using Larv.Util;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit.Input;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Color = SharpDX.Color;
using Keys = SharpDX.Toolkit.Input.Keys;

namespace Larv
{
    public class LarvGame : Game
    {
        private GraphicsDeviceManager _graphicsDeviceManager;

        private IGameState _gameState;
        private LarvContent _lcontent;
        private Serpents _serpents;

        private readonly FramesPerSecondCounter _fps = new FramesPerSecondCounter();

        private bool _paused;

        public LarvGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
            _graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
#if false
            var screen = Screen.AllScreens.First(_ => !_.Primary);
            _graphicsDeviceManager.IsFullScreen = true;
            _graphicsDeviceManager.PreferredBackBufferWidth = screen.Bounds.Width;
            _graphicsDeviceManager.PreferredBackBufferHeight = screen.Bounds.Height;
#else
            _graphicsDeviceManager.PreferredBackBufferWidth = 1920;
            _graphicsDeviceManager.PreferredBackBufferHeight = 1080;
#endif

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Window.Title = "Larv! by Dan Byström - factor10 Solutions";
            var nativeWindow = (Form) Window.NativeWindow;
            nativeWindow.WindowState = FormWindowState.Maximized;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Larv.app.ico"))
                nativeWindow.Icon = new Icon(stream);

            IsMouseVisible = true;
            base.Initialize();
        }

        private List<string> loadSceneDescriptions()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Larv.PlayingFields.txt"))
            using (var fs = new StreamReader(stream))
            {
                var list = new List<string>();
                while (!fs.EndOfStream)
                    list.Add(fs.ReadLine());
                return list;
            }
        }

        protected override void LoadContent()
        {
            _lcontent = new LarvContent(GraphicsDevice, Content, loadSceneDescriptions());

            var shadowCameraPos = new Vector3(12, 4, 12) - VisionContent.SunlightDirection*32;
            var shadowCameraLookAt = shadowCameraPos + VisionContent.SunlightDirection;
            _lcontent.ShadowMap.Camera.Update(shadowCameraPos, shadowCameraLookAt);

            var camera = new Camera(
                _lcontent.ClientSize,
                new KeyboardManager(this),
                new MouseManager(this),
                new PointerManager(this),
                AttractState.CameraPosition,
                AttractState.CameraLookAt) {MovingSpeed = 8};
            _serpents = new Serpents(_lcontent, camera, 0);
            _lcontent.ShadowMap.ShadowCastingObjects.Add(_serpents);
            _gameState = new AttractState(_serpents);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _fps.Update(gameTime);
            _serpents.Camera.UpdateInputDevices();
            _lcontent.Ground.Update(_serpents.Camera, gameTime);

            _paused ^= _serpents.Camera.KeyboardState.IsKeyPressed(Keys.P);
            if (_paused)
                _serpents.Camera.UpdateFreeFlyingCamera(gameTime);
            else
                _gameState.Update(_serpents.Camera, gameTime, ref _gameState);

            for (var key = Keys.D1; key < Keys.D5; key++)
                if (_serpents.Camera.KeyboardState.IsKeyPressed(key))
                    _gameState = new GotoBoardState(_serpents, key - Keys.D1);

            if (_serpents.Camera.KeyboardState.IsKeyDown(Keys.Escape))
            {
                _lcontent.Dispose();
                Exit();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            _lcontent.ShadowMap.Draw(_serpents.Camera);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _gameState.Draw(_serpents.Camera, DrawingReason.Normal, _lcontent.ShadowMap);

            //GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.WireFrame);
 
            using (_lcontent.UsingSpriteBatch())
                _lcontent.DrawString("FPS: {0}  {1}".Fmt(_fps.FrameRate, _gameState), Vector2.Zero, 0.5f, 0, Color.White);

            base.Draw(gameTime);
        }

    }

}
