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
    /// Sert de conteneur pour les composants et gère le cycle de vie de l'objet.
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
        
        // Liste des composants attachés
        private readonly List<Component> _components = new List<Component>();
        
        // Référence au GameManager
        protected GameManager _game;

        #endregion

        #region Constructors

        public GameObject(string name = "GameObject")
        {
            Id = Guid.NewGuid();
            Name = name;
            _game = GameManager.Instance;
            
            // Tout GameObject a automatiquement un Transform
            Transform = AddComponent<Transform>();
            
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
                Logger.Instance.Warning("Cannot remove Transform component from GameObject", LogCategory.Core);
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
            }
            else
            {
                // Désactiver tous les composants
                foreach (var component in _components)
                {
                    component.OnDisable();
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
            
            // Supprimer ce GameObject du gestionnaire
            GameObjectManager.UnregisterGameObject(this);
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
        }

        #endregion
    }
}