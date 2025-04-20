using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Logging;

namespace Potato.Core
{
    /// <summary>
    /// Représente un objet de jeu similaire à GameObject d'Unity.
    /// Sert de conteneur pour les composants et comportements, et gère le cycle de vie de l'objet.
    /// </summary>
    public class GameObject
    {
        #region Properties

        // Identifiant unique
        public string Name { get; set; }
        public Guid Id { get; private set; }
        
        // État de l'objet
        public bool IsActive { get; private set; } = true;
        public bool IsDestroyed { get; private set; } = false;
        
        // Composant de transformation (toujours présent)
        public Transform Transform { get; private set; }
        
        // Tag pour le filtrage
        public string Tag { get; set; } = "Untagged";
        
        // Référence à la scène à laquelle appartient cet objet
        private Scene _scene;
        public Scene Scene
        {
            get => _scene;
            internal set
            {
                if (_scene != value)
                {
                    // Désenregistrer de l'ancienne scène
                    _scene?.UnregisterGameObject(this);
                    
                    // Enregistrer dans la nouvelle scène
                    _scene = value;
                    _scene?.RegisterGameObject(this);
                }
            }
        }
        
        // Liste des composants attachés
        private readonly List<Component> _components = new List<Component>();
        
        // Liste des comportements attachés
        private readonly List<GameBehaviour> _behaviours = new List<GameBehaviour>();
        
        // Référence au GameManager
        protected GameManager _game;

        #endregion

        #region Constructors

        public GameObject(string name = "GameObject", Scene scene = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            _game = GameManager.Instance;
            
            // Tout GameObject a automatiquement un Transform
            Transform = AddComponent<Transform>();
            
            // Associer à la scène spécifiée ou à la scène active
            Scene = scene ?? SceneManager.ActiveScene;
            
            // Enregistrer ce GameObject dans le gestionnaire
            GameObjectManager.RegisterGameObject(this);
        }

        #endregion

        #region Component Management

        /// <summary>
        /// Ajoute un composant du type spécifié au GameObject
        /// </summary>
        public T AddComponent<T>() where T : Component, new()
        {
            // Créer une nouvelle instance du composant
            T component = new T();
            
            // Initialiser le composant
            component.GameObject = this;
            
            // Ajouter le composant à la liste
            _components.Add(component);
            
            // Réveiller le composant
            if (IsActive && !IsDestroyed)
            {
                component.Awake();
                component.OnEnable();
                component.Start();
            }
            
            return component;
        }
        
