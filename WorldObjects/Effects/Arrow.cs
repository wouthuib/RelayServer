using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Resources;
using RelayServer.WorldObjects.Entities;
using MapleLibrary;
using RelayServer.WorldObjects.Structures;
using RelayServer.Database.Players;

namespace RelayServer.WorldObjects.Effects
{
    class Arrow : GameEffect
    {
        // Drawing properties
        private Vector2 spriteOfset = new Vector2(90, 0);
        private float Speed;
        private Vector2 Direction, Curving;
        private string Shooter;                             // the name of the player shooting the arrow

        public Arrow(string shooter ,Vector2 position ,float speed, Vector2 direction, Vector2 curving)
            : base()
        {
            // Derived properties
            SpriteFrame = new Rectangle(0, 0, 60, 10);
            Position = position;
            Speed = speed + (int)(speed / 10); // slightly faster on server to avoid lag
            Direction = direction;
            this.size = new Vector2(0.5f, 0.5f);
            this.Curving = curving;

            // Link arrow to Shooter
            this.Shooter = shooter;

            // start timer and update clients
            instanceID = Guid.NewGuid().ToString();
            CreateArrow();

            if (Direction.X >= 1)
                sprite_effect = SpriteEffects.FlipHorizontally;
            else
                sprite_effect = SpriteEffects.None;
        }

        public override void Update(GameTime gameTime)
        {
            if(keepAliveTimer == -1)
                keepAliveTimer = gameTime.ElapsedGameTime.Seconds + 0.48f;

            // Arrow speed
            Position += (Direction + Curving) * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            CollisionCheck();

            // base Effect Update
            base.Update(gameTime);
        }

        private void CollisionCheck()
        {
            foreach (var entity in GameWorld.Instance.listEntity)
            {
                if (entity is MonsterSprite)
                {
                    if (new Rectangle((int)entity.Position.X + (int)(entity.SpriteFrame.Width * 0.30f),
                                          (int)entity.Position.Y,
                                          (int)entity.SpriteFrame.Width - (int)(entity.SpriteFrame.Width * 0.30f),
                                          (int)entity.SpriteFrame.Height).
                            Intersects(
                                new Rectangle((int)this.Position.X + (int)(this.SpriteFrame.Width * 0.45f),
                                    (int)this.Position.Y + (int)(this.SpriteFrame.Height * 0.45f),
                                    (int)this.SpriteFrame.Width - (int)(this.SpriteFrame.Width * 0.45f),
                                    (int)this.SpriteFrame.Height - (int)(this.SpriteFrame.Height * 0.45f))))
                    {
                        // make the monster suffer :-)
                        // and remove the arrow
                        if (entity.State != EntityState.Died &&
                            entity.State != EntityState.Spawn)
                        {
                            MonsterSprite monster = entity as MonsterSprite;
                            int damage = 0;

                            // remove arrow
                            RemoveArrow();

                            // Start damage controll
                            if (PlayerStore.Instance.playerStore.FindAll(x => x.Name == this.Shooter).Count > 0)
                            {
                                PlayerInfo player = PlayerStore.Instance.playerStore.Find(x => x.Name == this.Shooter);
                                damage = (int)Battle.battle_calc_damage(player, (MonsterSprite)monster, 100);
                                monster.HP -= damage;
                                monster.player_last_hit = this.Shooter;
                            }

                            // start skill hit effect
                            //if (this.hiteffect)
                            //{
                            //    // send effect to clients
                            //    EffectData effect = new EffectData
                            //    {
                            //        Name = "DamageEffect",
                            //        Path = hitsprpath,
                            //        PositionX = (int)(monster.Position.X),
                            //        PositionY = (int)(monster.Position.Y)
                            //    };

                            //    Server.singleton.SendObject(effect);
                            //}

                            // create damage balloon
                            EffectData baloon = new EffectData
                            {
                                Name = "DamageBaloon",
                                Path = @"gfx\effects\damage_counter1",
                                PositionX = (int)((monster.Position.X + monster.SpriteFrame.Width * 0.45f) - damage.ToString().Length * 5),
                                PositionY = (int)(monster.Position.Y + monster.SpriteFrame.Height * 0.20f),
                                Value_01 = damage
                            };

                            Server.singleton.SendObject(baloon);

                            // change monster hit
                            monster.State = EntityState.Hit;
                        }
                    }
                }
            }
        }

        private void RemoveArrow()
        {
            // stop timer
            this.keepAliveTimer = 0;

            // send effect to clients
            EffectData effect = new EffectData
            {
                Name = "DeleteArrow",
                InstanceID = instanceID,
                PositionX = (int)(position.X),
                PositionY = (int)(position.Y),
                Value_01 = (int)this.Direction.X
            };

            Server.singleton.SendObject(effect);
        }

        private void CreateArrow()
        {
            // initize timer
            this.keepAliveTimer = -1;

            // send effect to clients
            EffectData effect = new EffectData
            {
                Name = "AddArrow",
                InstanceID = instanceID,
                PositionX = (int)(position.X),
                PositionY = (int)(position.Y),
                Value_01 = (int)this.Direction.X
            };

            Server.singleton.SendObject(effect);
        }
    }
}
