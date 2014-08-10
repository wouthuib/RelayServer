using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.Text.RegularExpressions;
using RelayServer.Database.Monsters;
using RelayServer.WorldObjects.Structures;
using System.Timers;
using RelayServer.ClientObjects;

namespace RelayServer.WorldObjects.Entities
{
    public class MonsterSprite : Entity
    {
        #region properties

        // static and singletons
        //PlayerStore PlayerInfo = PlayerStore.Instance;                                              // get battle information of player
        GameWorld world;

        // Monster Store ID
        public int MonsterID = 0;

        // Drawing properties
        private SpriteEffects spriteEffect = SpriteEffects.None;

        // Respawn properties
        private Vector2 resp_pos = Vector2.Zero,                                                    // Respawn Position
                        resp_bord = Vector2.Zero;                                                   // Walking Border
        private bool spawn = false;                                                                 // Spawn Activator
        private int RESPAWN_TIME = 8;                                                               // 8 seconds respawn

        // Sprite Animation Properties
        private Vector2 Direction = Vector2.Zero;                                                   // Sprite Move direction
        private float Speed;                                                                        // Speed used in functions

        // Movement properties
        const int WALK_SPEED = 100;                                                                 // The actual speed of the entity
        const int ANIMATION_SPEED = 120;                                                            // Animation speed, 120 = default
        const int IDLE_TIME = 10;                                                                   // idle time until next movement
        Border Borders = new Border(0, 0);                                                          // max tiles to walk from center (avoid falling)

        // Clocks and Timers
        float previousWalkTimeSec,                                                                  // WalkTime in Seconds
              previousIdleTimeSec,                                                                  // IdleTime in Seconds
              previousHitTimeSec,                                                                   // IdleTime in Seconds
              previousFrozenTimeSec,                                                                // IdleTime in Seconds
              previousDiedTimeSec,                                                                  // IdleTime in Seconds
              previousSpawnTimeSec,                                                                 // IdleTime in Seconds
              previousAttackTimeSec = 0,                                                            // IdleTime in Seconds
              currentAttackTimeSec = 0;                                                             // IdleTime in Seconds

        #endregion

        public MonsterSprite(int ID, string mapname, Vector2 position, Vector2 borders)
            : base()
        {
            // Derived properties
            Active = true;
            Position = position;
            OldPosition = position;

            int size = 75;

            switch (MonsterStore.Instance.getMonster(ID).Size)
            {
                case "small":
                    size = 50;
                    break;
                case "medium":
                    size = 75;
                    break;
                case "big":
                    size = 100;
                    break;
            }

            SpriteFrame = new Rectangle(0, 0, size, size);

            // Save for respawning
            resp_pos = position;
            resp_bord = borders;

            // get battle information from monster database
            HP = MonsterStore.Instance.getMonster(ID).HP;
            MP = MonsterStore.Instance.getMonster(ID).Magic;
            ATK = MonsterStore.Instance.getMonster(ID).ATK;
            DEF = MonsterStore.Instance.getMonster(ID).DEF;
            LVL = MonsterStore.Instance.getMonster(ID).Level;
            HIT = MonsterStore.Instance.getMonster(ID).Hit;
            FLEE = MonsterStore.Instance.getMonster(ID).Flee;
            EXP = MonsterStore.Instance.getMonster(ID).EXP;
            SIZE = MonsterStore.Instance.getMonster(ID).Size;
            Speed = MonsterStore.Instance.getMonster(ID).Speed;

            // read the items drops (see region functions)
            ReadDrops(ID);

            // Local properties
            instanceID = Guid.NewGuid();
            MonsterID = ID;
            mapName = mapname;
            Direction = new Vector2();                                                              // Move direction
            state = EntityState.Spawn;                                                              // Player state
            Borders = new Border(borders.X, borders.Y);                                             // Max Tiles from center
        }

        #region update
        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                previousState = state;

                update_movement(gameTime);
                update_animation(gameTime);
                // update_collision(gameTime);

