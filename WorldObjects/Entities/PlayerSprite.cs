using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RelayServer.Database.Players;
using System.Collections.Generic;
using RelayServer.Database.Items;
using RelayServer.WorldObjects.Structures;
using MapleLibrary;
using RelayServer.WorldObjects.Effects;

namespace RelayServer.WorldObjects.Entities
{
    public class PlayerSprite : Entity
    {
        #region properties and constructor

        public string Name;
        private Vector2 previousPosition;
        private float previousGameTimeMsec;
        private float displayTimer = 0;

        private const int PLAYER_SPEED = 215;                                                       // The network player is a bit slower due to latency
        private const int ANIMATION_SPEED = 120;                                                    // Animation speed, 120 = default 
        private const int MOVE_UP = -1;                                                             // player moving directions
        private const int MOVE_DOWN = 1;                                                            // player moving directions
        private const int MOVE_LEFT = -1;                                                           // player moving directions
        private const int MOVE_RIGHT = 1;                                                           // player moving directions

        private string
            armor_name,                 // Armor and Costume Sprite (4)
            //accessorry_top_name,        // Accessory top Sprite (Sunglasses, Ear rings) (5)
            //accessorry_bottom_name,     // Accessory bottom Sprite (mouth items, capes) (6)
            headgear_name,              // Headgear Sprite (Hats, Helmets) (7)
            weapon_name;                // Weapon Sprite (8)
            //hands_name;                 // Hands Sprite (9)        

        public Vector2 Direction, previousDirection;                                                // Sprite Move direction
        protected float Speed;                                                                      // Speed used in functions
        protected Vector2 Velocity = new Vector2(0, 1);                                             // speed used in jump
        protected bool landed = false;                                                              // landed sprite
        protected Vector2 Climbing;

        // Player properties
        public SpriteEffects spriteEffect = SpriteEffects.None,
                             previousSpriteEffect;
        protected float transperancy = 1;
        protected bool debug = false;

        // new Texture properties
        public int spriteframe = 0, prevspriteframe = 0, maxspriteframe = 0;
        public string spritename = "stand1_0", attackSprite;
        public string[] spritepath = new string[] 
        { 
            @"gfx\player\body\head\",                                                               // Head Sprite  (0)
            @"gfx\player\body\torso\",                                                              // Body Sprite (1)
            @"gfx\player\faceset\face1\",                                                           // Faceset Sprite (2)
            @"gfx\player\hairset\hair1\",                                                           // Hairset Sprite (3)
            "",                                                                                     // Armor and Costume Sprite (4)
            "",                                                                                     // Accessory top Sprite (Sunglasses, Ear rings) (5)
            "",                                                                                     // Accessory bottom Sprite (mouth items, capes) (6)
            "",                                                                                     // Headgear Sprite (Hats, Helmets) (7)
            "",                                                                                     // Weapon Sprite (8)
            @"gfx\player\body\hands\",                                                              // Hands Sprite (9)
        };
        
        public PlayerInfo Player 
        {
            get
            {
                if (this.Name != null)
                    return PlayerStore.Instance.playerStore.Find(x => x.Name == this.Name);
                else
                    return null;
            }
        }


        string Client_action;

