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
        
        // Hiérarchie parent-enfant
        private GameObject _parent;
        public GameObject Parent
        {
            get => _parent;
            set
            {
                if (_parent == value)
                    return;
                    
                // Se détacher de l'ancien parent
                if (_parent != null)
                {
                    _parent._children.Remove(this);
                }
                
                _parent = value;
                
                // S'attacher au nouveau parent
                if (_parent != null)
                {
                    _parent._children.Add(this);
                    // Mettre à jour la transformation pour refléter le nouveau parent
                    Transform.UpdateFromParent();
                }
            }
        }
        
        // Liste des enfants
        private readonly List<GameObject> _children = new List<GameObject>();
        public IReadOnlyList<GameObject> Children => _children.AsReadOnly();
        
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
                Logger.Warning("Impossible de supprimer le composant Transform d'un GameObject", LogCategory.Core);
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

        #region Hierarchy Management
        
        /// <summary>
        /// Ajoute un enfant à ce GameObject
        /// </summary>
        public void AddChild(GameObject child)
        {
            if (child == null)
                return;
                
            child.Parent = this;
        }
        
        /// <summary>
        /// Supprime un enfant de ce GameObject
        /// </summary>
        public void RemoveChild(GameObject child)
        {
            if (child == null || child.Parent != this)
                return;
                
            child.Parent = null;
        }
        
        /// <summary>
        /// Trouve un enfant par son nom
        /// </summary>
        public GameObject FindChild(string name)
        {
            return _children.FirstOrDefault(child => child.Name == name);
        }
        
        /// <summary>
        /// Recherche récursivement un GameObject par son nom dans toute la hiérarchie d'enfants
        /// </summary>
        public GameObject FindInChildren(string name)
        {
            // Recherche directe dans les enfants
            GameObject directChild = FindChild(name);
            if (directChild != null)
                return directChild;
                
            // Recherche récursive dans les enfants des enfants
            foreach (var child in _children)
            {
                GameObject found = child.FindInChildren(name);
                if (found != null)
                    return found;
            }
            
            return null;
        }
        
        /// <summary>
        /// Vérifie si cet objet est un descendant d'un autre GameObject
        /// </summary>
        public bool IsChildOf(GameObject potentialParent)
        {
            if (potentialParent == null)
                return false;
                
            GameObject current = Parent;
            while (current != null)
            {
                if (current == potentialParent)
                    return true;
                    
                current = current.Parent;
            }
            
            return false;
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
                    if (!component.HasStarted && component.Enabled)
                    {
                        component.Start();
                    }
                }
                
                // Activer tous les comportements
                foreach (var behaviour in _behaviours)
                {
                    behaviour.Enable();
                }
                
                // Propager aux enfants seulement si le parent est actif
                foreach (var child in _children)
                {
                    // Restaurer l'état précédent de l'enfant - si l'enfant était actif avant
                    // que le parent ne soit désactivé, il devrait être réactivé
                    if (child._wasActiveBeforeParentDeactivated)
                    {
                        child.SetActive(true);
                        child._wasActiveBeforeParentDeactivated = false;
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
                
                // Désactiver tous les comportements
                foreach (var behaviour in _behaviours)
                {
                    behaviour.Disable();
                }
                
                // Propager aux enfants
                foreach (var child in _children)
                {
                    // Mémoriser l'état de l'enfant avant la désactivation
                    child._wasActiveBeforeParentDeactivated = child.IsActive;
                    if (child.IsActive)
                    {
                        child.SetActive(false);
                    }
                }
            }
        }
        
        // Champ pour stocker l'état avant désactivation par le parent
        private bool _wasActiveBeforeParentDeactivated = false;
        
        /// <summary>
        /// Détruit ce GameObject
        /// </summary>
        public void Destroy()
        {
            if (IsDestroyed)
                return;
                
            IsDestroyed = true;
            
            // Détruire tous les enfants d'abord
            foreach (var child in _children.ToList())
            {
                child.Destroy();
            }
            
            // Désactiver et détruire tous les composants
            foreach (var component in _components)
            {
                if (component.Enabled)
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
            
            // Se détacher du parent
            Parent = null;
            
            // Supprimer ce GameObject de sa scène
            Scene?.UnregisterGameObject(this);
            
            // Supprimer ce GameObject du gestionnaire
            GameObjectManager.UnregisterGameObject(this);
        }
        
        /// <summary>
        /// Méthode interne pour mettre à jour tous les composants et les enfants
        /// </summary>
        internal void Update(GameTime gameTime)
        {
            if (!IsActive || IsDestroyed)
                return;
                
            // Mettre à jour tous les composants
            foreach (var component in _components.ToList()) // ToList pour éviter les problèmes de modification pendant l'itération
            {
                if (component.Enabled)
                {
                    component.Update(gameTime);
                }
            }
            
            // Mettre à jour tous les enfants
            foreach (var child in _children.ToList())
            {
                child.Update(gameTime);
            }
            
            // Note: Les comportements sont mis à jour par le BehaviourManager,
            // pas par le GameObject directement
        }
        
        /// <summary>
        /// Méthode interne pour dessiner tous les composants et les enfants
        /// </summary>
        internal void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || IsDestroyed)
                return;
                
            // Dessiner tous les composants
            foreach (var component in _components.ToList())
            {
                if (component.Enabled)
                {
                    component.Draw(spriteBatch);
                }
            }
            
            // Dessiner tous les enfants
            foreach (var child in _children.ToList())
            {
                child.Draw(spriteBatch);
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
                if (component.Enabled && !component.HasStarted)
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