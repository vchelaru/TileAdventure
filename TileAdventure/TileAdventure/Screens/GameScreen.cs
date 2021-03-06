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

using Cursor = FlatRedBall.Gui.Cursor;
using GuiManager = FlatRedBall.Gui.GuiManager;
using FlatRedBall.Localization;
using Microsoft.Xna.Framework;
using TileAdventure.Entities;
using TileAdventure.DataTypes;
using System.Collections.Specialized;
using System.Linq;

#if FRB_XNA || SILVERLIGHT
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;
#endif
#endregion

namespace TileAdventure.Screens
{
	public partial class GameScreen
	{
        static string levelToLoad = "Level1";
        static string startPointName = "BottomOfTown";

        bool CanMoveCharacter
        {
            get
            {
                return DialogDisplayInstance.Visible == false;
            }
        }

		void CustomInitialize()
        {
            LoadLevel(levelToLoad);

            InitializeCharacter();
        }

        private void HandleNewNpc(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach(var item in e.NewItems)
            {
                (item as Character).ReactToReposition();
            }
        }

        void LoadLevel(string levelToLoad)
        {
            InitializeLevel(levelToLoad);
            AdjustCamera();
            AdjustNpcs();

#if DEBUG
            this.SolidCollisions.Visible =
                DebuggingVariables.ShowShapes;
#endif
        }

        private void AdjustNpcs()
        {
            foreach(var character in NpcCharacterList)
            {
                character.ReactToReposition();
            }
        }

        private void AdjustCamera()
        {
            Camera.Main.MinimumX -= .5f;
            Camera.Main.MaximumX += .5f;
            Camera.Main.MinimumY -= .5f;
            Camera.Main.MaximumY += .5f;
            


            Camera.Main.X += .25f;
            Camera.Main.Y += .25f;
        }

        private void InitializeCharacter()
        {
            var foundStartPoint = this.StartPointList.FirstOrDefault(item => item.Name == startPointName);
            if(foundStartPoint == null)
            {
                throw new Exception($"Could not find start point with a name of {startPointName}");
            }
            this.CharacterInstance.X = foundStartPoint.X;
            this.CharacterInstance.Y = foundStartPoint.Y;

            this.CharacterInstance.ReactToReposition();

            this.CharacterInstance.MovementInput = InputManager.Keyboard.Get2DInput(
                Keys.A, Keys.D, Keys.W, Keys.S);

            this.CharacterInstance.ActionInput = InputManager.Keyboard.GetKey(Keys.Space);
        }

        void CustomActivity(bool firstTimeCalled)
		{
            DialogActivity();

            if (CanMoveCharacter)
            {
                this.CharacterInstance.PerformMovementActivity(this.SolidCollisions, NpcCharacterList);
            }

            CollisionActivity();
		}

        private void DialogActivity()
        {
            if (CharacterInstance.IsAttemptingAction)
            {
                if (this.DialogDisplayInstance.Visible)
                {
                    this.DialogDisplayInstance.Visible = false;
                }
                else
                {
                    Character npcTalkingTo = null;
                    foreach (var npc in this.NpcCharacterList)
                    {
                        if (CharacterInstance.ActionCollision.CollideAgainst(npc.BackwardCollision))
                        {
                            npcTalkingTo = npc;
                            break;
                        }
                    }

                    if(npcTalkingTo != null)
                    {
                        ShowDialog(npcTalkingTo.Dialog);
                    }
                }
            }
        }

        private void ShowDialog(string stringId)
        {
            this.DialogDisplayInstance.Visible = true;
            this.DialogDisplayInstance.Text = LocalizationManager.Translate(stringId);
        }

        private void CollisionActivity()
        {
            foreach(var trigger in MapNavigationTriggerList)
            {
                if(CharacterInstance.BackwardCollision.CollideAgainst(trigger.Collision))
                {
                    levelToLoad = trigger.TargetMap;
                    startPointName = trigger.StartPointName;

                    if(string.IsNullOrEmpty(startPointName))
                    {
                        throw new Exception("Trigger has an empty StartPointName");
                    }

                    RestartScreen(reloadContent: false);
                }
            }

        }

        void CustomDestroy()
		{


		}

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

	}
}