        /// <summary>
        /// Obtient le premier composant du type spécifié attaché à ce GameObject
        /// </summary>
        public T GetComponent<T>() where T : Component
        {
            return _components.OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Obtient tous les composants du type spécifié attachés à ce GameObject
        /// </summary>
        public IEnumerable<T> GetComponents<T>() where T : Component
        {
            return _components.OfType<T>();
        }
        
        /// <summary>
        /// Supprime un composant du GameObject
        /// </summary>
        public void RemoveComponent<T>(T component) where T : Component
        {
            if (component is Transform)
            {
                Logger.Instance.Warning("Impossible de supprimer le composant Transform d'un GameObject", LogCategory.Core);
                return;
            }
            
            if (_components.Contains(component))
            {
                component.OnDisable();
                component.OnDestroy();
                _components.Remove(component);
            }
        }

        #endregion

        #region Behaviour Management

        /// <summary>
        /// Ajoute un comportement du type spécifié au GameObject
        /// </summary>
        public T AddBehaviour<T>() where T : GameBehaviour, new()
        {
            // Créer une nouvelle instance du comportement
            T behaviour = new T();
            
            // Initialiser le comportement
            behaviour.GameObject = this;
            
            // Ajouter le comportement à la liste
            _behaviours.Add(behaviour);
            
            // Enregistrer le comportement dans le BehaviourManager
            BehaviourManager.RegisterBehaviour(behaviour);
            
            // Réveiller le comportement
            if (IsActive && !IsDestroyed)
            {
                behaviour.Enable();
            }
            
            return behaviour;
        }
        
        /// <summary>
        /// Obtient le premier comportement du type spécifié attaché à ce GameObject
        /// </summary>
        public T GetBehaviour<T>() where T : GameBehaviour
        {
            return _behaviours.OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Obtient tous les comportements du type spécifié attachés à ce GameObject
        /// </summary>
        public IEnumerable<T> GetBehaviours<T>() where T : GameBehaviour
        {
            return _behaviours.OfType<T>();
        }
        
        /// <summary>
        /// Supprime un comportement du GameObject
        /// </summary>
        public void RemoveBehaviour<T>(T behaviour) where T : GameBehaviour
        {
            if (_behaviours.Contains(behaviour))
            {
                behaviour.Disable();
                behaviour.Destroy();
                _behaviours.Remove(behaviour);
                
                // Désenregistrer le comportement du BehaviourManager
                BehaviourManager.UnregisterBehaviour(behaviour);
            }
        }

        #endregion

        #region Lifecycle Management

        /// <summary>
        /// Active ou désactive ce GameObject
        /// </summary>
        public void SetActive(bool active)
        {
            if (IsActive == active || IsDestroyed)
                return;
                
            IsActive = active;
            
            if (IsActive)
            {
                // Activer tous les composants
                foreach (var component in _components)
                {
                    component.OnEnable();
                    
                    // Si Start n'a pas encore été appelé, l'appeler maintenant
                    if (!component.HasStarted && component.IsEnabled)
                    {
                        component.Start();
                    }
                }
                
                // Activer tous les comportements
                foreach (var behaviour in _behaviours)
                {
                    behaviour.Enable();
                }
            }
            else
            {
                // Désactiver tous les composants
                foreach (var component in _components)
                {
                    component.OnDisable();
                }
                
                // Désactiver tous les comportements
                foreach (var behaviour in _behaviours)
                {
                    behaviour.Disable();
                }
            }
        }
        
        /// <summary>
        /// Détruit ce GameObject
        /// </summary>
        public void Destroy()
        {
            if (IsDestroyed)
                return;
                
            IsDestroyed = true;
            
            // Désactiver et détruire tous les composants
            foreach (var component in _components)
            {
                if (component.IsEnabled)
                {
                    component.OnDisable();
                }
                component.OnDestroy();
            }
            
            // Désactiver et détruire tous les comportements
            foreach (var behaviour in _behaviours)
            {
                if (behaviour.IsActive)
                {
                    behaviour.Disable();
                }
                behaviour.Destroy();
                
                // Désenregistrer le comportement du BehaviourManager
                BehaviourManager.UnregisterBehaviour(behaviour);
            }
            
            // Supprimer ce GameObject de sa scène
            Scene?.UnregisterGameObject(this);
            
            // Supprimer ce GameObject du gestionnaire
            GameObjectManager.UnregisterGameObject(this);
        }
        
        /// <summary>
        /// Marque cet objet comme persistent à travers les changements de scènes
        /// </summary>
        public void DontDestroyOnLoad()
        {
            Scene.MarkAsPersistent(this);
        }
        
        /// <summary>
        /// Méthode interne pour mettre à jour tous les composants
        /// </summary>
        internal void Update(GameTime gameTime)
        {
            if (!IsActive || IsDestroyed)
                return;
                
            foreach (var component in _components.ToList()) // ToList pour éviter les problèmes de modification pendant l'itération
            {
                if (component.IsEnabled)
                {
                    component.Update(gameTime);
                }
            }
            
            // Note: Les comportements sont mis à jour par le BehaviourManager,
            // pas par le GameObject directement
        }
        
        /// <summary>
        /// Méthode interne pour dessiner tous les composants
        /// </summary>
        internal void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || IsDestroyed)
                return;
                
            foreach (var component in _components.ToList())
            {
                if (component.IsEnabled)
                {
                    component.Draw(spriteBatch);
                }
            }
            
            // Note: Les comportements sont dessinés par le BehaviourManager,
            // pas par le GameObject directement
        }
        
        /// <summary>
        /// Réveille tous les composants
        /// </summary>
        internal void Awake()
        {
            if (IsDestroyed)
                return;
                
            foreach (var component in _components)
            {
                component.Awake();
            }
            
            // Note: Les comportements sont réveillés par le BehaviourManager
            // ou lors de l'ajout à ce GameObject
        }
        
        /// <summary>
        /// Démarre tous les composants
        /// </summary>
        internal void Start()
        {
            if (!IsActive || IsDestroyed)
                return;
                
            foreach (var component in _components)
            {
                if (component.IsEnabled && !component.HasStarted)
                {
                    component.Start();
                }
            }
            
            // Note: Les comportements sont démarrés par le BehaviourManager
            // ou lors de l'ajout à ce GameObject
        }

        #endregion
    }
}