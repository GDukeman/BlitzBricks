using System;
using Helper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace BlitzBricks
{
    //Ball Class
    //Inherits ColorSprite
    //Ball Class automatically draws the special texture when that ball is in special mode.
    
    class Ball:ColorSprite
    {
        //public Texture2D mSpecialTexture { get; set; }
        //public Texture2D mSpriteTexture { get; set; }
        private static int Segments = 48;
        public bool CanIntersect { get; set; }
        //public int SpecialTimer { get; set; }
        private int R;
        private Brick LastBrick = new Brick();
        public Vector2 MySpeed { get; set; }
        private Vector2 CenterVector;
        private Vector2[] RadialVector = new Vector2[Segments];

        public void RandomSpeed()
        {
            switch ((int)new Random().Next(1, 4))
            {
                case 1:
                    MySpeed = new Vector2(-1f, 1f);
                    break;
                case 2:
                    MySpeed = new Vector2(1f, 1f);
                    break;
                case 3:
                    MySpeed = new Vector2(-1f,-1f);
                    break;
                case 4:
                    MySpeed = new Vector2(1f,-1f);
                    break;
            }

        }
        
        public void Initialize()
        {
            CenterVector.X = Texture.Bounds.Center.X;
            CenterVector.Y = Texture.Bounds.Center.Y;
            R = Texture.Bounds.Width / 2;
            double angle = (360/Segments);
            MySpeed = new Vector2(1f, -1f);

            for (int Z = 0; Z < Segments; Z++)
            {
                RadialVector[Z].X = (int)Math.Round(R * Math.Cos(Trig.DegreeToRadian(angle * Z)));
                RadialVector[Z].Y = (int)Math.Round(R * Math.Sin(Trig.DegreeToRadian(angle * Z)));
            }
        }     
              
        public Vector2 CollideWith(Rectangle NewRect)
        {
            return CollideWith(NewRect, false, 0);
        }
        
        public Vector2 CollideWith(Rectangle NewRect, bool IsPaddle,float Momentum)
        {
            Vector2 CurSpeed = MySpeed;
            Rectangle TempRec = new Rectangle();
            Vector2 TempVect = new Vector2();
            int CX,CY;
            // which corner are we coming after
            if (CurSpeed.X <=0){CX=NewRect.Right;} else {CX=NewRect.Left;}
            if (CurSpeed.Y <=0){CY=NewRect.Bottom;} else {CY=NewRect.Top;}
            Vector2 IPoint = new Vector2(CX,CY);
            Vector2 MaxVect = IPoint;
            //double PP = .5; // paddle point

            for (int Z = 0; Z < Segments; Z++)
            {
                TempVect = CenterVector + RadialVector[Z] + Position;
                TempRec = new Rectangle((int)TempVect.X, (int)TempVect.Y, 1, 1);
                if (NewRect.Intersects(TempRec))
                {
                    if (Vector2.Distance(IPoint, TempVect) > Vector2.Distance(IPoint, MaxVect))
                    {
                        MaxVect = TempVect;
                    }
                }
            }

            if (MaxVect != IPoint)
            {
                if (IsPaddle) //Paddle is special case since always at bottom
                {
                    CurSpeed.X += Momentum;
                    if (CurSpeed.X > 1.5) { CurSpeed.X = 1.5f; }
                    if (CurSpeed.X < -1.5) { CurSpeed.X = -1.5f; }
                    CurSpeed.Y *= -1;
                }
                else if (Vector2.Distance(new Vector2((float)MaxVect.X, 0), new Vector2((float)IPoint.X, 0)) < Vector2.Distance(new Vector2((float)MaxVect.Y, 0), new Vector2((float)IPoint.Y, 0)))
                {
                    CurSpeed.X *= -1;
                }
                else
                {
                    CurSpeed.Y *= -1;
                }
            }
            MySpeed = CurSpeed;
            return MySpeed;
        }
        
    }
}
