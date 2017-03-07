using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Helper;

namespace BlitzBricks
{

    class Sparkle : ColorSprite
    {

        public Vector2 InitialVector = new Vector2();
        public void NewSparkle(Vector2 StartPosition)
        {
            Random MyRnd = new Random((int)(StartPosition.X * DateTime.Now.Millisecond));
            Position = StartPosition;
            InitialVector = new Vector2((float)(MyRnd.NextDouble() * 2) - 1, (float)(MyRnd.NextDouble() * 2) - 1);

        }

        public void DrawSparkle(SpriteBatch theSpriteBatch, byte FadeCount)
        {
            InitialVector = new Vector2(InitialVector.X + (InitialVector.X *.1f), InitialVector.Y + (InitialVector.Y * .15f));
            Position += InitialVector;
            theSpriteBatch.Draw(Texture, Position, new Color(FadeCount, FadeCount, FadeCount, FadeCount));
        }
    }



    class Brick : ColorSprite, IEquatable<Brick>
    {
        private Sparkle[] MySparkles = new Sparkle[50];
        private Texture2D sTexture;
        public Texture2D SparkleTexture
        {
            get
            {
                return sTexture;
            }
            set
            {
                sTexture = value;
                for (int x = 0; x < 50; x++)
                {
                    MySparkles[x] = new Sparkle();
                    MySparkles[x].Texture = sTexture;
                }
            }

        }
        public Specials Special { get; set; }
        public int BasePointValue { get; set; }
        public bool InPlay { get; set; }
        public byte FadeCount { get; set; }
        public Rectangle BrickRect { get; set; }
        public int HitPoints { get; set; }
        public bool LaserAffects { get; set; }
        public string Name { get; set; }

        public bool Equals(Brick other)
        {
            if (this.Position == other.Position)
                return true;
            else
                return false;
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            if (FadeCount > 0)
            {
                foreach (Sparkle s in MySparkles)
                {
                    s.DrawSparkle(theSpriteBatch, (byte)(FadeCount * 12.75));
                }
                FadeCount--;
            }
            else
            {
                theSpriteBatch.Draw(Texture, Position, Color.White);
            }
        }

        public void SetSparkles()
        {
            for (int x = 0; x < 25; x++)
            {
                MySparkles[x].NewSparkle(new Vector2((float)new Random().Next((int)Texture.Bounds.Center.X+(int)Position.X,(int)Texture.Bounds.Center.X+(int)Position.X+Texture.Width-9),(float)new Random().Next((int)Texture.Bounds.Center.Y+(int)Position.Y,(int)Texture.Bounds.Center.Y+(int)Position.Y+Texture.Height-9)));
            }
        }
    }
}

