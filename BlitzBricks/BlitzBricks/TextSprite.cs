using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BlitzBricks
{
    public enum TextAlignment
    {
        Left,
        Right,
        Center
    }
    
    class TextSprite:ColorSprite
    {
        public bool AutoSize { get; set; }
        public TextAlignment Alignment { get; set;}
        public String Text { get; set; }
        public SpriteFont Font { get; private set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Margin { get; set; }
        private Rectangle Bounds;

        public void LoadContent(ContentManager myContentManager, String FontName)
        {
            Font = myContentManager.Load<SpriteFont>(FontName);
        }

        public override void Draw(SpriteBatch mSpriteBatch)
        {
            Draw(mSpriteBatch, Color.White);
        }

        public override void Draw(SpriteBatch mSpriteBatch, Color sColor)
        {
            if (Text == null) { Text = "X"; }
            Bounds = new Rectangle(Left + Margin, Top + Margin, Width - Margin * 2, Height - Margin*2);
            switch (Alignment)
            {
                case TextAlignment.Left:
                    Position = new Vector2(Left, (Top + (Height / 2) - (Font.MeasureString(Text).Y / 2)));
                    break;
                case TextAlignment.Right:
                    Position = new Vector2(Bounds.Left + Bounds.Width - Font.MeasureString(Text).X, (Top +(Height / 2) - (Font.MeasureString(Text).Y/2)));
                    break;
                case TextAlignment.Center:
                     Position = new Vector2(Left + (Width/2)-(Font.MeasureString(Text).X/2) , (Top + (Height / 2) - (Font.MeasureString(Text).Y/2)));
                    break;
            }
            
            mSpriteBatch.DrawString(Font, Text, Position, sColor);
        }

    }
}
