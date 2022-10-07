#region File Description

//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion File Description

#region Using Statements

using GameStateManagement.Class;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;

#endregion Using Statements

namespace GameStateManagement
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    internal class GameplayScreen : GameScreen
    {
        #region Fields

        private ContentManager Content;

        #endregion Fields

        #region Variablen
        private SpriteBatch _spriteBatch;

        // player
        private Player player;

        //enemy
        private Enemy enemy;

        //controller
        private Controller controller;     

        // Font
        private SpriteFont spriteFont;

        // Viewport
        private Viewport viewport;

        // Sprites
        //private Texture2D ShipTexture;
        private Texture2D StarTexture;

        // Laser Variablen
        private List<Vector2> laserShots = new List<Vector2>();       

        // Spieler-Punkte und Zeichenposition der Punkte
        private int playerScore;
        private Vector2 scorePosition;

        //Kollision
        private Rectangle safeBounds;
        private const float SafeAreaPortion = 0.05f;

        #endregion Variablen

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");
            
            if(player == null)
            {
                player = new Player(Content.Load<Texture2D>("ship"), Content.Load<Texture2D>("laser"), Content.Load<SoundEffect>("laserfire"));
            }

            if (enemy == null)
            {
                enemy = new Enemy(Content.Load<Texture2D>("enemy"), Content.Load<SoundEffect>("explosion"));
            }

            if(controller == null)
            {
                controller = new Controller();
            }

            // Ein SpriteBatch zum Zeichnen
            _spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            // Viewport speichern
            viewport = ScreenManager.GraphicsDevice.Viewport;

            //Kollider initialisieren
            safeBounds = new Rectangle(
                (int)(viewport.Width * SafeAreaPortion),
                (int)(viewport.Height * SafeAreaPortion),
                (int)(viewport.Width * (1 - 2 * SafeAreaPortion)),
                (int)(viewport.Height * (1 - 2 * SafeAreaPortion)));

            // Texturen laden
            StarTexture = Content.Load<Texture2D>("starfield");

            // Font laden
            spriteFont = Content.Load<SpriteFont>("Verdana");

            //Sound lautstärke
            SoundEffect.MasterVolume = 0.025f;

            // Das Raumschiff positionieren
            player.setShipPosition(new Vector2(viewport.Width/2, viewport.Height - 100));

            // Radius der Feinde festlegen
            enemy.calcRadius();

            // Position der Score Ausgabe festlegen
            scorePosition = new Vector2(25, 25);

        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            Content.Unload();
        }

        #endregion Initialization

        #region Update and Draw

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            //CurrentKeyboardState
            controller.setCurrentKeyboardState(Keyboard.GetState());

            // Prevent the person from moving off of the screen
            player.setShipPosition(new Vector2(MathHelper.Clamp(player.getShipPosition().X, safeBounds.Left, safeBounds.Right - player.getShipTexture().Width), player.getShipPosition().Y));

            // Space
            if (controller.IsNewKeyPressed(Keys.Space))
            {
                player.FireLaser(laserShots);
            }

            controller.setPreviousKeyboardState(controller.getCurrentKeyboardState());
            
            //Enemy pause 
            if (!otherScreenHasFocus)
            {
                enemy.UpdateEnemies();
            }
            
            UpdateLaserShots();

            base.Update(gameTime, otherScreenHasFocus, false);
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                // Otherwise move the player position.
                // player movement
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    player.MoveShipLeft();
                }

                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    player.MoveShipRight();
                }
            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Hintergrund zeichnen
            DrawBackground();

            // Das Schiff zeichnen
            DrawSpaceShip();

            // Laser zeichnen
            DrawLaser();

            // Feinde zeichnen
            DrawEnemy();

            // Punkte anzeigen
            DrawScore();

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion Update and Draw
               
        #region Update von Lasern und Gegnern

        public void UpdateLaserShots()
        {
            int laserIndex = 0;

            while (laserIndex < laserShots.Count)
            {
                // hat der Schuss den Bildschirm verlassen?
                if (laserShots[laserIndex].Y < 0)
                {
                    laserShots.RemoveAt(laserIndex);
                }
                else
                {
                    // Position des Schusses aktualiesieren
                    Vector2 pos = laserShots[laserIndex];
                    //pos.Y -= laserSpeed;
                    pos.Y -= player.getLaserSpeed();
                    laserShots[laserIndex] = pos;

                    // Überprüfen ob ein Treffer vorliegt
                    int enemyIndex = 0;

                    while (enemyIndex < enemy.getEnemyPositions().Count)
                    {
                        // Abstand zwischen Feind-Position und Schuss-Position ermitteln
                        float distance = Vector2.Distance(enemy.getEnemyPositions()[enemyIndex], laserShots[laserIndex]);

                        // Treffer?
                        if (distance < enemy.getEnemyRadius())
                        {
                            // Schuss entfernen
                            laserShots.RemoveAt(laserIndex);
                            // Feind entfernen
                            enemy.getEnemyPositions().RemoveAt(enemyIndex);
                            // Punkte erhöhen
                            playerScore++;

                            enemy.PlayExplosionSound();

                            // Schleife verlassen
                            break;
                        }
                        else
                        {
                            enemyIndex++;
                        }
                    }
                    laserIndex++;
                }
            }
        }

        #endregion

        #region methods
     
        private void DrawBackground()
        {
            // TODO
            // Die Sternenfeld Grafik an der Position 0,0 zeichnen
            _spriteBatch.Draw(StarTexture, new Vector2(0, 0), Color.White);
        }

        private void DrawSpaceShip()
        {
            // TODO
            // Das Schiff mittig an den Koordinaten des Schiffes (shipPosition) zeichnen 
            //_spriteBatch.Draw(ShipTexture, shipPosition, Color.White);
            _spriteBatch.Draw(player.getShipTexture(), player.getShipPosition(), Color.White);
        }

        private void DrawLaser()
        {
            // TODO
            // Die Liste mit den Laser-Schüssen (laserShots) durchlaufen
            // und alle Schüsse (LaserTexture) zeichnen
            foreach (Vector2 laser in laserShots)
            {
                //_spriteBatch.Draw(LaserTexture, laser, Color.Green);
                _spriteBatch.Draw(player.getLaserTexture(), laser, Color.Green);
            }
        }

        private void DrawEnemy()
        {
            // TODO
            // Die Liste mit allen Gegnern (enemyPositions) durchlaufen
            // und alle Feinde (EnemyTexture) zeichnen
            foreach (Vector2 enemypos in enemy.getEnemyPositions())
            {
                _spriteBatch.Draw(enemy.getEnemyTexture(), enemypos, enemy.getEnemyColor());
            }
        }

        private void DrawScore()
        {
            // TODO
            // Die Punkte (playerScore) oben links (scorePosition) anzeigen
            _spriteBatch.DrawString(spriteFont, "Highscore: " + playerScore, scorePosition, Color.White);
        }
        #endregion methods
    }
}