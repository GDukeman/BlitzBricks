using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BlitzBricks
{
    public partial class ColorSprite
    {
        //The current position of the Sprite
        public Vector2 Position { get; set; }
        public float PosX 
        {
            get { return Position.X; }
            set
            { 
                Position = new Vector2(value, Position.Y);
                Texture.Bounds.Offset((int)value, (int)Position.Y);
            }
        }
        public float PosY
        {
            get { return Position.Y; }
            set
            {
                Position = new Vector2(Position.X, value);
                Texture.Bounds.Offset((int)Position.X, (int)value);
            }
        }
        public Texture2D Texture { get; set; }
        
        //Draw the sprite to the screen - Calls Overloaded Draw
        public virtual void Draw(SpriteBatch Batch)
        {
            Draw(Batch, Color.White);
        }

        public virtual void Draw(SpriteBatch Batch, Color sColor)
        
        {
            Batch.Draw(Texture, Position, sColor);
        }

        public virtual void Draw(SpriteBatch Batch, Vector2 XScale)
        {
                Batch.Draw(Texture, Position, null,Color.White,0f, Vector2.Zero, XScale, SpriteEffects.None, 0f);
        }
    }
}
