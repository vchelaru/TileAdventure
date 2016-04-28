#region Usings

using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;

using FlatRedBall.Math.Geometry;
using FlatRedBall.Math.Splines;
using BitmapFont = FlatRedBall.Graphics.BitmapFont;
using Cursor = FlatRedBall.Gui.Cursor;
using GuiManager = FlatRedBall.Gui.GuiManager;
using FlatRedBall.TileCollisions;
using Microsoft.Xna.Framework;

#if FRB_XNA || SILVERLIGHT
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

#endif
#endregion

namespace TileAdventure.Entities
{
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }


	public partial class Character
	{

        public I2DInput MovementInput { get; set; }

        public bool isMovingToTile = false;


        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        private void CustomInitialize()
		{
            ForwardCollision.Position = this.Position;
            BackwardCollision.Position = this.Position;

#if DEBUG
            ForwardCollision.Visible = BackwardCollision.Visible = true;
            ForwardCollision.Color = Color.Green;
            BackwardCollision.Color = Color.Red;
#endif
        }

        private void CreateCollision()
        {
        }

        private void CustomActivity()
		{

		}

        public void PerformMovementActivity(TileShapeCollection collision)
        {
            var desiredDirection = GetDesiredDirection();

            bool startedMoving = ApplyDesiredDirectionToMovement(desiredDirection, collision);

            ApplyDesiredDirectionToAnimation(desiredDirection, startedMoving);
        }

        private void ApplyDesiredDirectionToAnimation(Direction desiredDirection, bool startedMoving)
        {
            bool shouldFaceDirection = startedMoving ||
                // This means the user is facing a collision area
                (desiredDirection != Direction.None && this.isMovingToTile == false);
            if (shouldFaceDirection)
            {
                switch (desiredDirection)
                {
                    case Direction.Left: SpriteInstance.CurrentChainName = "WalkLeft"; break;
                    case Direction.Right: SpriteInstance.CurrentChainName = "WalkRight"; break;
                    case Direction.Up: SpriteInstance.CurrentChainName = "WalkUp"; break;
                    case Direction.Down: SpriteInstance.CurrentChainName = "WalkDown"; break;
                }
            }

            // If standing still, don't animate.
            this.SpriteInstance.Animate = isMovingToTile;
        }

        private bool ApplyDesiredDirectionToMovement(Direction desiredDirection, TileShapeCollection collision)
        {
            bool movedNewDirection = false;

            const int tileSize = 16;


            if(isMovingToTile == false && desiredDirection != Direction.None)
            {
                float desiredX = this.X;
                float desiredY = this.Y;
                
                switch(desiredDirection)
                {
                    case Direction.Left: desiredX -= tileSize; break;
                    case Direction.Right: desiredX += tileSize; break;
                    case Direction.Up: desiredY += tileSize; break;
                    case Direction.Down: desiredY -= tileSize; break;
                }
                float timeToTake = tileSize / MovementSpeed;

                this.ForwardCollision.X = desiredX;
                this.ForwardCollision.Y = desiredY;

                bool isBlocked = collision.CollideAgainst(ForwardCollision);

                if (isBlocked)
                {
                    // move the collision back so it occupies the character's tile
                    this.ForwardCollision.Position = this.Position;
                }
                else
                {
                    InstructionManager.MoveToAccurate(this, desiredX, desiredY, this.Z, timeToTake);
                    isMovingToTile = true;
                    this.Set(nameof(isMovingToTile)).To(false).After(timeToTake);
                    this.Call(() => BackwardCollision.Position = this.Position).After(timeToTake);
                    movedNewDirection = true;
                }
            }

            return movedNewDirection;
        }

        private Direction GetDesiredDirection()
        {
            Direction desiredDirection = Direction.None;

            if (MovementInput != null)
            {
                if (MovementInput.X < 0)
                {
                    desiredDirection = Direction.Left;
                }
                else if (MovementInput.X > 0)
                {
                    desiredDirection = Direction.Right;
                }
                else if (MovementInput.Y < 0)
                {
                    desiredDirection = Direction.Down;
                }
                else if (MovementInput.Y > 0)
                {
                    desiredDirection = Direction.Up;
                }
            }

            return desiredDirection;
        }

        private void CustomDestroy()
		{
            ShapeManager.Remove(ForwardCollision);
            ShapeManager.Remove(BackwardCollision);

		}


        internal void ReactToReposition()
        {
            this.ForwardCollision.Position = this.Position;
            this.BackwardCollision.Position = this.Position;
        }
        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
