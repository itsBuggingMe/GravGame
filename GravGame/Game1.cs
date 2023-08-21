using Apos.Shapes;
using Bloom_Sample;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GravGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private Drawing drawing;
        private starSystem starSystem;
        private RenderTarget2D preBloom;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Point fullScreenSize = new Point(_graphics.GraphicsDevice.DisplayMode.Width, _graphics.GraphicsDevice.DisplayMode.Height);

            _graphics.PreferredBackBufferHeight = fullScreenSize.Y;
            _graphics.PreferredBackBufferWidth = fullScreenSize.X;
            Window.Position = new Point(
                (GraphicsDevice.DisplayMode.Width - fullScreenSize.X) / 2,
                (GraphicsDevice.DisplayMode.Height - fullScreenSize.Y) / 2  
                );
            _graphics.ApplyChanges();

            preBloom = new RenderTarget2D(_graphics.GraphicsDevice, fullScreenSize.X, fullScreenSize.Y);
            

            drawing = new Drawing(this, fullScreenSize);
            this.starSystem = new starSystem(drawing, new System.Collections.Generic.List<planet>());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            drawing.setFont("File");
            drawing.addTexture("landerFinal");


        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            starSystem.tick();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(preBloom);
            GraphicsDevice.Clear(Color.Black);

            drawing.spriteBatch.Begin();
            drawing.shapeBatch.Begin();

            starSystem.draw();

            drawing.shapeBatch.End();
            drawing.spriteBatch.End();
            

            Texture2D bloom = drawing._bloomFilter.Draw(preBloom, drawing.fullScreenSize.X, drawing.fullScreenSize.Y);

            GraphicsDevice.SetRenderTarget(null);
            drawing.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            drawing.spriteBatch.Draw(preBloom, Vector2.Zero, Color.White);
            drawing.spriteBatch.Draw(bloom, Vector2.Zero, Color.White);
            drawing.textBuffer.draw();
            drawing.spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}