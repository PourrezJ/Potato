using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace Potato.Core
{
    /// <summary>
    /// Classe de base pour tous les composants qui peuvent être attachés à un GameObject.
    /// Similaire au Component d'Unity.
    /// </summary>
    public abstract class Component
    {
        #region Properties

        // Référence au GameObject parent de ce comportement
        private GameObject _gameObject;
        
        /// <summary>
        /// Le GameObject auquel ce comportement est attaché. Ne peut jamais être null.
        /// </summary>
        public GameObject GameObject 
        {
            get => _gameObject;
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Un GameBehaviour doit être attaché à un GameObject");
                
                _gameObject = value;
            }
        }
        
        /// <summary>
        /// Transform du GameObject auquel ce comportement est attaché. Raccourci pour GameObject.Transform.
        /// </summary>
        public Transform Transform => GameObject?.Transform;
        
        // États du composant
        public bool Enabled { get; set; } = true;
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
        /// Appelé après toutes les mises à jour.
        /// Utile pour les calculs qui dépendent des résultats d'autres mises à jour.
        /// </summary>
        public virtual void LateUpdate(GameTime gameTime) { }
        
        /// <summary>
        /// Appelé à un intervalle fixe. Idéal pour la physique.
        /// </summary>
        public virtual void FixedUpdate(GameTime gameTime) { }
        
        /// <summary>
        /// Appelé pour le rendu du composant.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch) { }
        
        /// <summary>
        /// Appelé lorsque le composant devient actif.
        /// </summary>
        public virtual void OnEnable()
        {
            Enabled = true;
        }
        
        /// <summary>
        /// Appelé lorsque le composant devient inactif.
        /// </summary>
        public virtual void OnDisable()
        {
            Enabled = false;
        }
        
        /// <summary>
        /// Appelé lorsque le composant est détruit.
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// Appelé quand un autre collider entre en collision avec le collider de cet objet.
        /// </summary>
        public virtual void OnCollisionEnter(Component other) { }

        /// <summary>
        /// Appelé quand un autre collider reste en collision avec le collider de cet objet.
        /// </summary>
        public virtual void OnCollisionStay(Component other) { }

        /// <summary>
        /// Appelé quand un autre collider quitte la collision avec le collider de cet objet.
        /// </summary>
        public virtual void OnCollisionExit(Component other) { }

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
        /// Raccourci pour accéder à tous les composants du type T dans le GameObject.
        /// </summary>
        public T[] GetComponents<T>() where T : Component
        {
            return GameObject?.GetComponents<T>().ToArray();
        }
        
        /// <summary>
        /// Raccourci pour ajouter un composant au GameObject.
        /// </summary>
        public T AddComponent<T>() where T : Component, new()
        {
            return GameObject?.AddComponent<T>();
        }

        /// <summary>
        /// Récupère un composant du type spécifié s'il existe, sinon l'ajoute au GameObject.
        /// </summary>
        public T GetOrAddComponent<T>() where T : Component, new()
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Recherche un composant dans les objets parents.
        /// </summary>
        public T GetComponentInParent<T>() where T : Component
        {
            // Implémentation à réaliser quand la hiérarchie parent-enfant sera implémentée
            return null;
        }

        /// <summary>
        /// Recherche un composant dans les objets enfants.
        /// </summary>
        public T GetComponentInChildren<T>() where T : Component
        {
            // Implémentation à réaliser quand la hiérarchie parent-enfant sera implémentée
            return null;
        }

        #endregion
    }
}