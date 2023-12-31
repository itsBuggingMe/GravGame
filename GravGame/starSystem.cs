﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace GravGame
{
    internal class starSystem
    {
        Drawing drawing;
        Camera camera;

        List<planet> planets;
        List<planet> pretendPlanets = new List<planet>();
        Ship ship;

        Random random = new Random();

        int activePlanet = -1;
        int focusPlanet = -1;

        bool pause = true;
        bool playerController = false;

        float simulationSpeed = 1;

        float planetMass = 25;

        Color[] planetColors = new Color[] { new Color(57, 255, 20), Color.Aquamarine, new Color(235, 152, 19), Color.Magenta};
        int colorIndex;

        public starSystem(Drawing drawing, List<planet> planets) 
        { 
            this.drawing = drawing;
            this.planets = planets;
            this.ship = new Ship(drawing.fullScreenSize.ToVector2()/2f, 1, Vector2.Zero, Color.Gray);
            this.camera = drawing.camera;
            camera.SetZoom(0.1f);
            colorIndex = random.Next(planetColors.Length);
            planets.Add(new planet(drawing.fullScreenSize.ToVector2() / 2f, 2048, Vector2.Zero, Color.Yellow));
        }

        MouseState mouseState = Mouse.GetState();
        MouseState prevMouseState;

        KeyboardState keyboardState = Keyboard.GetState();
        KeyboardState prevKeyboardState;

        public void tick()
        {
            setInputs();

            updateCamera();

            if(keyPress(Keys.Space))
            {
                pause = !pause;
            }

            if(keyPress(Keys.K))
            {
                planets.Clear();
                activePlanet = -1;
                focusPlanet = -1;
            }

            if (!pause)
            {
                for(int i = 0; i < simulationSpeed; i++)
                {
                    tickPhysics();
                }
            }

            if (playerController)
            {
                playerInput();
            }
            else
            {
                genericInput();
            }
        }

        public void draw()
        {
            if(activePlanet == -1)
            {
                foreach (planet planet in planets)
                {
                    planet.drawLines(drawing.shapeBatch, drawing.camera);
                }
                foreach (planet planet in planets)
                {
                    planet.drawNoLines(drawing.shapeBatch, drawing.camera);
                }
            }
            else
            {
                foreach (planet planet in pretendPlanets)
                {
                    planet.drawLines(drawing.shapeBatch, drawing.camera);
                }
                foreach (planet planet in pretendPlanets)
                {
                    planet.drawNoLines(drawing.shapeBatch, drawing.camera);
                }
            }
            if(playerController)
                ship.draw(drawing);

            drawing.textBuffer.addText($"Planet Mass: {planetMass}\nSimulation Speed: {simulationSpeed}\n", new Vector2(16, 16), Color.White, 0, Vector2.Zero, 1);
        }

        public void genericInput()
        {
            if (rightMouseClick())
            {
                activePlanet = planets.Count;
                /*
                Color randColor = new Color((uint)random.NextInt64(4278190080));
                randColor.A = 255;

                if(planetMass >= 4096)
                {
                    randColor = Color.Yellow;
                }
                */

                
                Color randColor = planetColors[colorIndex];
                colorIndex++;
                colorIndex %= planetColors.Length;
                if(planetMass >= 1024)
                {
                    if(planetMass < 3072)
                    {
                        randColor = Color.Lerp(Color.Yellow, Color.LightGoldenrodYellow, mathFunc.normailise(3072, 1024, planetMass));
                    }
                    else
                    {
                        randColor = Color.Lerp(Color.LightGoldenrodYellow, new Color(120, 230, 255), mathFunc.normailise(4096, 3072, planetMass));
                    }
                }


                planets.Add(new planet(
                    camera.ScreenToWorld(mouseState.Position.ToVector2()),
                    planetMass,
                    new Vector2(0, 0),
                    randColor
                    ));
            }//start launch

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                pause = true;
                planet activePlanet = planets[this.activePlanet];
                activePlanet.momentum = -(activePlanet.location - camera.ScreenToWorld(mouseState.Position.ToVector2())) / 1028f;
                activePlanet.mass = planetMass;

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    activePlanet.location.Y -= 8;
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    activePlanet.location.X -= 8;
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    activePlanet.location.Y += 8;
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    activePlanet.location.X += 8;
                }


                pretendPlanets = new List<planet>(planets.Count);

                for (int i = 0; i < planets.Count; i++)
                {
                    pretendPlanets.Add(new planet(planets[i]));
                }

                int interations = planet.locationBufferSize * planet.locationSampleInterval;
                for (int i = 0; i < interations; i++)
                {
                    foreach (planet pretendPlanet in pretendPlanets)
                    {
                        pretendPlanet.calculateVectors(pretendPlanets);
                    }


                    bool colorChanged = false;
                    for (int j = pretendPlanets.Count - 1; j >= 0; j--)
                    {
                        if (pretendPlanets[j].remove)
                        {
                            pretendPlanets[j].planetColor = new Color(255, 49, 49);
                            colorChanged = true;
                        }
                    }
                    if (colorChanged)
                    {
                        break;
                    }

                    foreach (planet pretendPlanet in pretendPlanets)
                    {
                        pretendPlanet.tickStep();
                    }
                }
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    simulationSpeed *= 1.6f;
                    simulationSpeed = (float)Math.Clamp(Math.Round(simulationSpeed, 2), 1, 512);
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    simulationSpeed *= 0.94f;
                    simulationSpeed = (float)Math.Clamp(Math.Round(simulationSpeed, 2), 1, 512);
                }
            }

            if (focusPlanet != -1)
            {
                if (planets.Count == focusPlanet)
                {
                    focusPlanet = -1;
                }
                else
                {
                    camera.SetLocation(planets[focusPlanet].location);
                }
            }

            if (rightMouseRelease())
            {
                pause = false;
                planets[activePlanet].resetLocationBuffer();

                activePlanet = -1;
                pretendPlanets.Clear();
            }//end launch

            float deltaScroll = mouseState.ScrollWheelValue - prevMouseState.ScrollWheelValue;

            if (deltaScroll != 0 && keyboardState.IsKeyDown(Keys.LeftShift))
            {
                if (deltaScroll > 0)
                {
                    planetMass *= 1.2f;
                }
                else
                {
                    planetMass *= 0.8f;
                }
                planetMass = (float)Math.Clamp(Math.Round(planetMass, 2), 1, 4096);
            }


            if (keyPress(Keys.E))
            {
                playerController = true;
                ship = new Ship(camera.ScreenToWorld(drawing.fullScreenSize.ToVector2()/2), 0.2f, Vector2.Zero, Color.Gray);
            }


            if (keyPress(Keys.F))
            {
                if(focusPlanet == -1)
                {
                    float smallestDistance = float.MaxValue;
                    int index = -1;
                    Vector2 cameraCenterLocation = camera.ScreenToWorld(drawing.fullScreenSize.ToVector2() / 2f);
                    for (int i = 0; i < planets.Count; i++)
                    {
                        float distance = Vector2.Distance(cameraCenterLocation, planets[i].location);
                        if (distance < smallestDistance)
                        {
                            index = i;
                            smallestDistance = distance;
                        }
                    }

                    focusPlanet = index;
                }
                else
                {
                    focusPlanet = -1;
                }
            }
        }

        public void playerInput()
        {
            if (keyPress(Keys.E))
            {
                playerController = false;
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                simulationSpeed *= 1.04f;
                simulationSpeed = (float)Math.Clamp(Math.Round(simulationSpeed, 2), 1, 512);
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                simulationSpeed *= 0.8f;
                simulationSpeed = (float)Math.Clamp(Math.Round(simulationSpeed, 2), 1, 512);
            }

            if (keyboardState.IsKeyDown(Keys.W))
            {
                ship.applyForce();
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                ship.rotation -= 8;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                ship.rotation += 8;
            }

            camera.SetLocation(ship.location);
        }

        //FORGET ABOUT IT FUNCTIONS
        //
        //
        //

        public void setInputs()
        {
            prevKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            prevMouseState = mouseState;
            mouseState = Mouse.GetState();
        }

        public void updateCamera()
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                camera.Translate((camera.ScreenToWorld(mouseState.Position.ToVector2()) - camera.ScreenToWorld(prevMouseState.Position.ToVector2())));
            }

            float deltaScroll = mouseState.ScrollWheelValue - prevMouseState.ScrollWheelValue;

            if (deltaScroll != 0 && !keyboardState.IsKeyDown(Keys.LeftShift))
            {
                if (deltaScroll > 0)
                {
                    camera.ChangeZoom(1.1f);
                }
                else
                {
                    camera.ChangeZoom(0.9f);
                }
            }
        }
        
        public void tickPhysics()
        {
            for(int i = planets.Count - 1; i >= 0; i--)
            {
                planets[i].calculateVectors(planets);
            }

            ship.calculateVectors(planets);

            foreach (planet planet in planets)
            {
                planet.tickStep();
            }
            ship.tickStep();

            for (int j = planets.Count - 1; j >= 0; j--)
            {
                if (planets[j].remove)
                {
                    planets.RemoveAt(j);
                }
            }
        }

        public bool leftMouseClick()
        {
            return prevMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed;
        }
        public bool rightMouseClick()
        {
            return prevMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed;
        }
        public bool leftMouseRelease()
        {
            return prevMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released;
        }

        public bool rightMouseRelease()
        {
            return prevMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released;
        }
        public bool keyPress(Keys key)
        {
            return keyboardState.IsKeyDown(key) && !prevKeyboardState.IsKeyDown(key);
        }
    }
}
