using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using RelayServer.WorldObjects.Entities;
using RelayServer.Database.Monsters;
using RelayServer.WorldObjects.Structures;
using System.Windows.Forms;

namespace RelayServer.WorldObjects
{
    public class GameWorld : Microsoft.Xna.Framework.Game
    {
        public System.Timers.Timer gameTime;
        public static GameWorld Instance;

        // Map table
        private List<Squared.Tiled.Map> maps = new List<Squared.Tiled.Map>();

        // Map entities
        public List<Entity> listEntity = new List<Entity>();
        public List<Entity> newEntity = new List<Entity>();
        //public List<GameEffect> listEffect = new List<GameEffect>();
        //public List<GameEffect> newEffect = new List<GameEffect>();

        public GameWorld()
        {
            GameWorld.Instance = this; // singleton

            Form MyGameForm = (Form)Form.FromHandle(Window.Handle);
            MyGameForm.FormBorderStyle = FormBorderStyle.None;
            MyGameForm.Opacity = 0;
            MyGameForm.ShowInTaskbar = false;
            MyGameForm.Visible = false;
            MyGameForm.Hide();
        }

        protected override void LoadContent()
        {            
        }

        protected override void Initialize()
        {
            OutputManager.WriteLine("- Loading maps:");

            string[] fileEntries = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Maps\");

            foreach (var file in fileEntries)
            {
                Squared.Tiled.Map map = Squared.Tiled.Map.Load(file);
                maps.Add(map);
                OutputManager.WriteLine("\t Loaded map {0}", new string[] { map.Name });
            }

            OutputManager.WriteLine("- Loading Import tables:");
            MonsterStore.Instance.loadMonster(Directory.GetCurrentDirectory() + @"\Import\", "monstertable.bin");
            OutputManager.WriteLine("\t Monster table '\\Import\\monstertable.bin' loaded!");

            OutputManager.WriteLine("- Loading Map Entities:");
            LoadEntities();
            OutputManager.WriteLine("- Map Entities Loaded!");

            OutputManager.WriteLine("- Starting Game World");
            OutputManager.WriteLine("- Game World Started!");

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Server.singleton.Listener.Active)
            {
                //if (GameWorld.Instance.previousTime <= (float)e.SignalTime.Second)
                //{
                //    GameWorld.Instance.previousTime = (float)e.SignalTime.Second + 10.0f;
                //    Console.WriteLine(GameWorld.Instance.previousTime);
                //}

                foreach(var entity in GameWorld.Instance.listEntity)
                {
                    if(entity is MonsterSprite)
                    {
                        MonsterSprite monster = (MonsterSprite)entity;
                        monster.Update(gameTime);
                    }
                }

                // Update all enitities map collisions
                GameWorld.Instance.UpdateMapEntities();
            }

            base.Update(gameTime);
        }

