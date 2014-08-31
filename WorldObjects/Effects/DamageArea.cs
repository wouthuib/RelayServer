﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RelayServer.WorldObjects.Entities;

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

                    if (monster != null)

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
                                // int damage = (int)Battle.battle_calc_damage(PlayerStore.Instance.activePlayer, (MonsterSprite)monster, DamagePercent);
                                // monster.HP -= damage;

                                // start skill hit effect
                                if (this.hiteffect)
                                {
                                    //GameWorld.Instance.newEffect.Add(new WeaponHitEffect(this.hitsprpath, monster.Position, this.hitsprframes));

                                    // send effect to clients
                                }

                                // create damage balloon
                                //GameWorld.GetInstance.newEffect.Add(new DamageBaloon(
                                //        ResourceManager.GetInstance.Content.Load<Texture2D>(@"gfx\effects\damage_counter1"),
                                //        new Vector2((monster.Position.X + monster.SpriteFrame.Width * 0.45f) - damage.ToString().Length * 5,
                                //        monster.Position.Y + monster.SpriteFrame.Height * 0.20f), damage));

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
