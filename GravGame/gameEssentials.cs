using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apos.Shapes;
using Bloom_Sample;
using System.Reflection.Metadata;

namespace GravGame
{
    public class Drawing
    {
        public BloomFilter _bloomFilter;
        public textBuffer textBuffer;
        public SpriteBatch spriteBatch;
        public ShapeBatch shapeBatch;
        public SpriteFont font;
        public SoundFX sounds;
        public Textures textures;
        public Effects effects;
        public GraphicsDevice graphics;
        public Game game;
        public Camera camera;
        public Point fullScreenSize;
        public Drawing(Game game, Point fullScreenSize)
        {
            this.fullScreenSize = fullScreenSize;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            shapeBatch = new ShapeBatch(game.GraphicsDevice, game.Content);
            graphics = game.GraphicsDevice;
            this.game = game;
            camera = new Camera(fullScreenSize);
            _bloomFilter = new BloomFilter();
            _bloomFilter.Load(game.GraphicsDevice, game.Content, fullScreenSize.X, fullScreenSize.Y);
            _bloomFilter.BloomPreset = BloomFilter.BloomPresets.Focussed;
            _bloomFilter.BloomThreshold = 0;

            effects = new Effects();
            sounds = new SoundFX();
            textures = new Textures();
        }

        public void addSound(string name)
        {
            sounds.addSound(name, game.Content.Load<SoundEffect>(name));
        }
        public void addTexture(string name)
        {
            textures.addTexture(name, game.Content.Load<Texture2D>(name));
        }

        public void addEffect(string name)
        {
            effects.addEffect(name, game.Content.Load<SpriteEffect>(name));
        }

        public void setFont(string name)
        {
            font = game.Content.Load<SpriteFont>(name);
            textBuffer = new textBuffer(spriteBatch, font);
        }

        public class SoundFX
        {
            Dictionary<string, SoundEffect> soundDictionary = new Dictionary<string, SoundEffect>();
            public SoundFX()
            {

            }

            public void addSound(string name, SoundEffect sound)
            {
                soundDictionary[name] = sound;
            }

            public void play(string name)
            {
                if (soundDictionary.TryGetValue(name, out SoundEffect sound))
                {
                    sound.Play();
                }
                else
                {
                    throw new Exception($"Sound File: {name} does not exist");
                }
            }

            public void play(string name, float volume)
            {
                if (soundDictionary.TryGetValue(name, out SoundEffect sound))
                {
                    SoundEffectInstance instance = sound.CreateInstance();
                    instance.Volume = MathHelper.Clamp(volume, 0.0f, 1.0f);
                    instance.Play();
                }
                else
                {
                    throw new Exception($"Sound File: {name} does not exist");
                }
            }
        }
        public class Textures
        {
            Dictionary<string, Texture2D> textureDictionary = new Dictionary<string, Texture2D>();
            public Textures()
            {
            }

            public void addTexture(string name, Texture2D texture)
            {
                textureDictionary[name] = texture;
            }

            public Texture2D get(string name)
            {
                if (textureDictionary.TryGetValue(name, out Texture2D texture))
                {
                    return texture;
                }
                else
                {
                    throw new Exception($"Texture2D File: {name} does not exist");
                }
            }
        }
        public class Effects
        {
            Dictionary<string, SpriteEffect> textureDictionary = new Dictionary<string, SpriteEffect>();
            public Effects()
            {
            }

            public void addEffect(string name, SpriteEffect texture)
            {
                textureDictionary[name] = texture;
            }

            public SpriteEffect get(string name)
            {
                if (textureDictionary.TryGetValue(name, out SpriteEffect texture))
                {
                    return texture;
                }
                else
                {
                    throw new Exception($"SpriteEffect File: {name} does not exist");
                }
            }
        }
    }
    public class Camera
    {
        public float Zoom { get; private set; }
        public Vector2 Position { get; private set; }
        private Point ScreenSize;

        private readonly Vector2 ScreenSizeVectorHalf;

        public Camera(Point screenSize)
        {
            Zoom = 1;
            Position = screenSize.ToVector2() / 2;
            ScreenSize = screenSize;
            ScreenSizeVectorHalf = screenSize.ToVector2() / 2f;

        }

        public Vector2 ScreenToWorld(Vector2 screen)
        {
            return (screen - ScreenSize.ToVector2() / 2) / Zoom + Position;
        }

        public Vector2 WorldToScreen(Vector2 world)
        {
            return (world - Position) * Zoom + ScreenSizeVectorHalf;
        }

        public void Translate(Vector2 offset)
        {
            Position -= offset;
        }
        public void SetLocation(Vector2 loc)
        {
            Position = loc;
        }

        public void SetZoom(float zoom)
        {
            Zoom = Math.Max(zoom, 0.00001f);
        }
        public void ChangeZoom(float zoom)
        {
            Zoom *= zoom;
            Zoom = Math.Max(Zoom, 0.00001f);
        }
    }

    public static class mathFunc
    {
        public static float normailise(float max, float min, float value)
        {
            return (Math.Max(min, Math.Min(max, value)) - min) / (max - min);
        }
    }

    public class textBuffer
    {
        SpriteBatch spriteBatch;
        SpriteFont font;

        List<string> text = new List<string>();
        List<Vector2> position = new List<Vector2>();
        List<Color> color = new List<Color>();
        List<float> rotation = new List<float>();
        List<Vector2> origin = new List<Vector2>();
        List<float> scale = new List<float>();


        public textBuffer(SpriteBatch spriteBatch, SpriteFont font)
        {
            this.spriteBatch = spriteBatch;
            this.font = font;
        }

        public void addText(string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale)
        {
            this.text.Add(text);
            this.position.Add(position);
            this.color.Add(color);
            this.rotation.Add(rotation);
            this.origin.Add(origin);
            this.scale.Add(scale);
        }

        public void draw()
        {
            for (int i = 0; i < text.Count; i++)
            {
                spriteBatch.DrawString(
                    font,
                    text[i],
                    position[i],
                    color[i],
                    rotation[i],
                    origin[i],
                    scale[i],
                    SpriteEffects.None,
                    1
                );
            }
            text = new List<string>();
            position = new List<Vector2>();
            color = new List<Color>();
            rotation = new List<float>();
            origin = new List<Vector2>();
            scale = new List<float>();
        }
    }
}
