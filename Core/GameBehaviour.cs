using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Logging;
using System;

namespace Potato.Core
{
    /// <summary>
    /// Classe de base inspirée du MonoBehaviour de Unity, implémentant un cycle de vie similaire
    /// </summary>
    public abstract class GameBehaviour
    {
        protected GameManager _game;
        protected bool _isActive = false;
        protected bool _isAwakeCalled = false;
        protected bool _isStartCalled = false;
        
        // Propriété de priorité pour contrôler l'ordre d'exécution
        private int _executionOrder = 0;
        
        /// <summary>
        /// Priorité d'exécution du comportement. Les valeurs plus basses sont exécutées en premier.
        /// Similaire à l'attribut ExecutionOrder de Unity.
        /// </summary>
        public int ExecutionOrder 
        { 
            get => _executionOrder;
            set 
            {
                if (_executionOrder != value)
                {
                    _executionOrder = value;
                    // Informer le BehaviourManager que la priorité a changé
                    BehaviourManager.RequestSortBehaviours();
                }
            }
        }
        
        public bool IsActive => _isActive;
        
        /// <summary>
        /// Appelé une seule fois lors de l'initialisation
        /// Équivalent de Awake dans Unity
        /// </summary>
        public virtual void Awake() 
        {
            _isAwakeCalled = true;
            //Logger.Instance.Debug($"[{GetType().Name}] Awake appelé", LogCategory.Core);
        }
        
        /// <summary>
        /// Appelé une seule fois lorsque l'objet devient actif
        /// Équivalent de Start dans Unity
        /// </summary>
        public virtual void Start() 
        {
            _isStartCalled = true;
            //Logger.Instance.Debug($"[{GetType().Name}] Start appelé", LogCategory.Core);
        }
        
        /// <summary>
        /// Appelé à chaque frame lorsque l'objet est actif
        /// Équivalent de Update dans Unity
        /// </summary>
        public virtual void Update(GameTime gameTime) { }
        
        /// <summary>
        /// Appelé après Update pour gérer le rendu
        /// Équivalent de OnGUI dans Unity
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch) { }
        
        /// <summary>
        /// Appelé quand l'objet devient actif
        /// Équivalent de OnEnable dans Unity
        /// </summary>
        public virtual void OnEnable()
        {
            _isActive = true;
            //Logger.Instance.Debug($"[{GetType().Name}] Activé", LogCategory.Core);
        }
        
        /// <summary>
        /// Appelé quand l'objet devient inactif
        /// Équivalent de OnDisable dans Unity
        /// </summary>
        public virtual void OnDisable()
        {
            _isActive = false;
            //Logger.Instance.Debug($"[{GetType().Name}] Désactivé", LogCategory.Core);
        }
        
        /// <summary>
        /// Appelé lors de la destruction de l'objet
        /// Équivalent de OnDestroy dans Unity
        /// </summary>
        public virtual void OnDestroy()
        {
            //Logger.Instance.Debug($"[{GetType().Name}] Détruit", LogCategory.Core);
        }
        
        /// <summary>
        /// Active l'objet et appelle les méthodes du cycle de vie appropriées
        /// </summary>
        public void Enable()
        {
            if (!_isAwakeCalled)
            {
                Awake();
            }
            
            OnEnable();
            
            if (!_isStartCalled && _isActive)
            {
                Start();
            }
        }
        
        /// <summary>
        /// Désactive l'objet et appelle la méthode du cycle de vie appropriée
        /// </summary>
        public void Disable()
        {
            OnDisable();
        }
        
        /// <summary>
        /// Détruit l'objet et appelle la méthode du cycle de vie appropriée
        /// </summary>
        public void Destroy()
        {
            if (_isActive)
            {
                OnDisable();
            }
            
            OnDestroy();
        }
    }
}