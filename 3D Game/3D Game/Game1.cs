using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _3D_Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // For sound
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;
        Cue trackCue;

        // Shot variables could be used for special shots too &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        float shotSpeed = 10;
        int shotDelay = 300;
        int shotCountdown = 0;
        int specialShotCountdown = 0;
        int specialList = 5;

        // For random numbers
        public Random rnd { get; protected set; }
        // For camera
        public Camera camera { get; protected set; }

        //For model manager direction &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
     //   public SpinningEnemy spinningEnemy2 { get; }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Class level variable
        ModelManager modelManager;

        //for crosshair
        Texture2D crosshairTexture;

        // variables for game stats
        public enum GameState { START, PLAY, LEVEL_CHANGE, END }
        GameState currentGameState = GameState.START;

        SplashScreen splashScreen;
        int score = 0;

        //font for score
        SpriteFont scoreFont;

        int originalShotDely = 300;
        public enum PowerUps { RAPID_FIRE }
        int shotDelyRapidFire = 100;
        int rapidFireTime = 10000;
        int powerUpCountdown = 0;
        string powerUpText = "";
        int powerUpTextTimer = 0;
        SpriteFont powerUpFont;

       



        public void ChangeGameState(GameState state, int level)
        {
            currentGameState = state;
            CancelPowerUps();

            switch (currentGameState)
            {
                case GameState.LEVEL_CHANGE:
                    splashScreen.SetData("Level " + (level + 1),
                        GameState.LEVEL_CHANGE);
                    modelManager.Enabled = false;
                    modelManager.Visible = false;
                    splashScreen.Enabled = true;
                    splashScreen.Visible = true;

                    //Stop the sound track look
                    trackCue.Stop(AudioStopOptions.Immediate);
                    break;

                case GameState.PLAY:
                    modelManager.Enabled = true;
                    modelManager.Visible = true;
                    splashScreen.Enabled = false;
                    splashScreen.Visible = false;

                    if (trackCue.IsPlaying)
                        trackCue.Stop(AudioStopOptions.Immediate);

                    //To play a stopped cue, get the cue from the sound back a gian
                    trackCue = soundBank.GetCue("Tracks");
                    trackCue.Play();
                    break;

                case GameState.END:
                    splashScreen.SetData("Game Over.\nLevel: " + (level + 1) +
                        "\nScore: " + score, GameState.END);
                    modelManager.Enabled = false;
                    modelManager.Visible = false;
                    splashScreen.Enabled = true;
                    splashScreen.Visible = true;

                    //Stop the sound loop
                    trackCue.Stop(AudioStopOptions.Immediate);
                    break;

            }
        }



        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            rnd = new Random();

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;