        public PlayerSprite(
            string name,
            string ip,
            float positionX,
            float positionY,
            string _spritename,
            string _spritestate,
            int _prevspriteframe,
            int _maxspriteframe,
            string _attackSprite,
            string _spriteEffect,
            string mapName,
            string skincolor,
            string facesprite,
            string hairsprite,
            string haircolor,
            string armor,
            string headgear,
            string weapon
            )
            : base()
        {
            Name = name;
            entityName = name; // used by removing instance in worldmap!!
            Position = new Vector2(positionX, positionY);
            if (_spritename != null)
                spritename = _spritename;
            if (_spritestate != null)
                state = (EntityState)Enum.Parse(typeof(EntityState), _spritestate);
            prevspriteframe = _prevspriteframe;
            maxspriteframe = _maxspriteframe;
            attackSprite = _attackSprite;
            if (_spriteEffect != null)
                spriteEffect = (SpriteEffects)Enum.Parse(typeof(SpriteEffects), _spriteEffect);
            else
                spriteEffect = SpriteEffects.FlipHorizontally;
            MapName = mapName;

            this.spriteFrame = new Rectangle(0, 0, 50, 70); // default for collision detection

            this.Player.skin_color = getColor(skincolor);
            this.Player.faceset_sprite = facesprite;
            this.Player.hair_sprite = hairsprite;
            this.Player.hair_color = getColor(haircolor);
            this.armor_name = armor;
            this.headgear_name = headgear;
            this.weapon_name = weapon;
        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            previousPosition = this.position;   // save previous postion
            previousState = this.state;         // save previous state before
            previousDirection = this.Direction; // save previous direction

            //this.PLAYER_SPEED = Randomizer.Instance.generateRandom(105, 315); // for debugging only!!!!

            switch (state)
            {
                #region state skillactive
                case EntityState.Skill:

                    // Move the Character
                    OldPosition = Position;

                    // lock player at position
                    this.Direction.X = 0;

                    // Walk speed
                    Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;

                #endregion
                #region state cooldown
                case EntityState.Cooldown:
                    break;

                #endregion
                #region state swinging
                case EntityState.Swing:

                    Speed = 0;
                    Direction = Vector2.Zero;
                    Velocity = Vector2.Zero;

                    // Move the Character
                    OldPosition = Position;

                    // reduce timer
                    previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Player animation
                    if (prevspriteframe != spriteframe)
                    {
                        prevspriteframe = spriteframe;
                        for (int i = 0; i < spritepath.Length; i++)
                        {
                            spritename = attackSprite + spriteframe.ToString();
                        }
                    }

                    if (previousGameTimeMsec < 0)
                    {
                        previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;

                        // set sprite frames
                        spriteframe++;

                        if (spriteframe > maxspriteframe)
                        {
                            spriteframe = maxspriteframe;
                            previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;

                            // create swing effect
                            if (spriteEffect == SpriteEffects.FlipHorizontally)
                            {
                                Vector2 pos = new Vector2(this.Position.X + this.SpriteFrame.Width * 1.6f, this.Position.Y + this.SpriteFrame.Height * 0.7f);

                                //world.newEffect.Add(new WeaponSwing(pos, WeaponSwingType.Swing01, spriteEffect));
                            }
                            else
                            {
                                Vector2 pos = new Vector2(this.Position.X - this.SpriteFrame.Width * 0.6f, this.Position.Y + this.SpriteFrame.Height * 0.7f);

                                //world.newEffect.Add(new WeaponSwing(pos, WeaponSwingType.Swing01, spriteEffect));
                            }

                            //state = EntityState.Cooldown;
                        }
                    }

                    break;
                #endregion
                #region state stabbing
                case EntityState.Stab:

                    Speed = 0;
                    Direction = Vector2.Zero;
                    Velocity = Vector2.Zero;

                    // Move the Character
                    OldPosition = Position;

                    // reduce timer
                    previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Player animation
                    if (prevspriteframe != spriteframe)
                    {
                        prevspriteframe = spriteframe;
                        for (int i = 0; i < spritepath.Length; i++)
                        {
                            spritename = attackSprite + spriteframe.ToString();
                        }
                    }

                    if (previousGameTimeMsec < 0)
                    {
                        previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;

                        // set sprite frames
                        spriteframe++;

                        if (spriteframe > maxspriteframe)
                        {
                            spriteframe = maxspriteframe;
                            previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;

                            // create stab effect
                            if (spriteEffect == SpriteEffects.FlipHorizontally)
                            {
                                Vector2 pos = new Vector2(this.Position.X + this.SpriteFrame.Width * 0.3f, this.Position.Y + this.SpriteFrame.Height * 0.7f);
                                //world.newEffect.Add(new WeaponSwing(pos, WeaponSwingType.Stab01, spriteEffect));
                            }
                            else
                            {
                                Vector2 pos = new Vector2(this.Position.X - this.SpriteFrame.Width * 0.7f, this.Position.Y + this.SpriteFrame.Height * 0.7f);
                                //world.newEffect.Add(new WeaponSwing(pos, WeaponSwingType.Stab01, spriteEffect));
                            }

                            //state = EntityState.Cooldown;
                        }
                    }

                    // Apply Gravity 
                    // Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region state shooting
                case EntityState.Shoot:

                    Speed = 0;
                    Direction = Vector2.Zero;
                    Velocity = Vector2.Zero;

                    // Move the Character
                    OldPosition = Position;

                    if (Client_action == "Release")
                    {
                        // create and release an arrow
                        if (spriteEffect == SpriteEffects.FlipHorizontally)
                            GameWorld.Instance.newEffect.Add(
                                new Arrow(Player.Name,
                                    new Vector2(this.Position.X, this.Position.Y + this.SpriteFrame.Height * 0.6f),
                                    800, new Vector2(1, 0), Vector2.Zero));
                        else
                            GameWorld.Instance.newEffect.Add(
                                new Arrow(Player.Name,
                                    new Vector2(this.Position.X, this.Position.Y + this.SpriteFrame.Height * 0.6f),
                                    800, new Vector2(-1, 0), Vector2.Zero));

                        spriteFrame.X = 0;
                        state = EntityState.Stand; // << was changed from cooldown
                    }

                    break;
                #endregion
                #region state sit
                case EntityState.Sit:

                    Speed = 0;
                    Direction = Vector2.Zero;
                    Velocity = Vector2.Zero;
                        
                    if (Client_action == "Stand")
                        state = EntityState.Stand;

                    // Move the Character
                    OldPosition = Position;
                    
                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region state Rope
                case EntityState.Rope:

                    Velocity = Vector2.Zero;
                    spriteEffect = SpriteEffects.None;

                    // double check collision
                    if (this.collideLadder == false)
                        this.state = EntityState.Falling;

                    if (Client_action == "Down")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.Y = MOVE_DOWN;
                        this.Speed = PLAYER_SPEED * 0.80f;
                    }
                    else if (Client_action == "Up")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.Y = MOVE_UP;
                        this.Speed = PLAYER_SPEED * 0.80f;
                    }
                    else if (Client_action == "Stop")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.Y = 0;
                        this.Speed = 0;
                    }

