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
        static float initialCharacterX = 216;
        static float initialCharacterY = -216;

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

            // Temporary until we get TMX support:
            CreateLevelTriggers();

        }

        void LoadLevel(string levelToLoad)
        {
            InitializeLevel(levelToLoad);
            CreateNpcs(levelToLoad);
        }

        private void CreateNpcs(string levelToLoad)
        {
            var levelNpcFile = GetFile(levelToLoad + "Npcs") as Dictionary<string, DataTypes.LevelNpc>;

            foreach(var npcData in levelNpcFile.Values)
            {
                CreateNpc(npcData);
            }
        }

        private void CreateNpc(LevelNpc npcData)
        {
            Character npc = new Character();
            npc.X = npcData.X;
            npc.Y = npcData.Y;
            npc.Z = 1;
            npc.ReactToReposition();

            npc.Dialog = npcData.Dialog;
            npc.Animation = npcData.Animation;
            NpcCharacterList.Add(npc);
        }

        private void InitializeCharacter()
        {
            CharacterInstance.X = initialCharacterX;
            this.CharacterInstance.Y = initialCharacterY;

            this.CharacterInstance.ReactToReposition();

            this.CharacterInstance.MovementInput = InputManager.Keyboard.Get2DInput(
                Keys.A, Keys.D, Keys.W, Keys.S);

            this.CharacterInstance.ActionInput = InputManager.Keyboard.GetKey(Keys.Space);
        }

        private void CreateLevelTriggers()
        {
            var trigger = new MapNavigationTrigger();
            var collision = new AxisAlignedRectangle();
            collision.Width = 100;
            collision.Height = 100;

            collision.X = 50;
            collision.Y = -50;
            collision.Visible = true;

            trigger.Collision.AxisAlignedRectangles.Add(collision);

            trigger.TargetMap = "Level2";
            trigger.TargetX = 160;
            trigger.TargetY = -160;

            this.MapNavigationTriggerList.Add(trigger);
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
                        if (CharacterInstance.ForwardCollision.CollideAgainst(npc.BackwardCollision))
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
                    initialCharacterX = trigger.TargetX;
                    initialCharacterY = trigger.TargetY;

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