#if !DEBUG
            graphics.IsFullScreen = true;
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera = new Camera(this, new Vector3(0, 0, 50),
                Vector3.Zero, Vector3.Up);
            Components.Add(camera);

            //Intilize the Model manager
            modelManager = new ModelManager(this);
            Components.Add(modelManager);

            modelManager.Enabled = false;
            modelManager.Visible = false;

            //Splash screen component
            splashScreen = new SplashScreen(this);
            Components.Add(splashScreen);
            splashScreen.SetData("Welcome to space Defender!",
                currentGameState);


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            crosshairTexture = Content.Load<Texture2D>(@"textures\crosshair");
            powerUpFont = Content.Load<SpriteFont>(@"fonts\PowerUpFont");
            //Load sound
            audioEngine = new AudioEngine(@"Content\Audio\GameAudio.xgs");
            waveBank = new WaveBank(audioEngine, @"Content\Audio\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, @"Content\Audio\Sound Bank.xsb");
            trackCue = soundBank.GetCue("Tracks");
            trackCue.Play();

            //load score font
            scoreFont = Content.Load<SpriteFont>(@"Fonts\ScoreFont");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //Only shots when in play mode
            if (currentGameState == GameState.PLAY)
            {
                // see if the player fierd a shot
                FireShots(gameTime);
                FireSpecialShots(gameTime);
            }

            UpdatePowerUp(gameTime);

            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            base.Draw(gameTime);

            //Only the crosshair will draw when its on game mode

            if (currentGameState == GameState.PLAY)
            {
                spriteBatch.Begin();

                //Draw the current score
                string scoreText = "Score: " + score;
                spriteBatch.DrawString(scoreFont, scoreText,
                    new Vector2(10, 10), Color.Red);

                string WeaponText = "Special Weapon: " + specialList;
                spriteBatch.DrawString(scoreFont, WeaponText,
                    new Vector2(10, scoreFont.MeasureString(scoreText).Y + 40),
                    Color.Red);

                //Let the player know how maney misses left
                spriteBatch.DrawString(scoreFont, "Misses Left: " +
                    modelManager.missesLeft,
                    new Vector2(10, scoreFont.MeasureString(scoreText).Y + 10),
                    Color.Red);
                spriteBatch.Draw(crosshairTexture,
                    new Vector2((Window.ClientBounds.Width / 2)
                    - (crosshairTexture.Width / 2),
                    (Window.ClientBounds.Height / 2)
                    - (crosshairTexture.Height / 2)),
                Color.White);

                // If power up time is a live
                if (powerUpTextTimer > 0)
                {
                    powerUpTextTimer -= gameTime.ElapsedGameTime.Milliseconds;
                    Vector2 textSize = powerUpFont.MeasureString(powerUpText);
                    spriteBatch.DrawString(powerUpFont,
                        powerUpText,
                        new Vector2((Window.ClientBounds.Width / 2) -
                            (textSize.X / 2),
                            (Window.ClientBounds.Height / 2) -
                            (textSize.Y / 2)),
                            Color.Goldenrod);
                }

                spriteBatch.End();
            }
        } 

        protected void FireShots(GameTime gameTime)
        {
            if (shotCountdown <= 0)
            {
                //Did the player press the spacebare or left  click
                if (Keyboard.GetState().IsKeyDown(Keys.Space) ||
                    Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    //Add a shot to the model manager
                    modelManager.AddShot(camera.cameraPosition + new Vector3(0, -5, 0),
                        camera.GetCameraDirection * shotSpeed);
                    //play shot sound
                    PlayCue("Shot");

                    //reset the shot count down
                    shotCountdown = shotDelay;
                }
            }
            else
                shotCountdown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        protected void FireSpecialShots(GameTime gameTime) //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        {
            if (specialList > 0)
            {
                if (specialShotCountdown <= 0)
                {
                    //Did the player press the spacebare or left  click
                    if (Keyboard.GetState().IsKeyDown(Keys.X) ||
                        Mouse.GetState().RightButton == ButtonState.Pressed)
                    {
                        //Add a shot to the model manager &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

                        Vector3 localDirection;
                        Vector3 localPosition;

                        localDirection = modelManager.getDirectionEnemy;
                        localPosition = modelManager.getPositionEnemy();

                        modelManager.AddSpecialShots(camera.cameraPosition + new Vector3(0, 5, 0),
                            camera.GetCameraDirection * (shotSpeed / 4));
                        //play shot sound
                        PlayCue("Shot");
                        --specialList;

                        //reset the shot count down
                        specialShotCountdown = shotDelay;
                    }
                }
                else
                    specialShotCountdown -= gameTime.ElapsedGameTime.Milliseconds;
                
            }

        }


        public void PlayCue(string cue)
        {
            soundBank.PlayCue(cue);
        }

        public void AddPoints(int points)
        {
            score += points;
        }

        //cansel power up
        private void CancelPowerUps()
        {
            modelManager.consecutiveKills = 0;
            shotDelay = originalShotDely;
        }

        //update the power up
        protected void UpdatePowerUp(GameTime gameTime)
        {
            if (powerUpCountdown > 0)
            {
                powerUpCountdown -= gameTime.ElapsedGameTime.Milliseconds;
                if (powerUpCountdown <= 0)
                {
                    CancelPowerUps();
                    powerUpCountdown = 0;

                }
            }
        }
     
        //Method for madel manager to access the power up
        public void StartPowerUp(PowerUps powerUp)
        {
            switch (powerUp)
            {
                case PowerUps.RAPID_FIRE:
                    shotDelay = shotDelyRapidFire;
                    powerUpCountdown = rapidFireTime;
                    powerUpText = "Rapid Fire Mode!";
                    powerUpTextTimer = 10000;
                    soundBank.PlayCue("RapidFire");
                    break;
            }
        }

            
    }
}
