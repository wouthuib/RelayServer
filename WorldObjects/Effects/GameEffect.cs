using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RelayServer.WorldObjects.Effects
{

    public enum EffectType { DamageBaloon, ItemSprite, WeaponSwing };

    public abstract class GameEffect
    {
        #region Drawable properties
        protected Texture2D sprite;
        protected Vector2 spriteSize;
        protected Rectangle spriteFrame;
        protected Vector2 position;
        protected EffectType type;

        protected float transperant = 1;
        protected Vector2 size = Vector2.One;
        protected float angle = 0;
        protected Vector2 origin = Vector2.Zero;
        protected bool settimer = false;
        protected SpriteEffects sprite_effect = SpriteEffects.None;

        public Texture2D Sprite
        {
            get { return sprite; }
            set { sprite = value; }
        }
        public Vector2 SpriteSize
        {
            get { return spriteSize; }
            set { spriteSize = value; }
        }
        public Rectangle SpriteFrame
        {
            get { return spriteFrame; }
            set { spriteFrame = value; }
        }
        public virtual Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public EffectType Type
        {
            get { return type; }
            set { type = value; }
        }
        #endregion

        #region Timer properties
        protected float keepAliveTimer = -1;

        public float KeepAliveTimer
        {
            get { return keepAliveTimer; }
            set { keepAliveTimer = value; }
        }
        #endregion

        #region Constructor Region
        public GameEffect()
        {
        }

        public string instanceID { get; set; }

        public virtual void Update(GameTime gameTime)
        {
            if (keepAliveTimer <= 0)
            {
                if (!settimer)
                    settimer = true;
                else
                    this.keepAliveTimer = 0;
            }
            else
            {
                // Remove ItemSprite Timer
                keepAliveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (sprite != null)
                spriteBatch.Draw(sprite, new Rectangle((int)Position.X, (int)Position.Y,
                    (int)(SpriteFrame.Width * size.X), (int)(SpriteFrame.Height * size.X)),
                     SpriteFrame, Color.White * transperant, angle, origin, sprite_effect, 0f);
        }

        #endregion
    }
}
