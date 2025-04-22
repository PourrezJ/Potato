using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Potato.Core
{
    /// <summary>
    /// Classe de base inspirée du MonoBehaviour de Unity, implémentant un cycle de vie similaire.
    /// Un GameBehaviour doit toujours être attaché à un GameObject.
    /// </summary>
    public abstract class GameBehaviour : Component
    {
        protected GameManager _game;
        protected bool _isActive = false;

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
        /// Constructeur protégé pour empêcher l'instanciation directe.
        /// Les GameBehaviours doivent être créés via GameObject.AddComponent<T>().
        /// </summary>
        protected GameBehaviour()
        {
            _game = GameManager.Instance;
        }
              
        /// <summary>
        /// Active l'objet et appelle les méthodes du cycle de vie appropriées
        /// </summary>
        public void Enable()
        {
            Awake();
          
            OnEnable();
            
            Start();
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
            OnDisable();
     
            OnDestroy();
        }
    }
}