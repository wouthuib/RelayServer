using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RelayServer.Database.Items;
using RelayServer.Database.Players;
using RelayServer.WorldObjects.Entities;
using MapleLibrary;
using System;

namespace RelayServer.WorldObjects.Effects
{
    class ItemSprite : GameEffect
    {
        #region properties
        ItemStore itemDB;
        //Inventory inventory;
        PlayerStore playerStore = PlayerStore.Instance;
        Item item;

        Vector2 spritesize = new Vector2(48, 48);
        Vector2 circleOrigin = Vector2.Zero;

        #endregion

        public ItemSprite(Vector2 position, int itemID) :
            base()
        {
            // Link properties to instance
            this.itemDB = ItemStore.Instance;

            // get item information from general DB
            item = itemDB.getItem(itemID);

            this.instanceID = System.Guid.NewGuid().ToString();

            // general properties
            this.position = position;

            // set the correct item sprite in item spritesheet
            this.spriteFrame = new Rectangle(0, 0, (int)spritesize.X, (int)spritesize.Y);

            // send effect to clients
            EffectData effect = new EffectData
            {
                Name = "AddItemSprite",                
                InstanceID = instanceID,
                PositionX = (int)(position.X),
                PositionY = (int)(position.Y),
                Value_01 = itemID
            };

            Server.singleton.SendObject(effect);
        }

        public void pickupItem(PlayerInfo player)
        {
            // add item to inventory
            player.inventory.addItem(this.item);

            // remove this sprite
            this.keepAliveTimer = 0;

            // send effect to clients
            EffectData effect = new EffectData
            {
                Name = "DeleteItemSprite",
                InstanceID = instanceID,
                PositionX = (int)(position.X),
                PositionY = (int)(position.Y),
                Value_01 = this.item.itemID
            };

            Server.singleton.SendObject(effect);
        }

        public override void Update(GameTime gameTime)
        {
            // Start ItemSprite (default is -1)
            if (keepAliveTimer < 0 && !settimer)
                keepAliveTimer = (float)gameTime.TotalGameTime.Seconds + 10;

            // check for monster collisions
            foreach (Entity entity in GameWorld.Instance.listEntity)
            {
                if (entity is PlayerSprite)
                {
                    PlayerSprite sprite = (PlayerSprite)entity;

                    if (new Rectangle((int)(sprite.Position.X + sprite.SpriteFrame.Width * 0.60f),
                        (int)sprite.Position.Y,
                        (int)(sprite.SpriteFrame.Width * 0.30f),
                        (int)sprite.SpriteFrame.Height).
                        Intersects(new Rectangle(
                            (int)Position.X, (int)Position.Y,
                            (int)SpriteFrame.Width, (int)SpriteFrame.Height)) == true && transperant >= 1)
                    {
                        pickupItem(playerStore.playerStore.Find(x => x.Name == sprite.Name));
                    }
                }
            }

            // base Effect Update
            base.Update(gameTime);
        }
    }
}
