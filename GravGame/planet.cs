using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Apos.Shapes;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
namespace GravGame
{
    internal class planet
    {
        public string tag;

        public Vector2 location;
        public Vector2 momentum;
        public float mass;
        const float G = 16;
        float planetRadius;
        public Color planetColor;

        public const int locationBufferSize = 7200;

        Vector2[] locationBuffer = new Vector2[locationBufferSize];
        List<Vector2> futurePositions = new List<Vector2>();

        int bufferIndex = 0;

        int ticks = 0;
        public bool remove = false;

        public planet(Vector2 location, float mass, Vector2 momentum, Color color)
        {
            this.location = location;

            this.momentum = momentum;
            this.mass = mass;
            this.planetRadius = diaGivenVol(mass);
            planetColor = color;


            for (int i = 0; i < locationBufferSize; i++)
            {
                locationBuffer[i] = location;
            }
        }
        public planet(planet copy)
        {
            this.location = copy.location;
            this.momentum = copy.momentum;
            this.mass = copy.mass;
            this.planetRadius = copy.planetRadius;
            this.planetColor = copy.planetColor;
            this.tag = copy.tag;
            


            for (int i = 0; i < locationBufferSize; i++)
            {
                locationBuffer[i] = location;
            }
        }

        public void tickStep()
        {
            location += momentum;
            ticks++;

            locationBuffer[bufferIndex] = location;
            bufferIndex = (bufferIndex + 1) % locationBufferSize;
        }

        public void tickStepPreciction()
        {
            location += momentum;
            ticks++;

            futurePositions.Add(location);
        }


        public void calculateVectors(List<planet> planets)
        {
            for (int i = planets.Count - 1; i >= 0; i--)
            {
                if(this != planets[i])
                {
                    momentum += calculateGravitationalForce(this, planets[i].location, planets[i].mass) / mass;

                    Vector2 potentialLocation = location + momentum;
                    float distance = Vector2.Distance(planets[i].location, potentialLocation);
                    float collisionDistance = planetRadius + planets[i].planetRadius;
                    
                    if(distance < collisionDistance && this.mass >= planets[i].mass)
                    {
                        this.mass += planets[i].mass;
                        this.planetRadius = diaGivenVol(mass);

                        if (mass >= 1024)
                        {
                            if (mass < 3072)
                            {
                                planetColor = Color.Lerp(Color.Yellow, Color.LightGoldenrodYellow, mathFunc.normailise(3072, 1024, mass));
                            }
                            else
                            {
                                planetColor = Color.Lerp(Color.LightGoldenrodYellow, new Color(60, 255, 255), mathFunc.normailise(4096, 3072, mass));
                            }
                        }
                        planets[i].remove = true;
                    }
                }
            }
        }

        public void draw(ShapeBatch shapeBatch, Camera camera)
        {


            shapeBatch.FillCircle(camera.WorldToScreen(location), planetRadius * camera.Zoom, planetColor);

        }

        public void drawLines(ShapeBatch shapeBatch, Camera camera)
        {

            for (int i = 0; i < locationBuffer.Length - 1; i++)
            {
                int currentIndex = (bufferIndex + i) % locationBuffer.Length;
                int nextIndex = (currentIndex + 1) % locationBuffer.Length;


                float opacity = 1;
                int threshhold = 7200;
                if (i < threshhold)
                {
                    float x = (i / (float)threshhold);
                    opacity = x * x;
                }
                ticks++;
                shapeBatch.FillLine(camera.WorldToScreen(locationBuffer[currentIndex]), camera.WorldToScreen(locationBuffer[nextIndex]), camera.Zoom * 0.6f, planetColor * opacity);
            }

            int lastIndex = (bufferIndex + locationBuffer.Length - 1) % locationBuffer.Length;
            shapeBatch.FillLine(camera.WorldToScreen(locationBuffer[lastIndex]), camera.WorldToScreen(location), 1, planetColor);
        }

        public void drawNoLines(ShapeBatch shapeBatch, Camera camera)
        {
            shapeBatch.FillCircle(camera.WorldToScreen(location), planetRadius * camera.Zoom, planetColor);
        }

        public static Vector2 calculateGravitationalForce(planet self, Vector2 otherLocation, float otherMass)
        {
            Vector2 direction = otherLocation - self.location;
            float distanceSquared = direction.LengthSquared();
            float factor = (G * self.mass * otherMass) / (distanceSquared * (float)Math.Sqrt(distanceSquared));

            return direction * factor;
        }
        private static float diaGivenVol(float vol)
        {
            return (3 * vol / 4 * MathF.PI) / 2f;
        }

        public static Color weightedAverageColor(Color color1, double weight1, Color color2, double weight2)
        {

            int red = (int)((color1.R * weight1 + color2.R * weight2) / (weight1 + weight2));
            int green = (int)((color1.G * weight1 + color2.G * weight2) / (weight1 + weight2));
            int blue = (int)((color1.B * weight1 + color2.B * weight2) / (weight1 + weight2));

            return new Color(red, green, blue);
        }

    }