                    // Move the Character
                    OldPosition = Position;

                    // Climb speed
                    Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region state Ladder
                case EntityState.Ladder:

                    Velocity = Vector2.Zero;
                    spriteEffect = SpriteEffects.None;

                    // double check collision
                    if (this.collideLadder == false)
                        this.state = EntityState.Falling;

                    if (Client_action == "Down")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.Y = MOVE_DOWN;
                        this.Speed = PLAYER_SPEED * 0.80f;
                    }
                    else if (Client_action == "Up")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.Y = MOVE_UP;
                        this.Speed = PLAYER_SPEED * 0.80f;
                    }
                    else if (Client_action == "Stop")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.Y = 0;
                        this.Speed = 0;
                    }

                    // Move the Character
                    OldPosition = Position;

                    // Climb speed
                    Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region state stand
                case EntityState.Stand:

                    Speed = 0;
                    Direction = Vector2.Zero;
                    Velocity = Vector2.Zero;

                    // Move the Character
                    OldPosition = Position;

                    if (Client_action == "Right")
                    {
                        state = EntityState.Walk;
                        spriteEffect = SpriteEffects.FlipHorizontally;
                    }
                    else if (Client_action == "Left")
                    {
                        state = EntityState.Walk;
                        spriteEffect = SpriteEffects.None;
                    }
                    else if (Client_action == "Jump")
                    {
                        if (!collideNPC)
                        {
                            Velocity += new Vector2(0, -1.6f); // Add an upward impulse
                            state = EntityState.Jump;
                        }
                    }
                    else if (Client_action == "Sit")
                    {
                        state = EntityState.Sit;
                    }
                    else if (Client_action == "Up")
                    {
                        if (this.collideLadder)
                        {
                            state = EntityState.Ladder;
                            this.Direction.Y = MOVE_UP;
                            this.Speed = PLAYER_SPEED * 0.80f;
                        }
                        else if (this.collideRope)
                        {
                            state = EntityState.Rope;
                            this.Direction.Y = MOVE_UP;
                            this.Speed = PLAYER_SPEED * 0.80f;
                        }
                    }
                    else if (Client_action == "Action")
                    {
                        // check if weapon is equiped
                        if (Player.equipment.item_list.FindAll(delegate(Item item) { return item.Type == ItemType.Weapon; }).Count > 0)
                        {
                            WeaponType weapontype = Player.equipment.item_list.Find(delegate(Item item) { return item.Type == ItemType.Weapon; }).WeaponType;

                            // check the weapon type
                            if (weapontype == WeaponType.Bow)
                            {
                                previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + (float)((350 - Player.ASPD * 12) * 0.0006f) + 0.05f;
                                state = EntityState.Shoot;
                            }
                            //else
                            //{
                            //    previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + (float)((350 - Player.ASPD * 12) * 0.0006f) + 0.05f;

                            //    spriteframe = 0;
                            //    GetattackSprite(weapontype);
                            //}
                        }
                    }
                    
                    if (state != EntityState.Ladder && state != EntityState.Rope)
                    {
                        // Check if player is steady standing
                        if (Position.Y > OldPosition.Y && collideSlope == false)
                            state = EntityState.Falling;

                        // Apply Gravity
                        Position += new Vector2(0, 1) * PLAYER_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    
                    break;
                #endregion
                #region state walk
                case EntityState.Walk:

                    Speed = 0;
                    Direction = Vector2.Zero;
                    Velocity = Vector2.Zero;

                    if (spriteEffect == SpriteEffects.FlipHorizontally)
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.X = MOVE_RIGHT;
                        this.Speed = PLAYER_SPEED;
                    }
                    else if (spriteEffect == SpriteEffects.None)
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.X = MOVE_LEFT;
                        this.Speed = PLAYER_SPEED;
                    }

                    if (Client_action == "Stop")
                    {
                        state = EntityState.Stand;
                    }
                    else if (Client_action == "Jump")
                    {
                        if (!collideNPC)
                        {
                            Velocity += new Vector2(0, -1.6f); // Add an upward impulse
                            state = EntityState.Jump;
                        }
                    }

                    // Check if monster is steady standing
                    if (Position.Y > OldPosition.Y && collideSlope == false)
                        state = EntityState.Falling;

                    // Move the Character
                    OldPosition = Position;

                    // Walk speed
                    Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    
                    // Apply Gravity 
                    Position += new Vector2(0, 1) * PLAYER_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region state jump
                case EntityState.Jump:

                    if (previousState != this.state)
                        Velocity = new Vector2(0, -1.5f);
                    else
                        Velocity.Y += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (Client_action == "Left")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.X += MOVE_LEFT * 0.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
                        this.Speed = PLAYER_SPEED;

                        if (this.Direction.X < -1)
                            this.Direction.X = -1;
                        else if (this.Direction.X < 0)
                            this.Direction.X = 0;
                    }
                    else if (Client_action == "Right")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.X += MOVE_RIGHT * 0.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
                        this.Speed = PLAYER_SPEED;

                        if (this.Direction.X > 1)
                            this.Direction.X = 1;
                        else if (this.Direction.X > 0)
                            this.Direction.X = 0;
                    }
                    else if (Client_action == "Down")
                    {
                        if (this.collideLadder)
                            state = EntityState.Ladder;
                        else if (this.collideRope)
                            state = EntityState.Rope;
                        else
                            state = EntityState.Sit;
                    }
                    else if (Client_action == "Up")
                    {
                        if (this.collideLadder)
                            state = EntityState.Ladder;
                        else if (this.collideRope)
                            state = EntityState.Rope;
                    }

                    // Move the Character
                    OldPosition = Position;

                    // Apply Gravity + jumping
                    if (Velocity.Y < -1.2f)
                    {
                        // Apply jumping
                        Position += Velocity * 350 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Walk / Jump speed
                        Position += Direction * (Speed / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        landed = false;
                        state = EntityState.Falling;
                    }

                    break;
                #endregion
                #region state falling
                case EntityState.Falling:

                    if (Client_action == "Left")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.X += MOVE_LEFT * 0.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
                        this.Speed = PLAYER_SPEED;

                        if (this.Direction.X < -1)
                            this.Direction.X = -1;
                        else if (this.Direction.X < 0)
                            this.Direction.X = 0;
                    }
                    else if (Client_action == "Right")
                    {
                        // move player location (make ActiveMap tile check here in the future)
                        this.Direction.X += MOVE_RIGHT * 0.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
                        this.Speed = PLAYER_SPEED;

                        if (this.Direction.X > 1)
                            this.Direction.X = 1;
                        else if (this.Direction.X > 0)
                            this.Direction.X = 0;
                    }

                    if (OldPosition.Y < position.Y)
                    {
                        // Move the Character
                        OldPosition = Position;

                        Velocity.Y += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Walk / Jump speed
                        Position += Direction * (Speed / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        // reduce timer
                        previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (previousGameTimeMsec < 0)
                        {
                            previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.02f;

                            if (landed == true)
                                state = EntityState.Stand;
                            else
                                landed = true;
                        }

                        // Move the Character
                        OldPosition = Position;

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Walk / Jump speed
                        Position += Direction * (Speed / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }

                    break;
                #endregion
                #region state hit
                case EntityState.Hit:

                    // Add an upward impulse
                    Velocity = new Vector2(0, -1.5f);

                    // Add an sideward pulse
                    if (spriteEffect == SpriteEffects.None)
                        Direction = new Vector2(1.6f, 0);
                    else
                        Direction = new Vector2(-1.6f, 0);

                    // Damage controll and balloon is triggered in monster-sprite Class

                    // Move the Character
                    OldPosition = Position;

                    // Set new state
                    state = EntityState.Frozen;

                    break;
                #endregion
                #region state frozen
                case EntityState.Frozen:

                    // Upward Position
                    Velocity.Y += (float)gameTime.ElapsedGameTime.TotalSeconds * 2;

                    // Move the Character
                    OldPosition = Position;

                    // Apply Gravity + jumping
                    if (Velocity.Y < -1.2f && OldPosition.Y != Position.Y)
                    {
                        // Apply jumping
                        Position += Velocity * 350 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Walk / Jump speed
                        Position += Direction * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        landed = false;
                        state = EntityState.Falling;
                        Direction = Vector2.Zero;
                        Velocity = Vector2.Zero;
                        this.transperancy = 1;
                    }

                    break;
                #endregion
            }

            if (previousState != State || previousDirection != Direction)
            {
                fromServerToClient();
            }

            // Timebased Server update, to avoid lag
            displayTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (displayTimer <= 0)
            {
                displayTimer = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.25f;

                if (state == EntityState.Walk || 
                    state == EntityState.Ladder || 
                    state == EntityState.Rope)
                    fromServerToClient();
            }

            // reset client actions
            Client_action = "";
        }

        public void fromServerToClient()
        {
            playerData playerSprite = PlayerStore.Instance.toPlayerData(
                    PlayerStore.Instance.playerStore.Find(x => x.Name == this.Name));

            // sometimes dumps in next array.find statement
            // due to logged of client and remaining sprite ... to be investigate
            Client client = Array.Find(Server.singleton.client, x => x.AccountID == playerSprite.AccountID);

            playerSprite.Action = "Sprite_Update";

            playerSprite.PositionX = (float)this.Position.X;
            playerSprite.PositionY = (float)this.Position.Y;
            playerSprite.spritestate = this.State.ToString();
            playerSprite.mapName = this.mapName.ToString();

            playerSprite.prevspriteframe = this.prevspriteframe;
            playerSprite.maxspriteframe = this.maxspriteframe;
            playerSprite.attackSprite = this.attackSprite;
            playerSprite.spriteEffect = this.spriteEffect.ToString();
            playerSprite.direction = this.Direction.ToString();

            playerSprite.skincol = this.Player.skin_color.ToString();
            playerSprite.hailcol = this.Player.hair_color.ToString();
            playerSprite.facespr = this.Player.faceset_sprite.ToString();
            playerSprite.hairspr = this.Player.hair_sprite.ToString();

            playerSprite.armor = this.armor_name;
            playerSprite.headgear = this.headgear_name;
            playerSprite.weapon = this.weapon_name;

            Server.singleton.SendObject(playerSprite);
        }

        public void fromClientToServer(playerData player)
        {
            Client_action = player.Action;
            
            prevspriteframe = player.prevspriteframe;
            maxspriteframe = player.maxspriteframe;
            attackSprite = player.attackSprite;
            spriteEffect = (SpriteEffects)Enum.Parse(typeof(SpriteEffects), player.spriteEffect);
            MapName = player.mapName;
            //state = (EntityState)Enum.Parse(typeof(EntityState), player.spritestate);
            //Direction = getVector(player.direction);

            this.Player.skin_color = getColor(player.skincol);
            this.Player.faceset_sprite = player.facespr;
            this.Player.hair_sprite = player.hairspr;
            this.Player.hair_color = getColor(player.hailcol);

            this.armor_name = player.armor;
            this.headgear_name = player.headgear;
            this.weapon_name = player.weapon;
        }

        private Color getColor(string colorcode)
        {
            string[] values = colorcode.Split(':');

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim(new char[] { ' ', 'R', 'G', 'B', 'A', '{', '}' });
            }

            return new Color(
                Convert.ToInt32(values[1]),
                Convert.ToInt32(values[2]),
                Convert.ToInt32(values[3]));
        }

        private Vector2 getVector(string vectorstr)
        {
            string[] values = vectorstr.Split(':');

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim(new char[] { ' ', 'X', 'Y', '{', '}' });
                values[i] = values[i].Replace('.', '&')
                                     .Replace(',', '.')
                                     .Replace('&', ',');
            }

            return new Vector2(
                float.Parse(values[1]),
                float.Parse(values[2]));
        }

        public static PlayerSprite PlayerToSprite(playerData player)
        {
            PlayerSprite sprite = new PlayerSprite
                (
                player.Name,
                player.IP,
                player.PositionX,
                player.PositionY,
                player.spritename,
                player.spritestate,
                player.prevspriteframe,
                player.maxspriteframe,
                player.attackSprite,
                player.spriteEffect,
                player.mapName,
                player.skincol,
                player.facespr,
                player.hairspr,
                player.hailcol,
                player.armor,
                player.headgear,
                player.weapon
                );

            return sprite;
        }

    }
}
