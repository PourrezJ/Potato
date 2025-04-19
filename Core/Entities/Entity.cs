using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Stats;

namespace Potato.Core.Entities
{
    public abstract class Entity
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public bool IsActive { get; set; }
        public bool IsDead { get; protected set; }
        public Rectangle Bounds { get; protected set; }
        public StatsComponent Stats { get; protected set; }
        protected Texture2D _texture;
        protected Game _game;

        public Entity()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Rotation = 0f;
            IsActive = true;
            IsDead = false;
            Stats = new StatsComponent();
        }

        public virtual void Initialize()
        {
            _game = Game1.Instance;
            LoadContent();
        }

        protected virtual void LoadContent()
        {
            // Load textures and other content here
        }

        public virtual void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update position based on velocity
            Position += Velocity * deltaTime;

            // Update bounding box
            UpdateBounds();
        }

        protected virtual void UpdateBounds()
        {
            if (_texture != null)
            {
                Bounds = new Rectangle(
                    (int)Position.X - _texture.Width / 2,
                    (int)Position.Y - _texture.Height / 2,
                    _texture.Width,
                    _texture.Height);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive && _texture != null)
            {
                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.White,
                    Rotation,
                    new Vector2(_texture.Width / 2, _texture.Height / 2),
                    1.0f,
                    SpriteEffects.None,
                    0);
            }
        }

        public virtual void TakeDamage(float damage)
        {
            if (!IsDead)
            {
                Stats.Health -= damage;
                
                if (Stats.Health <= 0)
                {
                    Die();
                }
            }
        }

        protected virtual void Die()
        {
            IsDead = true;
            IsActive = false;
        }

        public virtual bool Intersects(Entity other)
        {
            return Bounds.Intersects(other.Bounds);
        }

        public virtual void Reset()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Rotation =.0f;
            IsActive = true;
            IsDead = false;
            Stats.Reset();
        }
    }
}