    internal class Ship
    {
        public string name;

        public Vector2 location;
        public Vector2 momentum;
        float mass;
        const float G = 256;
        float planetRadius;
        Color shipColor;
        Color accentColor;

        public const int locationBufferSize = 7200;

        Vector2[] locationBuffer = new Vector2[locationBufferSize];

        int bufferIndex = 0;

        int ticks = 0;

        public float rotation = 0;

        bool thrusting = false;

        public Ship(Vector2 location, float mass, Vector2 momentum, Color color)
        {
            this.location = location;

            this.momentum = momentum;
            this.mass = mass;
            this.planetRadius = diaGivenVol(mass);
            shipColor = color;

            accentColor = new Color(Math.Max(color.R - 24, 0), Math.Max(color.G - 24, 0), Math.Max(color.B - 24, 0));

            for (int i = 0; i < locationBufferSize; i++)
            {
                locationBuffer[i] = location;
            }
        }
        public Ship(Ship copy)
        {
            this.location = copy.location;
            this.momentum = copy.momentum;
            this.mass = copy.mass;
            this.planetRadius = copy.planetRadius;
            this.shipColor = copy.shipColor;
            this.name = copy.name;

            for (int i = 0; i < locationBufferSize; i++)
            {
                locationBuffer[i] = copy.locationBuffer[i];
            }
        }

        public void tickStep()
        {
            thrusting = false;
            location += momentum;
            ticks++;

            if (ticks % 10 == 0)
            {
                locationBuffer[bufferIndex] = location;
                bufferIndex = (bufferIndex + 1) % locationBufferSize;
            }
        }

        public void calculateVectors(List<planet> planets)
        {
            for (int i = 0; i < planets.Count; i++)
            {
                if (this.location != planets[i].location)
                {
                    momentum += calculateGravitationalForce(this, planets[i].location, planets[i].mass) / mass;
                }
            }
        }

        public void applyForce()
        {
            rotation = rotation % 360;
            this.momentum += Rotate(new Vector2(0.1f, 0), rotation);
            thrusting = true;
        }

        public void draw(Drawing drawing)
        {
            //Vector2 size = Vector2.One * planetRadius * drawing.camera.Zoom * 40;
            drawing.spriteBatch.Draw(drawing.textures.get("landerFinal"), drawing.camera.WorldToScreen(location), null, Color.White, MathHelper.ToRadians(rotation - 90), new Vector2(drawing.textures.get("landerFinal").Width/2, drawing.textures.get("landerFinal").Height/2), drawing.camera.Zoom * 0.125f, SpriteEffects.None, 1);
            if(thrusting)
            {
                Vector2 from = drawing.camera.WorldToScreen(location);
                
                Vector2 to = -Rotate(new Vector2(drawing.camera.Zoom * ((float)Math.Sin(ticks * 0.5f) * 3 + 56), 0), rotation);
                drawing.shapeBatch.DrawLine(from + to / 2f, to + from, 10.8f * drawing.camera.Zoom, Color.Yellow, Color.Orange, thickness : 4 * drawing.camera.Zoom);
            }

            for (int i = 0; i < locationBuffer.Length - 1; i++)
            {
                int currentIndex = (bufferIndex + i) % locationBuffer.Length;
                int nextIndex = (currentIndex + 1) % locationBuffer.Length;
                drawing.shapeBatch.FillLine(drawing.camera.WorldToScreen(locationBuffer[currentIndex]), drawing.camera.WorldToScreen(locationBuffer[nextIndex]), 1, shipColor);
            }

            int lastIndex = (bufferIndex + locationBuffer.Length - 1) % locationBuffer.Length;
            drawing.shapeBatch.FillLine(drawing.camera.WorldToScreen(locationBuffer[lastIndex]), drawing.camera.WorldToScreen(location), 1, shipColor);

        }
        public void drawNoLines(ShapeBatch shapeBatch, Camera camera)
        {
            shapeBatch.DrawCircle(camera.WorldToScreen(location), planetRadius * camera.Zoom, shipColor, accentColor, thickness: 64 * camera.Zoom);
        }

        public static Vector2 calculateGravitationalForce(Ship self, Vector2 otherLocation, float otherMass)
        {
            Vector2 direction = otherLocation - self.location;
            float distanceSquared = direction.LengthSquared();
            float factor = (G * self.mass * otherMass) / (distanceSquared * (float)Math.Sqrt(distanceSquared));

            return direction * factor;
        }
        private static float diaGivenVol(float vol)
        {
            return (3 * vol / 4 * MathF.PI) * 2;
        }

        public static Vector2 Rotate(Vector2 vector, float angle)
        {
            float angleInRadians = MathHelper.ToRadians(angle);
            float cos = (float)Math.Cos(angleInRadians);
            float sin = (float)Math.Sin(angleInRadians);

            float newX = vector.X * cos - vector.Y * sin;
            float newY = vector.X * sin + vector.Y * cos;

            return new Vector2(newX, newY);
        }
    }
}