using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Tasks;

namespace BlitzBricks
{
    #region Declarations

    
    public enum Specials
    {
        NoEffect,
        ExtraLife,
        TripleThreat,
        Laser,
        PaddleLarge,
        WreckingBall
    }

    public enum GameState
    {
        Splash,         //show splash screen
        Menu,           //show main menu
        GameStart,      //show game at start, ball not moving
        GameRunning,    //show game running as normal
        GamePaused,     //show game paused screen with Continue Game
        GameOver,       //show game paused screen with Try Again
        TrialOver,      //show purchase menu
    }

    public enum UserChoice
    {
        NoChoice,
        MainMenu,
        NewGame,
        ContinueGame,
        BuyApp,
        ChangeSound,
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BlitzBricks : Microsoft.Xna.Framework.Game
    {
        public struct BrickDef
        {
            public string Name;
            public int Points;
            public bool Laserable;
        
            public BrickDef (string BrickName, int BrickPoints, bool BrickLaserable)
            {
                Name = BrickName;
                Points = BrickPoints;
                Laserable = BrickLaserable;
            }
        }
        GameState CurrentState = GameState.Splash;
        UserChoice CurrentChoice = UserChoice.NoChoice;
        GraphicsDeviceManager MyGraphics;
        ContentManager MyContent;
        SpriteBatch MySpriteBatch;
        TextSprite NewGameText;
        TextSprite EndPauseText;
        TextSprite ContinueGameText;
        TextSprite PurchaseText;
        TextSprite ScoreText;
        TextSprite LivesText;
        TextSprite PauseText;
        TextSprite SlideText;
        TextSprite FireText;
        TextSprite Message;
        TextSprite TrialMessage;
        Vector2 BallSpeed = new Vector2(1f, -1f); // initial ball speed - Add two vectors to move the ball
        Vector2 OldSpeed = new Vector2(1f, -1f);
        Vector2 LaserVect = new Vector2(0f, -1.5f);
        Paddle MyPaddle;
        ColorSprite ProgressBar;
        ColorSprite Background;
        ColorSprite SplashScreen;
        ColorSprite PurchaseButton;
        ColorSprite MenuScreen;
        ColorSprite PausedScreen;
        ColorSprite SoundButton;
        List<Brick> Bricks = new List<Brick>();
        List<BrickDef> BrickDefs =  new List<BrickDef>();
        List<Laser> Lasers = new List<Laser>();
        List<Ball> Balls = new List<Ball>();

        SoundEffect SndBallLost;
        SoundEffect SndSpecial;
        SoundEffect SndLaserFire;
        SoundEffect SndPaddleHit;
        SoundEffect SndWallHit;
        SoundEffect SndBrickHit;
        SoundEffect SndBrickBreak;
        SoundEffect SndNextLevel;
        SoundEffect SndSpecialEnd;

        DateTime DrawSplashTimer;

        long Score;
        long Lives = 3;
        static int MaxLasers = 10;  // maximum lasers active at any one time
        int SpeedFactor = 6;        // used to scale the speed of the ball
        int MaxX, MinX, MaxY, MinY, MaxLevel;
        int LaserCount = 0;
        int Multiplier = 0;
        int BrickCount = 0;
        int CurrentLevel = 0;
        float LastPaddleX;
        float Momentum = 0;
        int SlideWidth = 334;
        int SlideStart = 134;
        int ThumbHeight = 180;      //area where thumb or finger go to move paddle
        int InfoHeight = 62;        //area where lives, score, time and achievements are displayed
        int MessageHeight = 30;
        bool ResetLevel = false;
        string LevelName;
        int MessageTimer = 0;
        string SaveGameFile = "savegame.txt";
        bool SoundOn = true;
        bool TrialLicense = Guide.IsTrialMode;
        string LevelPrefix;
        bool FirstCheck = true;
        string CurrentSpecial ="";
        bool LastLicenseTrial = false;
    #endregion

    #region Configuration

        public BlitzBricks()
        {
            MyContent = new ContentManager(Services);
            MyGraphics = new GraphicsDeviceManager(this);
            MyGraphics.PreferredBackBufferWidth = 480;
            MyGraphics.PreferredBackBufferHeight = 800;

            NewGameText = new TextSprite();
            EndPauseText = new TextSprite();
            ContinueGameText = new TextSprite();
            PurchaseText = new TextSprite();
            ScoreText = new TextSprite();
            PauseText = new TextSprite();
            LivesText = new TextSprite();
            SlideText = new TextSprite();
            FireText = new TextSprite();
            SlideText = new TextSprite();
            Message = new TextSprite();
            TrialMessage = new TextSprite();
            
            SoundButton = new ColorSprite();
            PurchaseButton = new ColorSprite();
            MenuScreen = new ColorSprite();
            SplashScreen = new ColorSprite();
            PausedScreen = new ColorSprite();
            MyPaddle = new Paddle();
            Background = new ColorSprite();
            ProgressBar = new ColorSprite();
           
            MyContent.RootDirectory = "Content";
            
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(202020);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //Guide.SimulateTrialMode = true;
            checkTrial();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            MyGraphics.GraphicsDevice.Clear(Color.Black);

            // Create a new SpriteBatch, which can be used to draw textures.
            MySpriteBatch = new SpriteBatch(GraphicsDevice);
            SplashScreen.Texture = MyContent.Load<Texture2D>("Images\\SplashScreen");

            MySpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend); ;
            SplashScreen.Draw(MySpriteBatch);
            MySpriteBatch.End();
            MyGraphics.GraphicsDevice.Present();
            DrawSplashTimer = DateTime.Now;

            PurchaseButton.Texture = MyContent.Load<Texture2D>("Images\\Button");
            PurchaseButton.PosX = 90;
            PurchaseButton.PosY = 547;
            
            MenuScreen.Texture = MyContent.Load<Texture2D>("Images\\MenuScreen");
            PausedScreen.Texture = MyContent.Load<Texture2D>("Images\\PauseScreen");
            
            SoundButton.Texture = MyContent.Load<Texture2D>("Images\\SoundOn");
            SoundButton.PosY = 660;
            SoundButton.PosX = 190;

            Message.LoadContent(MyContent, "Fonts\\SegoeWPMessage");
            Message.Top = InfoHeight + 3;
            Message.Left = 0; 
            Message.Width = 480;
            Message.Height = MessageHeight;
            Message.Alignment = TextAlignment.Center;
            Message.Margin = 2;
            Message.Text = LevelName;

            TrialMessage.LoadContent(MyContent, "Fonts\\SegoeWPMessage");
            TrialMessage.Top = 196;
            TrialMessage.Left = 0;
            TrialMessage.Width = 480;
            TrialMessage.Height = MessageHeight * 4;
            TrialMessage.Alignment = TextAlignment.Center;
            TrialMessage.Margin = 2;
            TrialMessage.Text = "      TRIAL VERSION\n      Buy Now and Play\n More Than 40 Levels &\n     Save Your Progress";

            NewGameText.LoadContent(MyContent, "Fonts\\Control");
            NewGameText.Left = 90;
            NewGameText.Top = 322;
            NewGameText.Width = 300;
            NewGameText.Height = InfoHeight;
            NewGameText.Alignment = TextAlignment.Center;
            NewGameText.Margin = 4;
            NewGameText.Text = "NEW GAME";

            EndPauseText.LoadContent(MyContent, "Fonts\\Control");
            EndPauseText.Left = 90;
            EndPauseText.Top = 318;
            EndPauseText.Width = 300;
            EndPauseText.Height = InfoHeight;
            EndPauseText.Alignment = TextAlignment.Center;
            EndPauseText.Margin = 4;
            EndPauseText.Text = "CONTINUE GAME";

            ContinueGameText.LoadContent(MyContent, "Fonts\\Control");
            ContinueGameText.Left = 90;
            ContinueGameText.Top = 431;
            ContinueGameText.Width = 300;
            ContinueGameText.Height = InfoHeight;
            ContinueGameText.Alignment = TextAlignment.Center;
            ContinueGameText.Margin = 4;
            ContinueGameText.Text = "RESUME GAME";

            PurchaseText.LoadContent(MyContent, "Fonts\\Control");
            PurchaseText.Left = 90;
            PurchaseText.Top = 547;
            PurchaseText.Width = 300;
            PurchaseText.Height = InfoHeight;
            PurchaseText.Alignment = TextAlignment.Center;
            PurchaseText.Margin = 4;
            PurchaseText.Text = "PURCHASE GAME";

            FireText.LoadContent(MyContent, "Fonts\\SegoeWPBlack");
            FireText.Left = 0;
            FireText.Top = 685;
            FireText.Width = 115;
            FireText.Height = InfoHeight;
            FireText.Alignment = TextAlignment.Center;
            FireText.Margin = 2;
            FireText.Text = "FIRE";

            SlideText.LoadContent(MyContent, "Fonts\\SegoeWPBlack");
            SlideText.Left = 122;
            SlideText.Top = 685;
            SlideText.Width = 354;
            SlideText.Height = InfoHeight;
            SlideText.Alignment = TextAlignment.Center;
            SlideText.Margin = 2;
            SlideText.Text = "SLIDE";

            ScoreText.LoadContent(MyContent, "Fonts\\SegoeWPBlack");
            ScoreText.Top = 0;
            ScoreText.Left = 300;
            ScoreText.Width = 170;
            ScoreText.Height = InfoHeight;
            ScoreText.Alignment = TextAlignment.Right;
            ScoreText.Margin = 2;
            ScoreText.Text = Score.ToString();

            LivesText.LoadContent(MyContent, "Fonts\\SegoeWPBlack");
            LivesText.Top = 0;
            LivesText.Left = 70;
            LivesText.Width = 150;
            LivesText.Height = InfoHeight;
            LivesText.Alignment = TextAlignment.Left;
            LivesText.Margin = 2;
            LivesText.Text = Lives.ToString();

            PauseText.LoadContent(MyContent, "Fonts\\SegoeWPBlack");
            PauseText.Text = "PAUSED";
            PauseText.Top = 200;
            PauseText.Left = 0;
            PauseText.Width = 480;
            PauseText.Height = 60;
            PauseText.Alignment = TextAlignment.Center;
            PauseText.Margin = 0;

            MyPaddle.Texture = MyContent.Load<Texture2D>("Images\\PaddleNormal");
            MyPaddle.sTexture = MyContent.Load<Texture2D>("Images\\PaddleLarge");
            MyPaddle.Position = new Vector2((MyGraphics.GraphicsDevice.Viewport.Width / 2) - MyPaddle.Texture.Width/2, MyGraphics.GraphicsDevice.Viewport.Height - (ThumbHeight + MyPaddle.Texture.Height));
            MyPaddle.ScreenWidth = MyGraphics.GraphicsDevice.Viewport.Width - 4;
            MyPaddle.InputStart = SlideStart;
            MyPaddle.InputWidth = SlideWidth;

            AddBall(false,new Vector2(0,0));
            Balls.First().Position = new Vector2(MyPaddle.Position.X + MyPaddle.Texture.Width / 2 - Balls.First().Texture.Width / 2, MyPaddle.Position.Y - (Balls.First().Texture.Height + 2));

            ProgressBar.Texture = MyContent.Load<Texture2D>("Images\\ProgressBar");
            ProgressBar.PosX = 0;
            ProgressBar.PosY = 66;
            Background.Texture = MyContent.Load<Texture2D>("Images\\NewBackground");

            //Configure Bounding Area
            MaxX = MyGraphics.GraphicsDevice.Viewport.Width - (Balls.First().Texture.Bounds.Width + 2);
            MinX = 2;
            MaxY = MyGraphics.GraphicsDevice.Viewport.Height - ThumbHeight;
            MinY = InfoHeight + 2 + MessageHeight;
            
            //Load Sounds
            SndPaddleHit = MyContent.Load<SoundEffect>("Sounds\\PaddleHit");
            SndWallHit = MyContent.Load<SoundEffect>("Sounds\\WallHit");
            SndLaserFire = MyContent.Load<SoundEffect>("Sounds\\LaserFiring");
            SndBallLost = MyContent.Load<SoundEffect>("Sounds\\balllost3");
            SndBrickHit = MyContent.Load<SoundEffect>("Sounds\\BrickHit");
            SndSpecial = MyContent.Load<SoundEffect>("Sounds\\Special");
            SndBrickBreak = MyContent.Load<SoundEffect>("Sounds\\BrickBreak");
            SndNextLevel = MyContent.Load<SoundEffect>("Sounds\\NextLevel");
            SndSpecialEnd = MyContent.Load<SoundEffect>("Sounds\\SpecialEnd");
            
            CreateBrickDefs();
            CreateLevel();
        }