                if (previousState != state)
                    sendtoClient();
            }
        }

        private void update_movement(GameTime gameTime)
        {
            switch (state)
            {
                case EntityState.Stand:

                    previousIdleTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (previousIdleTimeSec <= 0)
                    {
                        previousWalkTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + Randomizer.Instance.generateRandom(6, 12);

                        // temporary random generator
                        if (Randomizer.Instance.generateRandom(0, 2) == 1)
                            spriteEffect = SpriteEffects.None;
                        else
                            spriteEffect = SpriteEffects.FlipHorizontally;

                        // reset sprite frame and change state
                        state = EntityState.Walk;
                    }

                    break;

                case EntityState.Walk:

                    previousWalkTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (previousWalkTimeSec <= 0)
                    {
                        previousIdleTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + Randomizer.Instance.generateRandom(6, 12);

                        // reset sprite frame and change state
                        state = EntityState.Stand;
                    }

                    break;
            }
        }

        private void update_animation(GameTime gameTime)
        {
            switch (state)
            {
                #region stand
                case EntityState.Stand:

                    Speed = 0;
                    Direction = Vector2.Zero;

                    // Check if Monster is steady standing
                    if (Position.Y > OldPosition.Y)
                        state = EntityState.Falling;

                    // Move the Monster
                    OldPosition = Position;

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 200 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region walk
                case EntityState.Walk:

                    Speed = 0;
                    Direction = Vector2.Zero;

                    if (spriteEffect == SpriteEffects.FlipHorizontally)
                    {
                        // walk right
                        this.Direction.X = 1;
                        this.Speed = WALK_SPEED;
                    }
                    else if (spriteEffect == SpriteEffects.None)
                    {
                        // walk left
                        this.Direction.X = -1;
                        this.Speed = WALK_SPEED;
                    }

                    // Check if monster is steady standing
                    if (Position.Y > OldPosition.Y && collideSlope == false)
                        state = EntityState.Falling;

                    // Update the Position Monster
                    OldPosition = Position;

                    // Walk speed
                    Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 200 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Walking Border for monster
                    if (Position.X <= Borders.Min)
                    {
                        Position = OldPosition;
                        spriteEffect = SpriteEffects.FlipHorizontally;
                    }
                    else if (Position.X >= Borders.Max)
                    {
                        Position = OldPosition;
                        spriteEffect = SpriteEffects.None;
                    }

                    break;
                #endregion
                #region falling
                case EntityState.Falling:

                    if (OldPosition.Y < position.Y)
                    {
                        // Move the Character
                        OldPosition = Position;

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                        state = EntityState.Stand;

                    break;
                #endregion
                #region hit
                case EntityState.Hit:

                    previousHitTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (previousHitTimeSec <= 0)
                    {
                        // Start Hit timer (Avoid rapid hit)
                        previousHitTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.1f;

                        // Start freeze timer 
                        previousFrozenTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.7f;

                        // change state (freeze or kill)
                        if (this.HP <= 0)
                        {
                            // Monster respawn timer
                            previousDiedTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + RESPAWN_TIME;

                            // Monster Item Drops
                            //foreach (var drop in ItemDrop)
                            //{
                                // drop[0] = item, drop[1] = chance in %
                                //if (Randomizer.Instance.generateRandom(0, 100) <= drop[1])
                                //    GameWorld.Instance.newEffect.Add(new ItemSprite(
                                //        new Vector2(Randomizer.Instance.generateRandom((int)this.position.X + 20, (int)this.position.X + this.spriteFrame.Width - 20),
                                //            (int)(this.position.Y + this.spriteFrame.Height * 0.70f)), drop[0]));
                            //}

                            // Give player EXP
                            // PlayerStore.Instance.activePlayer.Exp += this.EXP;

                            // Change state monster
                            state = EntityState.Died;
                        }
                        else
                        {
                            state = EntityState.Frozen;
                        }
                    }

                    break;
                #endregion
                #region frozen
                case EntityState.Frozen:

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // reduce timer
                    previousFrozenTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    previousHitTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (previousFrozenTimeSec <= 0)
                    {
                        // reset sprite frame
                        state = EntityState.Stand;
                    }
                    break;
                #endregion
                #region died
                case EntityState.Died:

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // reduce timer
                    previousDiedTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // removing counter
                    if (previousDiedTimeSec <= 0)
                    {
                        // link to world
                        if (world == null)
                            world = GameWorld.Instance;

                        // respawn a new monster
                        world.newEntity.Add(new MonsterSprite(
                                    MonsterID,
                                    MapName,
                                    resp_pos,
                                    new Vector2((int)resp_bord.X, (int)resp_bord.Y)
                                    ));

                        // remove monster from map
                        this.keepAliveTime = 0;
                    }
                    break;
                #endregion
                #region spawn
                case EntityState.Spawn:

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // reduce timer
                    previousSpawnTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Monster Spawn Timer
                    if (previousSpawnTimeSec <= 0)
                    {
                        if (spawn)
                        {
                            state = EntityState.Stand;
                        }
                        else
                        {
                            spawn = true;
                            previousSpawnTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + 1.1f;
                        }
                    }

                    break;
                #endregion
            }
        }

        public void sendtoClient(Client user = null)
        {
            MonsterData p = new MonsterData()
            {
                MonsterID = this.MonsterID,
                InstanceID = (string)this.InstanceID.ToString(),
                MapName = (string)this.MapName,
                PositionX = (int)this.Position.X,
                PositionY = (int)this.Position.Y,
                BorderMin = (int)this.Borders.Min,
                BorderMax = (int)this.Borders.Max,
                spritestate = (string)this.State.ToString(),
                spriteEffect = (string)this.spriteEffect.ToString(),
            };

            if(user == null)
                Server.singleton.SendObject(p); // to all
            else
                Server.singleton.SendObject(p, user); // to one user

        }

        private void update_collision(GameTime gameTime)
        {
            // Monster attacks the player method

            // Check of world instance is created
            if (world == null)
                world = GameWorld.Instance;

            previousAttackTimeSec = currentAttackTimeSec;

            //Entity player = world.Player;

            //if (player is PlayerSprite)
            //{
            //    if (player.SpriteFrame.Intersects(SpriteBoundries))
            //    {
            //        // player + monster state not equal to hit or frozen
            //        if (this.State != EntityState.Hit &&
            //            this.State != EntityState.Died &&
            //            this.State != EntityState.Spawn &&
            //            player.State != EntityState.Hit &&
            //            player.State != EntityState.Frozen)
            //        {
            //            // activate timer
            //            currentAttackTimeSec += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //            // we now use 500 msec, but this should get a ASPD timer
            //            if (currentAttackTimeSec >= 0.5f)
            //            {
            //                // reset the attach timer
            //                currentAttackTimeSec = 0;

            //                // Start damage controll
            //                //int damage = (int)Battle.battle_calc_damage_mob(this, PlayerStore.Instance.activePlayer);
            //                //PlayerStore.Instance.activePlayer.HP -= damage;

            //                // Hit the player
            //                //if (damage > 0)
            //                //    player.State = EntityState.Hit;

            //                //world.newEffect.Add(new DamageBaloon(
            //                //    ResourceManager.GetInstance.Content.Load<Texture2D>(@"gfx\effects\damage_counter2"),
            //                //    new Vector2((player.Position.X + player.SpriteFrame.Width * 0.45f) - damage.ToString().Length * 5,
            //                //                 player.Position.Y + player.SpriteFrame.Height * 0.20f),
            //                //        damage));
            //            }
            //        }
            //    }
            //}

            // reset timer when no player collision
            if (currentAttackTimeSec == previousAttackTimeSec)
            {
                // monster gets hit will not reset the timer
                if (this.state != EntityState.Hit)
                    currentAttackTimeSec = 0;
            }
        }
        #endregion

        #region functions
        private struct Border
        {
            // Structure for monster walking bounds
            public float Min, Max;

            public Border(float min, float max)
            {
                Min = min * 32;
                Max = max * 32;
            }
        }

        private void ReadDrops(int ID)
        {
            PropertyInfo propertyMonster;
            int[] itemdrop = new int[] { 0, 1 };
            int index = 0;

            for (int a = 11; a < MonsterStore.Instance.getMonster(ID).GetType().GetProperties().Length; a++)
            {
                propertyMonster = MonsterStore.Instance.getMonster(ID).GetType().GetProperties()[a];

                if (propertyMonster.Name.StartsWith("drop") && propertyMonster.Name.EndsWith("Item"))
                {
                    var value = propertyMonster.GetValue(MonsterStore.Instance.getMonster(ID), null);
                    itemdrop[index] = Convert.ToInt32(value);
                    index++;
                }
                else if (propertyMonster.Name.StartsWith("drop") && propertyMonster.Name.EndsWith("Chance"))
                {
                    itemdrop[index] = Convert.ToInt32(propertyMonster.GetValue(MonsterStore.Instance.getMonster(ID), null));
                    //ItemDrop.Add(new int[] { itemdrop[0], itemdrop[1] });
                    index = 0;
                }
            }
        }

        public Rectangle SpriteBoundries
        {
            get
            {
                return new Rectangle(
                            (int)(Position.X + SpriteFrame.Width * 0.20f + MonsterStore.Instance.monster_list.Find(x => x.monsterID == this.MonsterID).sizeMod.X),
                            (int)(Position.Y + SpriteFrame.Height * 0.40f + MonsterStore.Instance.monster_list.Find(x => x.monsterID == this.MonsterID).sizeMod.Y),
                            (int)Math.Abs(SpriteFrame.Width * 0.60f - MonsterStore.Instance.monster_list.Find(x => x.monsterID == this.MonsterID).sizeMod.X),
                            (int)Math.Abs(SpriteFrame.Height * 0.60f - MonsterStore.Instance.monster_list.Find(x => x.monsterID == this.MonsterID).sizeMod.Y));
            }
        }

        #endregion
    }
}