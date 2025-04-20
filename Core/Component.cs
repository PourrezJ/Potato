using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Potato.Core
{
    /// <summary>
    /// Classe de base pour tous les composants qui peuvent être attachés à un GameObject.
    /// Similaire au Component d'Unity.
    /// </summary>
    public abstract class Component
    {
        #region Properties

        // Référence au GameObject parent
        public GameObject GameObject { get; internal set; }
        
        // Raccourci vers le Transform du GameObject
        public Transform Transform => GameObject?.Transform;
        
        // États du composant
        public bool IsEnabled { get; private set; } = true;
        internal bool HasAwoken { get; private set; } = false;
        internal bool HasStarted { get; private set; } = false;

        #endregion

        #region Lifecycle Methods

        /// <summary>
        /// Appelé lorsque le composant est créé.
        /// Utilisé pour l'initialisation des variables.
        /// </summary>
        public virtual void Awake()
        {
            HasAwoken = true;
        }
        
        /// <summary>
        /// Appelé avant la première mise à jour, après Awake.
        /// Utilisé pour l'initialisation qui nécessite que les autres composants soient initialisés.
        /// </summary>
        public virtual void Start()
        {
            HasStarted = true;
        }
        
        /// <summary>
        /// Appelé à chaque image lorsque le composant est actif.
        /// </summary>
        public virtual void Update(GameTime gameTime) { }
        
        /// <summary>
        /// Appelé pour le rendu du composant.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch) { }
        
        /// <summary>
        /// Appelé lorsque le composant devient actif.
        /// </summary>
        public virtual void OnEnable()
        {
            IsEnabled = true;
        }
        
        /// <summary>
        /// Appelé lorsque le composant devient inactif.
        /// </summary>
        public virtual void OnDisable()
        {
            IsEnabled = false;
        }
        
        /// <summary>
        /// Appelé lorsque le composant est détruit.
        /// </summary>
        public virtual void OnDestroy() { }

        #endregion

        #region Component Management

        /// <summary>
        /// Raccourci pour accéder aux composants du GameObject.
        /// </summary>
        public T GetComponent<T>() where T : Component
        {
            return GameObject?.GetComponent<T>();
        }
        
        /// <summary>
        /// Raccourci pour ajouter un composant au GameObject.
        /// </summary>
        public T AddComponent<T>() where T : Component, new()
        {
            return GameObject?.AddComponent<T>();
        }

        #endregion
    }
}