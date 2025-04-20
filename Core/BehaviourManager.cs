using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Attributes;
using Potato.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Potato.Core
{
    /// <summary>
    /// Gestionnaire central de tous les GameBehaviours, inspiré du système de Unity
    /// </summary>
    public static class BehaviourManager
    {
        private static List<GameBehaviour> _behaviours = new List<GameBehaviour>();
        private static List<GameBehaviour> _pendingAddition = new List<GameBehaviour>();
        private static List<GameBehaviour> _pendingRemoval = new List<GameBehaviour>();
        private static bool _isProcessingLists = false;
        
        // Flag pour indiquer si les comportements doivent être triés
        private static bool _needsSort = false;
         
        /// <summary>
        /// Charge automatiquement tous les GameBehaviours présents dans l'assembly
        /// </summary>
        public static void DiscoverBehaviours()
        {
            var game = GameManager.Instance;

            try
            {
                // Obtenir l'assembly courant
                Assembly assembly = Assembly.GetExecutingAssembly();
                
                // Trouver tous les types qui héritent de GameBehaviour
                var behaviourTypes = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(GameBehaviour)) && !t.IsAbstract);
                
                int count = 0;
                foreach (var type in behaviourTypes)
                {
                    try
                    {
                        // Vérifier si ce type a déjà été instancié
                        bool alreadyExists = _behaviours.Any(b => b.GetType() == type);
                        if (alreadyExists)
                        {
                            Logger.Instance.Debug($"Comportement {type.Name} déjà instancié, ignoré", LogCategory.Core);
                            continue;
                        }
                        
                        //Logger.Instance.Debug($"Tentative de création du behaviour {type.Name}", LogCategory.Core);
                        
                        // Essayer d'abord le constructeur sans paramètre
                        var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
                        if (defaultConstructor != null)
                        {
                            var behaviour = (GameBehaviour)defaultConstructor.Invoke(null);
                            RegisterBehaviour(behaviour);
                            count++;
                            continue;
                        }
                        
                        // Puis essayer le constructeur avec Game
                        var gameConstructor = type.GetConstructor(new[] { typeof(Game) });
                        if (gameConstructor != null && game != null)
                        {
                            var behaviour = (GameBehaviour)gameConstructor.Invoke(new object[] { game });
                            RegisterBehaviour(behaviour);
                            count++;
                            continue;
                        }
                        
                        Logger.Instance.Warning($"Impossible de créer le behaviour {type.Name} : aucun constructeur compatible", LogCategory.Core);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error($"Erreur lors de la création du behaviour {type.Name}: {ex.Message}", LogCategory.Core);
                    }
                }
                
                //Logger.Instance.Info($"Découvert {count} behaviours via réflexion", LogCategory.Core);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Erreur lors de la découverte des behaviours: {ex.Message}", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Enregistre un nouveau GameBehaviour
        /// </summary>
        public static void RegisterBehaviour(GameBehaviour behaviour)
        {
            if (_isProcessingLists)
            {
                _pendingAddition.Add(behaviour);
                return;
            }
            
            if (!_behaviours.Contains(behaviour))
            {
                // Lecture de l'attribut ExecutionOrder s'il existe
                ApplyExecutionOrderAttribute(behaviour);
                
                _behaviours.Add(behaviour);
                
                // Activer le comportement pour déclencher le cycle de vie (Awake, OnEnable, Start)
                behaviour.Enable();
                
                //Logger.Instance.Debug($"Behaviour {behaviour.GetType().Name} enregistré avec priorité {behaviour.ExecutionOrder}", LogCategory.Core);
                RequestSortBehaviours();
            }
        }
        
        /// <summary>
        /// Applique l'attribut ExecutionOrderAttribute s'il est défini sur la classe
        /// </summary>
        private static void ApplyExecutionOrderAttribute(GameBehaviour behaviour)
        {
            Type type = behaviour.GetType();
            ExecutionOrderAttribute attribute = type.GetCustomAttribute<ExecutionOrderAttribute>();
            
            if (attribute != null)
            {
                behaviour.ExecutionOrder = attribute.Order;
                Logger.Instance.Debug($"Attribut ExecutionOrder {attribute.Order} appliqué à {type.Name}", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Supprime un GameBehaviour
        /// </summary>
        public static void UnregisterBehaviour(GameBehaviour behaviour)
        {
            if (_isProcessingLists)
            {
                _pendingRemoval.Add(behaviour);
                return;
            }
            
            if (_behaviours.Contains(behaviour))
            {
                behaviour.Destroy();
                _behaviours.Remove(behaviour);
                Logger.Instance.Debug($"Behaviour {behaviour.GetType().Name} désenregistré", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Demande au gestionnaire de trier les comportements lors du prochain cycle
        /// </summary>
        public static void RequestSortBehaviours()
        {
            _needsSort = true;
        }
        
        /// <summary>
        /// Trie les comportements en fonction de leur ordre d'exécution
        /// </summary>
        private static void SortBehavioursByExecutionOrder()
        {
            _behaviours = _behaviours.OrderBy(b => b.ExecutionOrder).ToList();
            _needsSort = false;
            Logger.Instance.Debug("Behaviours triés par ordre d'exécution", LogCategory.Core);
        }
        
        /// <summary>
        /// Met à jour tous les GameBehaviours actifs
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            if (_needsSort)
            {
                SortBehavioursByExecutionOrder();
            }
            
            _isProcessingLists = true;
            
            foreach (var behaviour in _behaviours)
            {
                if (behaviour.IsActive)
                {
                    try
                    {
                        behaviour.Update(gameTime);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error($"Erreur dans Update de {behaviour.GetType().Name}: {ex.Message}", LogCategory.Core);
                    }
                }
            }
            
            _isProcessingLists = false;
            
            // Traiter les listes en attente
            ProcessPendingLists();
        }
        
        /// <summary>
        /// Dessine tous les GameBehaviours actifs
        /// </summary>
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_needsSort)
            {
                SortBehavioursByExecutionOrder();
            }
            
            _isProcessingLists = true;
            
            foreach (var behaviour in _behaviours)
            {
                if (behaviour.IsActive)
                {
                    try
                    {
                        behaviour.Draw(spriteBatch);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error($"Erreur dans Draw de {behaviour.GetType().Name}: {ex.Message}", LogCategory.Core);
                    }
                }
            }
            
            _isProcessingLists = false;
            
            // Traiter les listes en attente
            ProcessPendingLists();
        }
        
        /// <summary>
        /// Traite les listes d'ajout et de suppression en attente
        /// </summary>
        private static void ProcessPendingLists()
        {
            bool needsSorting = false;
            
            // Traiter les ajouts
            if (_pendingAddition.Count > 0)
            {
                foreach (var behaviour in _pendingAddition)
                {
                    if (!_behaviours.Contains(behaviour))
                    {
                        _behaviours.Add(behaviour);
                        needsSorting = true;
                    }
                }
                _pendingAddition.Clear();
            }
            
            // Traiter les suppressions
            if (_pendingRemoval.Count > 0)
            {
                foreach (var behaviour in _pendingRemoval)
                {
                    if (_behaviours.Contains(behaviour))
                    {
                        behaviour.Destroy();
                        _behaviours.Remove(behaviour);
                    }
                }
                _pendingRemoval.Clear();
            }
            
            // Trier si nécessaire
            if (needsSorting)
            {
                RequestSortBehaviours();
            }
        }
        
        /// <summary>
        /// Obtient tous les GameBehaviours d'un type spécifique
        /// </summary>
        public static List<T> GetBehavioursOfType<T>() where T : GameBehaviour
        {
            return _behaviours.OfType<T>().ToList();
        }
        
        /// <summary>
        /// Obtient le premier GameBehaviour d'un type spécifique
        /// </summary>
        public static T GetBehaviourOfType<T>() where T : GameBehaviour
        {
            return _behaviours.OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Libère toutes les ressources
        /// </summary>
        public static void Dispose()
        {
            foreach (var behaviour in _behaviours)
            {
                try
                {
                    behaviour.Destroy();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Erreur lors de la destruction de {behaviour.GetType().Name}: {ex.Message}", LogCategory.Core);
                }
            }
            
            _behaviours.Clear();
            _pendingAddition.Clear();
            _pendingRemoval.Clear();
            
            Logger.Instance.Info("BehaviourManager détruit", LogCategory.Core);
        }
    }
}