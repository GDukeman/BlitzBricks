using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlitzBricks
{
    class Paddle:ColorSprite
    {
        public int ScreenWidth { get; set; }
        public int InputWidth { get; set; }
        public int InputStart { get; set; }
        public DateTime SpecialTimer { get; set; }
        public Texture2D sTexture { get; set; }
        public int Height
        {
            get
            {
                return Texture.Height;
            }
        }
        public int Width 
        {
            get
            {
                if (SpecialTimer.AddSeconds(10) >= DateTime.Now)
                {
                    return sTexture.Width;
                }
                else
                {
                    return Texture.Width;
                }
            }   
        }
        private int MaxPosX;

        public void SetY(float PosY)
        {
            Position = new Vector2(Position.X,PosY);
        }

        public  void MoveTo(float SPosX)
        {
            float NewPos = 0f;
            try
            {
                if (SpecialTimer.AddSeconds(10) >= DateTime.Now)
                {
                    MaxPosX = ScreenWidth - sTexture.Width;
                    NewPos = (float)((SPosX - InputStart) / InputWidth) * ScreenWidth - sTexture.Width / 2;    
                }
                else
                {
                    MaxPosX = ScreenWidth - Texture.Width;
                    NewPos = (float)((SPosX - InputStart) / InputWidth) * ScreenWidth - Texture.Width / 2;
                }
               
                if (NewPos < 0) { NewPos = 0; }
                if (NewPos > MaxPosX) { NewPos = (float)(MaxPosX); }
                Position = new Vector2(NewPos, Position.Y);
            }
            catch
            {
            }
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            if (SpecialTimer.AddSeconds(10) >= DateTime.Now)
            {
                theSpriteBatch.Draw(sTexture, Position, Color.White);
            }
            else
            {
                theSpriteBatch.Draw(Texture, Position, Color.White);
            }
        }
    }
}