        //methods 
        public bool checkTrial()
        {
            if (Guide.IsTrialMode)
            {
                //sets game to trial mode 
                TrialLicense = true;
                LevelPrefix = "Trial";
                return true;
            }
            TrialLicense = false;
            LevelPrefix = "Level";
            return false;
        }

        public void buyGame()
        {
            Guide.ShowMarketplace(PlayerIndex.One);
            checkTrial();  
        } 

        public bool ExistsSavedGame(string SaveGameFile)
        {
            bool res = false;
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(SaveGameFile))
                {
                    res = true;
                }
            }
            return res;
        }

        public void LoadFromIsolatedStorage(string SaveGameFile)
        {
            // Load from Isolated Storage file
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = isolatedStorageFile.OpenFile(SaveGameFile, FileMode.Open))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        Score = Int32.Parse(streamReader.ReadLine(), System.Globalization.NumberStyles.Integer);
                        Lives = Int32.Parse(streamReader.ReadLine(), System.Globalization.NumberStyles.Integer);
                        CurrentLevel = Int32.Parse(streamReader.ReadLine(), System.Globalization.NumberStyles.Integer);
                        LastLicenseTrial = bool.Parse(streamReader.ReadLine());
                        streamReader.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Saves the gameplay screen data to Isolated storage file/// </summary>
        /// <param name="fileName"></param>
        public void SaveToIsolatedStorageFile(string SaveGameFile)
        {
            // Save to Isolated Storage file
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                isolatedStorageFile.DeleteFile(SaveGameFile);//clear old one
                // If user choose to save, create a new file
                using (IsolatedStorageFileStream fileStream = isolatedStorageFile.CreateFile(SaveGameFile))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        // Write date to the file 
                        streamWriter.WriteLine(Score);
                        streamWriter.WriteLine(Lives);
                        streamWriter.WriteLine(CurrentLevel);
                        streamWriter.WriteLine(Guide.IsTrialMode);
                        streamWriter.Close();
                    }
                }
            }
        }

        private void CreateBrickDefs()
        {
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create("LevelData\\BrickDefs.xml"))
            {
                reader.MoveToContent();
                reader.ReadToFollowing("MaxLevel");
                if (TrialLicense)
                {
                    MaxLevel = 2;
                }
                else
                {
                    MaxLevel = reader.ReadElementContentAsInt();
                }
                reader.ReadToFollowing("BrickDefs");
                while (reader.ReadToFollowing("Def"))
                {
                    BrickDef bd = new BrickDef();
                    reader.ReadToFollowing("BDName");
                    bd.Name = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Points");
                    bd.Points = reader.ReadElementContentAsInt();
                    reader.ReadToFollowing("Laser");
                    bd.Laserable = reader.ReadElementContentAsBoolean();
                    BrickDefs.Add(bd);
                }
            }
        }

        private void CreateLevel()
        {
            Bricks.Clear();
            string bdf;
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create("LevelData\\" + LevelPrefix + CurrentLevel.ToString("00") + ".xml"))
            {
                reader.MoveToContent();
                while (reader.ReadToFollowing("Level"))
                {
                    reader.ReadToFollowing("Name");
                    LevelName = reader.ReadElementContentAsString();
                    Message.Text = LevelName;
                    while (reader.ReadToFollowing("Brick"))
                    {                  
                        Brick b = new Brick();
                        reader.ReadToFollowing("BName");
                        bdf = reader.ReadElementContentAsString();
                        foreach (BrickDef bd in BrickDefs)
                        {
                            if (bd.Name == bdf)
                            {
                                b.Texture = MyContent.Load<Texture2D>("Images\\" + bd.Name);
                                b.Name = bd.Name;
                                b.LaserAffects = bd.Laserable;
                                b.BasePointValue = bd.Points;
                                b.SparkleTexture = MyContent.Load<Texture2D>("Images\\Sparkle");
                                b.FadeCount = 0;
                            }
                        }
                        reader.ReadToFollowing("Row");
                        b.PosY = (reader.ReadElementContentAsInt() * b.Texture.Height) + MinY;
                        reader.ReadToFollowing("Col");
                        b.PosX = reader.ReadElementContentAsFloat() * b.Texture.Width + MinX;
                        b.BrickRect = new Rectangle((int)b.PosX, (int)b.PosY, b.Texture.Width, b.Texture.Height);
                        reader.ReadToFollowing("HP");
                        b.HitPoints = reader.ReadElementContentAsInt();
                        reader.ReadToFollowing("Special");
                        b.Special = (Specials)Enum.Parse(typeof(Specials), reader.ReadElementContentAsString(), false);
                        b.InPlay = true;
                        b.SetSparkles();
                        Bricks.Add(b);
                    }
                }
            }
        }    
        #endregion

        protected override void Update(GameTime gameTime)
        {
            if (CurrentState == GameState.GameRunning)
            {
                for (int i = 0; i < 1; i++) { }
            }
            // Allow the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                CurrentChoice = UserChoice.NoChoice;
                switch (CurrentState)
                {
                    case GameState.Splash:
                        this.Exit();
                        break;

                    case GameState.Menu:
                        this.Exit();
                        break;

                    case GameState.GameStart:
                        CurrentState = GameState.Menu;
                        break;

                    case GameState.GameRunning:
                        CurrentState = GameState.GamePaused;
                        break;

                    case GameState.GamePaused:
                        CurrentState = GameState.Menu;
                        break;
                }
            }

            switch (CurrentState)
            {
                case GameState.Menu:
                HandleInputMultiTouch();
                    
                switch (CurrentChoice)
                {
                    case UserChoice.NewGame:
                        CurrentState = GameState.GameStart;
                        CurrentChoice = UserChoice.NoChoice;
                        CurrentLevel = -1;
                        Score = 0;
                        GotoNextLevel();
                        break;

                    case UserChoice.ContinueGame:
                        if (ExistsSavedGame(SaveGameFile) && FirstCheck)
                        {
                            LoadFromIsolatedStorage(SaveGameFile);
                            CurrentState = GameState.GameStart;
                            CurrentLevel --;
                            GotoNextLevel();
                        }
                        else
                        {
                            CurrentState = GameState.GameRunning;
                        }
                        CurrentChoice = UserChoice.NoChoice;
                        break;

                    case UserChoice.BuyApp:
                        buyGame();
                        CurrentChoice = UserChoice.NoChoice;
                        break;

                    case UserChoice.ChangeSound:
                        SoundOn = !SoundOn;
                        if (SoundOn)
                        {
                            SoundEffect.MasterVolume = 1f;
                            SoundButton.Texture = MyContent.Load<Texture2D>("Images\\SoundOn");                       
                        }
                        else
                        {
                            SoundEffect.MasterVolume = 0f;
                            SoundButton.Texture = MyContent.Load<Texture2D>("Images\\SoundOff");
                        }
                        CurrentChoice = UserChoice.NoChoice;
                        break;
                }
                break;

            case GameState.TrialOver:
                HandleInputMultiTouch();
                switch (CurrentChoice)
                {
                    case UserChoice.NewGame:
                        CurrentState = GameState.GameStart;
                        CurrentChoice = UserChoice.NoChoice;
                        CurrentLevel = -1;
                        Score = 0;
                        GotoNextLevel();
                        break;

                    case UserChoice.ContinueGame:
                        CurrentState = GameState.GameStart;
                        CurrentChoice = UserChoice.NoChoice;
                        CurrentLevel = -1;
                        Score=0;
                        GotoNextLevel();
                        break;

                    case UserChoice.BuyApp:
                        buyGame();
                        CurrentChoice = UserChoice.NoChoice;
                        break;

                    case UserChoice.ChangeSound:
                        SoundOn = !SoundOn;
                        if (SoundOn)
                        {
                            SoundEffect.MasterVolume = 1f;
                            SoundButton.Texture = MyContent.Load<Texture2D>("Images\\SoundOn");
                        }
                        else
                        {
                            SoundEffect.MasterVolume = 0f;
                            SoundButton.Texture = MyContent.Load<Texture2D>("Images\\SoundOff");
                        }
                        CurrentChoice = UserChoice.NoChoice;
                        break;
                }
                break;

            case GameState.GameStart:
                HandleInputMultiTouch();
                break;

            case GameState.GameOver:
                HandleInputMultiTouch();
                switch (CurrentChoice)
                {
                    case UserChoice.ContinueGame:
                        CurrentState = GameState.GameStart;
                        CurrentChoice = UserChoice.NoChoice;
                        ResetLevel = true;
                        Score = 0;
                        Lives = 4;
                        break;

                    case UserChoice.MainMenu:
                        CurrentState = GameState.Menu;
                        CurrentChoice = UserChoice.NoChoice;
                        ResetLevel = false;
                        break;
                }
                break;

            case GameState.GameRunning:
                FirstCheck = false;
                HandleInputMultiTouch();
                CheckForCollision();
                break;
                
            case GameState.GamePaused:
                HandleInputMultiTouch();
                switch (CurrentChoice)
                {
                    case UserChoice.ContinueGame:
                        CurrentState = GameState.GameRunning;
                        CurrentChoice = UserChoice.NoChoice;
                        break;

                    case UserChoice.MainMenu:
                        CurrentState = GameState.Menu;
                        CurrentChoice = UserChoice.NoChoice;
                        break;
                }
                break;
            }

            if (ResetLevel)
            {
                if (Lives > 0)
                {
                    LaserCount = 0;
                    MessageTimer = 0;
                    StartBall();
                }
                else
                {
                    CurrentState = GameState.GameOver;
                }
            }        
            base.Update(gameTime);
        }
        
        private void StartBall()
        {
            CurrentState = GameState.GameStart;
            Lives -= 1;
            Lasers.Clear();
            LaserCount = 0;
            Multiplier = 0;
            Balls.Clear();
            Message.Text = LevelName;
            LivesText.Text = Lives.ToString();
            AddBall(false,new Vector2(0,0));
            MyPaddle.Position = new Vector2((MyGraphics.GraphicsDevice.Viewport.Width / 2) - (MyPaddle.Texture.Width / 2), MyGraphics.GraphicsDevice.Viewport.Height - (ThumbHeight + MyPaddle.Texture.Height));
            Balls.First().PosX = MyPaddle.Position.X + MyPaddle.Texture.Width / 2 - Balls.First().Texture.Width / 2;
            Balls.First().PosY = MyPaddle.Position.Y - (Balls.First().Texture.Height + 2);
            ResetLevel = false;
        }

        private void GotoNextLevel()
        {
            if (CurrentLevel < MaxLevel)
            {
                SndNextLevel.Play();
                CurrentLevel++; //Clear screen, load new level and reset ball
                SaveToIsolatedStorageFile(SaveGameFile);
                CreateLevel();
                Lives += 1;
                StartBall();
            }
            else if (TrialLicense)
            {
                CurrentState = GameState.TrialOver;    
            }
            else
            {
                //start over and keep going
                CurrentLevel = -1;
                GotoNextLevel();
            }
        }

        private void FireLaser()
        {
            SndLaserFire.Play();
            Laser TempLaser = new Laser();
            TempLaser.Texture = MyContent.Load<Texture2D>("Images\\LaserBeam");
            TempLaser.Position = new Vector2(MyPaddle.Position.X + MyPaddle.Texture.Width - 20, MyPaddle.Position.Y);
            Lasers.Add(TempLaser);

            TempLaser = new Laser();
            TempLaser.Texture = MyContent.Load<Texture2D>("Images\\LaserBeam");
            TempLaser.Position = new Vector2(MyPaddle.Position.X + 20, MyPaddle.Position.Y);
            Lasers.Add(TempLaser);
            LaserCount--;
        }

        private void EndLaser(Laser theLaser)
        {
            Lasers.Remove(theLaser);
        }

        private void ActivateEffect(Specials Spec)
        {
            SndSpecial.Play();
            MessageTimer = 495;
            switch (Spec)
            {
                case Specials.Laser:
                        CurrentSpecial = "LASERS";
                        LaserCount = 20;
                        break;
                
                case Specials.TripleThreat:
                    //add balls to ball list
                        for (int x = 1; x <= 3; x++)
                        {
                            AddBall(true, Balls.First().Position);
                        }
                        CurrentSpecial = "TRIPLE THREAT";
                        break;
                
                case Specials.PaddleLarge:
                        MyPaddle.SpecialTimer = DateTime.Now;
                        CurrentSpecial = "B.F.P. (Big Friggin' Paddle)";
                        break;
                
                case Specials.ExtraLife:
                        Lives++;
                        CurrentSpecial = "EXTRA LIFE";
                        break;
            }
        }
        
        //move this into a Specials Calls with static functions
        private void AddBall(bool RandomVector,Vector2 NewPosition) 
        {
            Ball MyBall = new Ball();
            MyBall.Texture = MyContent.Load<Texture2D>("Images\\newball");
            //MyBall.SpecialTimer = 0;
            MyBall.Initialize();
            MyBall.Position = NewPosition;
            if (RandomVector) {MyBall.RandomSpeed();}
            Balls.Add(MyBall);
        }

        private void CheckForCollision()
        {
            BrickCount = 0;
            foreach(Brick b in Bricks)
            {
                if (b.InPlay && (b.Name != "IronBrick" & b.Name != "GlassBrick" & b.Name != "StoneBrick")) { BrickCount++; }
            }
            //Cleared Level?
            if (BrickCount < 1) { GotoNextLevel(); }
            
                foreach (Ball bl in Balls)
                {
                    // Check for bounce.
                    if (bl.Position.X > MaxX)
                    {
                        bl.MySpeed = new Vector2(bl.MySpeed.X * -1, bl.MySpeed.Y);
                        bl.Position = new Vector2(MaxX, bl.Position.Y);
                        SndWallHit.Play();
                    }
                    else if (bl.Position.X < 0)
                    {
                        bl.MySpeed = new Vector2(bl.MySpeed.X * -1, bl.MySpeed.Y);
                        bl.Position = new Vector2(MinX, bl.Position.Y);
                        SndWallHit.Play();
                    }
                    if (bl.Position.Y + bl.Texture.Height > MaxY)
                    {
                        if (Balls.Count == 1)
                        {
                            ResetLevel = true;
                            SndBallLost.Play();
                        }
                        else
                        {
                            Balls.Remove(bl);
                        }
                        return;
                    }
                    else if (bl.Position.Y < MinY)
                    {
                        bl.MySpeed = new Vector2(bl.MySpeed.X, bl.MySpeed.Y * -1);
                        bl.Position = new Vector2(bl.Position.X, MinY);
                        SndWallHit.Play();
                    }

                    // Check for Brick Intersection
                    foreach (Brick b in Bricks)
                    {
                        if (b.InPlay)
                        {
                            if (b.BrickRect.Intersects(new Rectangle((int)bl.PosX, (int)bl.PosY, bl.Texture.Width, bl.Texture.Height)))
                            {
                                OldSpeed = bl.MySpeed;
                                if (OldSpeed != bl.CollideWith(b.BrickRect))
                                {
                                    if (b.HitPoints == 1)
                                    {
                                        SndBrickBreak.Play();
                                        b.InPlay = false;
                                        b.FadeCount = 20;
                                        Multiplier++;
                                        Score += b.BasePointValue*Multiplier;
                                        ScoreText.Text = Score.ToString();
                                        bl.Position += (bl.MySpeed * SpeedFactor);
                                        if (b.Special != Specials.NoEffect) { ActivateEffect(b.Special); }
                                    }
                                    else
                                    {
                                        SndBrickHit.Play();
                                        b.HitPoints -= 1;
                                    
                                    }
                                    bl.Position += (bl.MySpeed * SpeedFactor);
                                    return;
                                }
                            }
                        
                    

                        }
                    }


                // Check for Paddle Intersection
                OldSpeed = bl.MySpeed;
                Rectangle PaddleRect = new Rectangle((int)MyPaddle.Position.X, (int)MyPaddle.Position.Y, MyPaddle.Width, MyPaddle.Height);
                if (OldSpeed != bl.CollideWith(PaddleRect, true, Momentum))
                {
                    bl.Position = new Vector2(bl.Position.X, (MyPaddle.Position.Y - bl.Texture.Height) - 1);
                    if (Balls.Count == 1) { Multiplier = 0; } //Turn off multiplier only if one ball is present
                    SndPaddleHit.Play();
                }

                bl.Position += (bl.MySpeed * SpeedFactor);
            }

            if (Lasers.Count > 0)
            {
                foreach (Brick b in Bricks)
                {
                    if (b.InPlay)
                    {
                        try
                        {
                            foreach (Laser l in Lasers)
                            {
                                if (b.BrickRect.Intersects(new Rectangle((int)l.Position.X, (int)l.Position.Y, l.Texture.Width, l.Texture.Height)))
                                {
                                    if (b.HitPoints <= 20 & b.LaserAffects)
                                    {
                                        SndBrickBreak.Play();
                                        b.InPlay = false;
                                        b.FadeCount = 10;
                                        Score += b.BasePointValue;
                                        ScoreText.Text = Score.ToString();
                                        if (b.Special != Specials.NoEffect) { ActivateEffect(b.Special); }
                                        EndLaser(l);
                                    }
                                    else if (b.LaserAffects)
                                    {
                                        SndBrickHit.Play();
                                        b.HitPoints -= 20;
                                        EndLaser(l);
                                    }
                                }
                                else
                                {
                                    if (l.Position.Y < MinY)
                                    {
                                        EndLaser(l);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

        try
        {
            foreach (Laser l in Lasers)
            {
                l.Position += (LaserVect * SpeedFactor * 2);
            }
        }
        catch { }

    }
                
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            MyGraphics.GraphicsDevice.Clear(Color.Black);
            MySpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            switch (CurrentState)
            {
                case GameState.Splash:
                    SplashScreen.Draw(MySpriteBatch);
                    if (DrawSplashTimer.AddMilliseconds(3000) < DateTime.Now) CurrentState = GameState.Menu;
                    break;

                case GameState.Menu:
                    MenuScreen.Draw(MySpriteBatch);
                    NewGameText.Draw(MySpriteBatch);
                    ContinueGameText.Draw(MySpriteBatch);
                    SoundButton.Draw(MySpriteBatch);
                    if (TrialLicense)
                    {
                        TrialMessage.Draw(MySpriteBatch);
                        PurchaseButton.Draw(MySpriteBatch);
                        PurchaseText.Draw(MySpriteBatch);
                    }
                    break;

                case GameState.TrialOver:
                    MenuScreen.Draw(MySpriteBatch);
                    TrialMessage.Draw(MySpriteBatch);
                    NewGameText.Draw(MySpriteBatch);
                    ContinueGameText.Draw(MySpriteBatch);
                    SoundButton.Draw(MySpriteBatch);
                    PurchaseButton.Draw(MySpriteBatch);
                    PurchaseText.Draw(MySpriteBatch);
                    break;
                
                case GameState.GameStart:
                    Message.Text = "Posistion Paddle and Press Fire to Start";
                    Background.Draw(MySpriteBatch);
                    FireText.Draw(MySpriteBatch);
                    Message.Draw(MySpriteBatch);
                    ScoreText.Draw(MySpriteBatch);
                    LivesText.Draw(MySpriteBatch);
                    
                    foreach (Ball bl in Balls)
                    {
                        bl.Draw(MySpriteBatch);
                    }
                    
                    foreach (Brick b in Bricks)
                    {
                        if (b.InPlay|b.FadeCount>0)
                        {
                            b.Draw(MySpriteBatch);
                        }
                    }
                    
                    MyPaddle.Draw(MySpriteBatch);
                    break;

                case GameState.GameRunning:
                    Background.Draw(MySpriteBatch);
                    Message.Text = LevelName;
                    if (LaserCount > 0)
                    {
                        ProgressBar.Draw(MySpriteBatch, new Vector2((float)LaserCount / 20, 1));
                        Message.Text = "LASERS " + LaserCount.ToString();
                        FireText.Draw(MySpriteBatch);
                    }
                    else if (MessageTimer > 0)
                    {
                        Message.Text = CurrentSpecial;
                        MessageTimer--;
                        if(MessageTimer == 0){SndSpecialEnd.Play();}
                    }
                    
                    Message.Draw(MySpriteBatch);
                    ScoreText.Draw(MySpriteBatch);
                    LivesText.Draw(MySpriteBatch);
                    SlideText.Draw(MySpriteBatch);
                    
                    foreach (Laser l in Lasers)
                    {
                        l.Draw(MySpriteBatch);
                    }
                    
                    foreach (Ball bl in Balls)
                    {
                        bl.Draw(MySpriteBatch);
                    }
                    
                    foreach (Brick b in Bricks)
                    {
                        if (b.InPlay|b.FadeCount>0)
                        {
                            b.Draw(MySpriteBatch);
                        }
                    }
                    
                    MyPaddle.Draw(MySpriteBatch);
                    break;

                case GameState.GamePaused:
                    Background.Draw(MySpriteBatch);
                    if (LaserCount > 0)
                    {
                        ProgressBar.Draw(MySpriteBatch, new Vector2((float)LaserCount / 20, 1));
                        Message.Text = "LASERS " + LaserCount.ToString();
                        FireText.Draw(MySpriteBatch);
                    }
                    else
                    {
                        Message.Text = LevelName;
                    }
                    
                    Message.Draw(MySpriteBatch);
                    ScoreText.Draw(MySpriteBatch);
                    LivesText.Draw(MySpriteBatch);
                    SlideText.Draw(MySpriteBatch);

                    foreach (Laser l in Lasers)
                    {
                        l.Draw(MySpriteBatch);
                    }
                    
                    foreach (Ball bl in Balls)
                    {
                        bl.Draw(MySpriteBatch);
                    }
                    
                    foreach (Brick b in Bricks)
                    {
                        if (b.InPlay|b.FadeCount>0)
                        {
                            b.Draw(MySpriteBatch);
                        }
                    }
                    
                    MyPaddle.Draw(MySpriteBatch);
                    PausedScreen.Draw(MySpriteBatch);
                    EndPauseText.Text = "RESUME GAME";
                    EndPauseText.Draw(MySpriteBatch);
                    PauseText.Text = "PAUSED";
                    PauseText.Draw(MySpriteBatch);
                    break;

                case GameState.GameOver:
                    Background.Draw(MySpriteBatch);
                    if (LaserCount > 0)
                    {
                        ProgressBar.Draw(MySpriteBatch, new Vector2((float)LaserCount / 20, 1));
                        Message.Text = "LASERS " + LaserCount.ToString();
                        FireText.Draw(MySpriteBatch);
                    }
                    else
                    {
                        Message.Text = LevelName;
                    }
                    
                    Message.Draw(MySpriteBatch);
                    ScoreText.Draw(MySpriteBatch);
                    LivesText.Draw(MySpriteBatch);
                    SlideText.Draw(MySpriteBatch);

                    foreach (Laser l in Lasers)
                    {
                        l.Draw(MySpriteBatch);
                    }
                    
                    foreach (Ball bl in Balls)
                    {
                        bl.Draw(MySpriteBatch);
                    }
                    
                    foreach (Brick b in Bricks)
                    {
                        if (b.InPlay|b.FadeCount>0)
                        {
                            b.Draw(MySpriteBatch);
                        }
                    }
                    
                    MyPaddle.Draw(MySpriteBatch);
                    PausedScreen.Draw(MySpriteBatch);
                    EndPauseText.Text = "TRY AGAIN";
                    EndPauseText.Draw(MySpriteBatch);
                    PauseText.Text = "GAME OVER";
                    PauseText.Draw(MySpriteBatch);
                    break;
            }
            MySpriteBatch.End(); 
            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Handles touch inputs 
        /// </summary>
        private void HandleInputMultiTouch()
        {
            TouchCollection touchCollection = TouchPanel.GetState();
            switch (CurrentState)
            {
                case GameState.Splash:
                    if (touchCollection.Count > 0) CurrentState = GameState.Menu;
                    break;
                    
                case GameState.Menu:
                    foreach (TouchLocation touchLoc in touchCollection)
                    {
                        if (touchLoc.State == TouchLocationState.Released)
                        {
                            Rectangle NewGameRect = new Rectangle(90, 322, 300, 64);
                            Rectangle ContinueRect = new Rectangle(90, 431, 300, 64);
                            Rectangle SoundCheck = new Rectangle(190, 660, 100, 64);
                            Rectangle TouchRect = new Rectangle((int)touchLoc.Position.X, (int)touchLoc.Position.Y, 1, 1);

                            if (TouchRect.Intersects(NewGameRect))
                            {
                                CurrentChoice = UserChoice.NewGame;
                            }

                            if (TouchRect.Intersects(ContinueRect))
                            {
                                CurrentChoice = UserChoice.ContinueGame;
                            }
                            if (TrialLicense)
                            {
                                Rectangle BuyMe = new Rectangle(90, 547, 300, 64);
                                if (TouchRect.Intersects(BuyMe))
                                {
                                    CurrentChoice = UserChoice.BuyApp;
                                }
                            }
                            if (TouchRect.Intersects(SoundCheck))
                            {
                                CurrentChoice = UserChoice.ChangeSound;
                            }
                        }
                    }
                    break;

                case GameState.TrialOver:
                    foreach (TouchLocation touchLoc in touchCollection)
                    {
                        if (touchLoc.State == TouchLocationState.Released)
                        {
                            Rectangle NewGameRect = new Rectangle(90, 322, 300, 64);
                            Rectangle ContinueRect = new Rectangle(90, 431, 300, 64);
                            Rectangle SoundCheck = new Rectangle(190, 660, 100, 64);
                            Rectangle BuyMe = new Rectangle(90, 547, 300, 64);
                            Rectangle TouchRect = new Rectangle((int)touchLoc.Position.X, (int)touchLoc.Position.Y, 1, 1);

                            if (TouchRect.Intersects(NewGameRect))
                            {
                                CurrentChoice = UserChoice.NewGame;
                            }
                            if (TouchRect.Intersects(ContinueRect))
                            {
                                CurrentChoice = UserChoice.ContinueGame;
                            }
                            if (TouchRect.Intersects(BuyMe))
                            {
                                CurrentChoice = UserChoice.BuyApp;
                            }
                            if (TouchRect.Intersects(SoundCheck))
                            {
                                CurrentChoice = UserChoice.ChangeSound;
                            }
                        }
                    }
                    break;
                
                case GameState.GameStart:
                    
                    foreach (TouchLocation touchLoc in touchCollection)
                    {
                        if (touchLoc.Position.X < 124 & touchLoc.State == TouchLocationState.Pressed)
                        {
                            Message.Text = LevelName;
                            CurrentState = GameState.GameRunning;
                        }
                        if (touchLoc.Position.X > SlideStart & touchLoc.Position.X < SlideStart + SlideWidth & touchLoc.State == TouchLocationState.Moved)
                        {
                            MyPaddle.MoveTo(touchLoc.Position.X);
                            Balls.First().PosX = MyPaddle.Position.X + MyPaddle.Texture.Width / 2 - Balls.First().Texture.Width / 2;
                        }
                    }
                    break;

                case GameState.GameRunning:

                    foreach (TouchLocation touchLoc in touchCollection)
                    {
                        if (touchLoc.Position.Y < MaxY - 20 & touchLoc.State == TouchLocationState.Pressed)
                        {
                            CurrentState = GameState.GamePaused;
                        }
                        else if (touchLoc.Position.Y > MaxY)
                        {
                            if (touchLoc.Position.X < 124 & LaserCount > 0 & Lasers.Count < MaxLasers & touchLoc.State == TouchLocationState.Pressed)
                            {
                                FireLaser();
                            }
                            if (touchLoc.Position.X > SlideStart & touchLoc.Position.X < SlideStart + SlideWidth & touchLoc.State == TouchLocationState.Moved)
                            {
                                MyPaddle.MoveTo(touchLoc.Position.X);

                                if (touchLoc.Position.X > LastPaddleX)
                                {
                                    Momentum += (float).15;
                                }
                                else if (touchLoc.Position.X < LastPaddleX)
                                {
                                    Momentum -= (float).15;
                                }
                                LastPaddleX = touchLoc.Position.X;
                            }
                        }
                    }
                    if (Momentum < 0)
                    {
                        Momentum += (float).05;
                    }
                    else if (Momentum > 0)
                    {
                        Momentum -= (float).05;
                    }
                    break;

                case GameState.GamePaused:
                    foreach (TouchLocation touchLoc in touchCollection)
                    {
                        if (touchLoc.State == TouchLocationState.Released)
                        {
                            Rectangle ContinueRect = new Rectangle(90, 318, 300, 64);
                            Rectangle MainMenuRect = new Rectangle(140, 477, 200, 43);
                            Rectangle TouchRect = new Rectangle((int)touchLoc.Position.X, (int)touchLoc.Position.Y, 1, 1);
                            if (TouchRect.Intersects(MainMenuRect))
                            {
                                CurrentChoice = UserChoice.MainMenu;
                            }

                            if (TouchRect.Intersects(ContinueRect))
                            {
                                CurrentChoice = UserChoice.ContinueGame;
                            }
                        }
                    }
                    break;

                case GameState.GameOver:
                    foreach (TouchLocation touchLoc in touchCollection)
                    {
                        if (touchLoc.State == TouchLocationState.Released)
                        {
                            Rectangle ContinueRect = new Rectangle(90, 318, 300, 64);
                            Rectangle MainMenuRect = new Rectangle(140, 477, 200, 43);
                            Rectangle TouchRect = new Rectangle((int)touchLoc.Position.X, (int)touchLoc.Position.Y, 1, 1);
                            
                            if (TouchRect.Intersects(MainMenuRect))
                            {
                                CurrentChoice = UserChoice.MainMenu;
                            }

                            if (TouchRect.Intersects(ContinueRect))
                            {
                                CurrentChoice = UserChoice.ContinueGame;
                            }
                        }
                    }
                    break;

            }
        }
        
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here

        }
        
    }
}