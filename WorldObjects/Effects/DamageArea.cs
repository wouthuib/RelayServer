using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RelayServer.WorldObjects.Entities;
using RelayServer.ClientObjects;
using RelayServer.Database.Players;
using RelayServer.WorldObjects.Structures;

namespace RelayServer.WorldObjects.Effects
{
    class DamageArea : GameEffect
    {
        #region properties

        string Owner;
        Rectangle Area = Rectangle.Empty;
        float DamagePercent;
        bool Permanent = false;

        // skill hit sprite properties
        private bool hiteffect = false;
        private string hitsprpath = null;
        private int hitsprframes = 0, MobHitCount = 0, MaxMobHitCount = 0;
        private List<Guid> MobHitID = new List<Guid>();

        #endregion

        public DamageArea(
            string owner, 
            Vector2 position,
            Rectangle area,
            bool permanent,
            int maxMobHitCount,
            float timer,
            float dmgpercent,
            bool gethiteffect = false,
            string gethitsprpath = null,
            int gethitsprframes = 0)
            : base()
        {
            this.Owner = owner;
            this.Area = area;
            this.Position = position;
            this.SpriteFrame = new Rectangle(0, 0, area.Width, area.Height);
            this.DamagePercent = dmgpercent;
            this.Permanent = permanent;
            this.MaxMobHitCount = maxMobHitCount;

            transperant = 0.5f;
            keepAliveTimer = timer;

            if (gethiteffect)
            {
                this.hiteffect = true;
                this.hitsprpath = gethitsprpath;
                this.hitsprframes = gethitsprframes;
            }
        }

        public override void Update(GameTime gameTime)
        {
            // check for monster collisions
            foreach (Entity entity in GameWorld.Instance.listEntity)
            {
                if (entity is MonsterSprite &&
                    ((MobHitID.FindAll(x => x == entity.InstanceID).Count == 0) || this.Permanent == true) &&
                    (this.MobHitCount < this.MaxMobHitCount || this.Permanent == true)
                   )
                {
                    MonsterSprite monster = entity as MonsterSprite;
                    PlayerInfo player = null;

                    if (PlayerStore.Instance.playerStore.FindAll(x => x.Name == Owner).Count > 0)
                        player = PlayerStore.Instance.playerStore.Find(x => x.Name == Owner);

                    if (monster != null && player != null)

                        if (monster.SpriteBoundries.Intersects(
                            new Rectangle(
                                (int)Position.X, (int)Position.Y,
                                (int)SpriteFrame.Width, (int)SpriteFrame.Height))
                            )
                        {
                            if (monster.State != EntityState.Hit &&
                                monster.State != EntityState.Died &&
                                monster.State != EntityState.Spawn)
                            {
                                // add monster to the hit list
                                MobHitID.Add(monster.InstanceID);
                                MobHitCount++;

                                // remove effect if not permanent
                                if (Permanent == false)
                                    KeepAliveTimer = 0;

                                // Start damage controll
                                int damage = (int)Battle.battle_calc_damage(player, (MonsterSprite)monster, DamagePercent);
                                monster.HP -= damage;

                                // start skill hit effect
                                if (this.hiteffect)
                                {
                                    // send effect to clients
                                    EffectData effect = new EffectData
                                    {
                                        Name = "DamageEffect",
                                        Path = hitsprpath,
                                        PositionX = (int)(monster.Position.X),
                                        PositionY = (int)(monster.Position.Y)
                                    };

                                    Server.singleton.SendObject(effect);
                                }

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

            // base Effect Update
            base.Update(gameTime);
        }
    }
}