        public void UpdateMapEntities()
        {
            #region map collisions

            if (listEntity.Count > 0)
            {
                foreach (var map in maps)
                {
                    foreach (Entity entity in listEntity)
                    {
                        // start with the collision check
                        if (entity.Position != entity.OldPosition)
                        {
                            entity.CollideLadder = false;
                            entity.CollideRope = false;
                            entity.CollideSlope = false;

                            Rectangle EntityRec = new Rectangle(
                                (int)entity.Position.X + (int)(entity.SpriteSize.Width * 0.25f),
                                (int)entity.Position.Y,
                                (int)entity.SpriteFrame.Width - (int)(entity.SpriteSize.Width * 0.25f),
                                (int)entity.SpriteFrame.Height);

                            #region slope collision
                            // Check Slope collision (player and NPC!)
                            foreach (var obj in map.ObjectGroups["Slopes"].Objects)
                            {
                                char[] chars = obj.Value.Name.ToCharArray();
                                string objname = new string(chars).Substring(0, 5); // max 4 chars to skip numbers

                                if (objname == "slope")
                                {
                                    Rectangle Slope = new Rectangle((int)obj.Value.X, (int)obj.Value.Y, (int)obj.Value.Width, (int)obj.Value.Height);

                                    if (EntityRec.Intersects(Slope) &&
                                        entity.Position.X + entity.SpriteFrame.Width * 0.70f > Slope.Left &&
                                        entity.Position.X + entity.SpriteFrame.Width * 0.30f < Slope.Right &&
                                        entity.Position.Y + entity.SpriteFrame.Height < Slope.Bottom + 2)
                                    {
                                        // set collision slope
                                        entity.CollideSlope = true;

                                        // collision detection
                                        Vector2 tileSlope = new Vector2(0, 0);

                                        EntityRec = new Rectangle((int)entity.Position.X, (int)entity.Position.Y,
                                            (int)entity.SpriteFrame.Width, (int)entity.SpriteFrame.Height);

                                        // reading XML - wall properties
                                        foreach (var property in obj.Value.Properties)
                                        {
                                            if (property.Key == "SlopeX")
                                                tileSlope.X = Convert.ToInt32(property.Value);
                                            if (property.Key == "SlopeY")
                                                tileSlope.Y = Convert.ToInt32(property.Value);
                                        }

                                        float SlopeRad = (float)Slope.Height / (float)Slope.Width;

                                        if (tileSlope.X < tileSlope.Y) // move down
                                        {
                                            //float inSlopePosition = (EntityRec.Center.X * SlopeRad) - Slope.Left;
                                            float inSlopePosition = (EntityRec.Center.X - Slope.Left) * SlopeRad;
                                            if (inSlopePosition > 0)
                                                if (EntityRec.Bottom > Slope.Top + inSlopePosition)
                                                    entity.PositionY = (Slope.Top + inSlopePosition) - entity.SpriteFrame.Height;
                                        }

                                        if (tileSlope.X > tileSlope.Y) // move up
                                        {
                                            float inSlopePosition = (EntityRec.Center.X - Slope.Left) * SlopeRad;

                                            if (inSlopePosition < Slope.Height)
                                                if (EntityRec.Bottom > Slope.Bottom - inSlopePosition)
                                                    entity.PositionY = (Slope.Bottom - inSlopePosition) - entity.SpriteFrame.Height;
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region walls collision
                            // Check wall collision (player and NPC!)
                            foreach (var obj in map.ObjectGroups["Walls"].Objects)
                            {
                                char[] chars = obj.Value.Name.ToCharArray();
                                string objname = new string(chars).Substring(0, 4); // max 4 chars to skip numbers

                                if (objname == "wall")
                                {
                                    Rectangle Wall = new Rectangle((int)obj.Value.X, (int)obj.Value.Y, (int)obj.Value.Width, (int)obj.Value.Height);

                                    if (EntityRec.Intersects(Wall) &&
                                        entity.Position.X + entity.SpriteFrame.Width * 0.70f > Wall.Left &&
                                        entity.Position.X + entity.SpriteFrame.Width * 0.30f < Wall.Right)
                                    {

                                        // reading XML - wall properties
                                        int Block = 0;
                                        foreach (var property in obj.Value.Properties)
                                            if (property.Key == "Block")
                                                Block = Convert.ToInt32(property.Value);

                                        if (Block == 1)
                                        {
                                            entity.PositionX = entity.OldPositionX;
                                        }
                                        else
                                        {
                                            switch (entity.State)
                                            {
                                                case EntityState.Rope:
                                                case EntityState.Ladder:
                                                    if (entity.Position.Y + EntityRec.Height * 0.75f < Wall.Top)
                                                    {
                                                        entity.PositionY -= 10;
                                                        entity.State = EntityState.Stand;
                                                    }
                                                    break;
                                                case EntityState.Jump:
                                                    break;
                                                default:
                                                    //if (entity is NPCharacter)
                                                    //{
                                                    //    if (entity.PositionY > entity.OldPositionY &&
                                                    //        entity.PositionY + (entity.SpriteFrame.Height * 0.90f) < Wall.Top)
                                                    //        entity.PositionY = Wall.Top - entity.SpriteFrame.Height;
                                                    //}
                                                    //else
                                                    //{
                                                    //    if (entity.PositionY > entity.OldPositionY &&
                                                    //        entity.PositionY + (entity.SpriteFrame.Height * 0.50f) < Wall.Top)
                                                    //        entity.PositionY = Wall.Top - entity.SpriteFrame.Height;
                                                    //}
                                                        if (entity.PositionY > entity.OldPositionY &&
                                                            entity.PositionY + (entity.SpriteFrame.Height * 0.50f) < Wall.Top)
                                                            entity.PositionY = Wall.Top - entity.SpriteFrame.Height;
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region ladder and rope collision
                            // Check ladder + Robe collision (player and NPC!)
                            foreach (var obj in map.ObjectGroups["Climbing"].Objects)
                            {
                                char[] chars = obj.Value.Name.ToCharArray();
                                string objname = new string(chars).Substring(0, 4); // max 4 chars to skip numbers

                                if (objname == "rope")
                                {
                                    Rectangle Rope = new Rectangle((int)obj.Value.X, (int)obj.Value.Y, (int)obj.Value.Width, (int)obj.Value.Height);

                                    if (EntityRec.Intersects(Rope) &&
                                        entity.Position.X + entity.SpriteFrame.Width * 0.65f > Rope.Left &&
                                        entity.Position.X + entity.SpriteFrame.Width * 0.45f < Rope.Right &&
                                        entity.Position.Y + entity.SpriteFrame.Height * 0.50f < Rope.Bottom)
                                    {
                                        entity.CollideRope = true;
                                    }
                                }
                                else if (objname == "ladd")
                                {
                                    Rectangle Ladder = new Rectangle((int)obj.Value.X, (int)obj.Value.Y, (int)obj.Value.Width, (int)obj.Value.Height);

                                    if (EntityRec.Intersects(Ladder) &&
                                        entity.Position.X + entity.SpriteFrame.Width * 0.65f > Ladder.Left &&
                                        entity.Position.X + entity.SpriteFrame.Width * 0.45f < Ladder.Right &&
                                        entity.Position.Y + entity.SpriteFrame.Height * 0.50f < Ladder.Bottom)
                                    {
                                        entity.CollideLadder = true;
                                    }
                                }
                            }
                            #endregion                            
                            #region mapbound collision
                            // check maps bounds (player and NPC!)
                            if (EntityRec.Right > map.Width * map.TileWidth || EntityRec.Bottom > map.Height * map.TileHeight ||
                                EntityRec.Left < 0 || EntityRec.Top < 0)
                                if (entity is MonsterSprite)
                                    entity.Position = entity.OldPosition;
                            #endregion

                        }
                    }
                }
            }
            #endregion
        }

        #region functions load/save

        public void LoadEntities()
        {
            foreach (var map in maps)
            {
                foreach (var group in map.ObjectGroups)
                {
                    if (group.Key == "NPCS")
                    {
                        foreach (var obj in group.Value.Objects)
                        {
                            char[] chars = obj.Value.Name.ToCharArray();
                            string objname = new string(chars).Substring(0, 3); // max 3 chars to skip numbers

                            int offsetX = 0, offsetY = 0, spritesizeX = 0, spritesizeY = 0, frames = 1;
                            string texture = null, face = null, name = null, script = null;

                            if (objname == "npc")
                            {
                                foreach (var objprop in obj.Value.Properties)
                                {
                                    string objkey = objprop.Key.ToString();
                                    string objvalue = objprop.Value.ToString();

                                    switch (objkey)
                                    {
                                        case "OffsetX":
                                            offsetX = Convert.ToInt32(objvalue);
                                            break;
                                        case "OffsetY":
                                            offsetY = Convert.ToInt32(objvalue);
                                            break;
                                        case "Texture":
                                            texture = objvalue;
                                            break;
                                        case "spriteSizeX":
                                            spritesizeX = Convert.ToInt32(objvalue);
                                            break;
                                        case "spriteSizeY":
                                            spritesizeY = Convert.ToInt32(objvalue);
                                            break;
                                        case "Frames":
                                            frames = Convert.ToInt32(objvalue);
                                            break;
                                        case "Face":
                                            face = objvalue;
                                            break;
                                        case "Name":
                                            name = objvalue;
                                            break;
                                        case "Script":
                                            script = objvalue;
                                            break;
                                    }
                                }
                                try
                                {
                                    //string facetext = null;
                                    // properties are filled now check the state
                                    //listEntity.Add(new NPCharacter(
                                    //            Content.Load<Texture2D>(@"gfx\NPCs\" + texture),
                                    //            new Vector2(offsetX, offsetY),
                                    //            new Vector2(spritesizeX, spritesizeY),
                                    //            new Vector2(obj.Value.X, obj.Value.Y),
                                    //            frames,
                                    //            facetext,
                                    //            name,
                                    //            script));
                                }
                                catch
                                {
                                    // bug handler for NPC import properties
                                    OutputManager.WriteLine("\t Error! loading NPC {0}, in Map {1}", new string[]{ name, map.Name});
                                }
                                finally
                                {
                                    OutputManager.WriteLine("\t Loaded NPC {0}, {1}", new string[]{ map.Name, name});
                                }
                            }
                        }
                    }
                    else if (group.Key == "Monsters")
                    {
                        foreach (var obj in group.Value.Objects)
                        {
                            char[] chars = obj.Value.Name.ToCharArray();
                            string objname = new string(chars).Substring(0, 3); // max 3 chars to skip numbers

                            int borderX = 0, borderY = 0, monsterID = 0;

                            if (objname == "mob")
                            {
                                foreach (var objprop in obj.Value.Properties)
                                {
                                    string objkey = objprop.Key.ToString();
                                    string objvalue = objprop.Value.ToString();

                                    switch (objkey)
                                    {
                                        case "monsterID":
                                            monsterID = Convert.ToInt32(objvalue);
                                            break;
                                        case "BorderL":
                                            borderX = Convert.ToInt32(objvalue);
                                            break;
                                        case "BorderR":
                                            borderY = Convert.ToInt32(objvalue);
                                            break;
                                    }
                                }
                                try
                                {
                                    string monstername = MonsterStore.Instance.monster_list.Find(m => m.monsterID == monsterID).monsterName;

                                    // properties are filled now check the state
                                    listEntity.Add(new MonsterSprite(
                                                monsterID,
                                                map.Name,
                                                new Vector2(obj.Value.X, obj.Value.Y),
                                                new Vector2(borderX, borderY)));
                                }
                                catch
                                {
                                    // bug handler for Mob import properties
                                    OutputManager.WriteLine("\t Error! loading Mob {0}, in Map {1}", new string[] { monsterID.ToString(), map.Name });
                                }
                                finally
                                {
                                    if (MonsterStore.Instance.monster_list.FindAll(m => m.monsterID == monsterID).Count > 0)
                                        OutputManager.WriteLine("\t Loaded Mob {0}, {1}", new string[] { map.Name, MonsterStore.Instance.monster_list.Find(m => m.monsterID == monsterID).monsterName });
                                }
                            }
                        }
                    }
                    else if (group.Key == "Warps")
                    {
                        foreach (var obj in group.Value.Objects)
                        {
                            char[] chars = obj.Value.Name.ToCharArray();
                            string objname = new string(chars).Substring(0, 4); // max 4 chars to skip numbers

                            if (objname == "warp")
                            {
                                string newmap = null;
                                int newposx = 0, newposy = 0, camposx = 0, camposy = 0;

                                foreach (var objprop in obj.Value.Properties)
                                {
                                    string objkey = objprop.Key.ToString();
                                    string objvalue = objprop.Value.ToString();

                                    if (objkey == "newmap")
                                        newmap = objvalue;
                                    else if (objkey == "newposx")
                                        newposx = Convert.ToInt32(objvalue);
                                    else if (objkey == "newposy")
                                        newposy = Convert.ToInt32(objvalue);
                                    else if (objkey == "camposx")
                                        camposx = Convert.ToInt32(objvalue);
                                    else if (objkey == "camposy")
                                        camposy = Convert.ToInt32(objvalue);

                                }
                                try
                                {
                                    //listEffect.Add(new Warp(Content.Load<Texture2D>(@"gfx\gameobjects\warp"),
                                    //                        new Vector2(obj.Value.X - 20, obj.Value.Y - 225),
                                    //                        newmap, new Vector2(newposx, newposy),
                                    //                        new Vector2(camposx, camposy)));
                                }
                                catch (Exception ee)
                                {
                                    // bug handler for Warp import properties
                                    string aa = ee.ToString();
                                }
                                finally
                                {
                                    string[] newmaps = newmap.Split('\\');
                                    OutputManager.WriteLine("\t Loaded Warp from {0} {1} {2}, to {3} {4} {5}", new string[] { map.Name, obj.Value.X.ToString(), obj.Value.Y.ToString(), newmaps[newmaps.Length -1], newposx.ToString(), newposy.ToString() });
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
