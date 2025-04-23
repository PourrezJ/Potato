using Microsoft.Xna.Framework;
using Potato.Core.Enemies;
using Potato.Core.Logging;
using System;
using Potato.Core;

namespace Potato.Engine
{
    /// <summary>
    /// Gère la génération de vagues d'ennemis et leur progression.
    /// Cette classe est maintenant un GameBehaviour, ce qui signifie qu'elle doit être attachée à un GameObject
    /// et être initialisée par les scènes.
    /// </summary>
    public class WaveManager : GameBehaviour
    {
        // Propriétés des vagues
        public int CurrentWave { get; private set; }
        public float WaveTimer { get; private set; }
        public bool IsBetweenWaves { get; private set; }
        public bool IsReadyForNextWave { get; private set; }
        
        // Événement déclenché quand une vague est terminée
        public event Action<int> OnWaveCompleted;
        
        // Variables privées
        private readonly Random _random;
        private readonly float _enemySpawnInterval = 2.0f;
        private float _enemySpawnTimer;
        
        // Instance statique pour la transition
        private static WaveManager _instance;
        public static WaveManager Instance => _instance;
        
        // Calcule la durée d'une vague en fonction de son numéro
        public float GetWaveDuration(int waveNumber)
        {
            // Première vague: 20 secondes, puis +5 secondes par vague
            return 20.0f + (waveNumber - 1) * 5.0f;
        }

        public float RemainingWaveTime
        {
            get
            {
                if (IsBetweenWaves)
                    return 0;
                return Math.Max(0, GetWaveDuration(CurrentWave) - WaveTimer);
            }
        }
        
        public WaveManager()
        {
            CurrentWave = 1;
            WaveTimer = 0;
            IsBetweenWaves = false;
            IsReadyForNextWave = false;
            _random = new Random();
            _enemySpawnTimer = 0;
            
            // Conserver une référence à l'instance pour la transition
            _instance = this;
        }
        
        /// <summary>
        /// Appelé lorsque le GameObject est activé
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();
            Logger.Info("WaveManager activé", LogCategory.Gameplay);
        }
        
        /// <summary>
        /// Appelé lorsque le GameObject est désactivé
        /// </summary>
        public override void OnDisable()
        {
            base.OnDisable();
            Logger.Info("WaveManager désactivé", LogCategory.Gameplay);
        }
        
        /// <summary>
        /// Appelé à chaque frame lorsque le GameObject est actif
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Si le WaveManager n'est pas actif, ne rien faire
            if (!IsActive)
                return;
                
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Debug fixe pour identifier l'état des vagues à chaque update
            if (gameTime.TotalGameTime.TotalSeconds % 3 < 0.016f) // Afficher environ toutes les 3 secondes
            {
                Logger.Debug($"[WaveManager] État: Vague={CurrentWave}, IsBetweenWaves={IsBetweenWaves}, Timer={WaveTimer:F2}, Ennemis={_game.Enemies.Count}", LogCategory.Gameplay);
            }

            // Si on est entre deux vagues, ne rien mettre à jour
            if (IsBetweenWaves)
                return;

            // Update wave timer
            WaveTimer += deltaTime;

            // Spawn enemies
            _enemySpawnTimer -= deltaTime;
            if (_enemySpawnTimer <= 0)
            {
                SpawnRandomEnemy();
                _enemySpawnTimer = _enemySpawnInterval / (1 + CurrentWave * 0.1f); // Spawn faster as waves progress
            }
            
            // Check if wave should end - mais seulement si on n'est pas déjà entre les vagues
            if (!IsBetweenWaves && (WaveTimer > GetWaveDuration(CurrentWave) || _game.Enemies.Count == 0))
            {
                System.Diagnostics.Debug.WriteLine($"[WAVE-END] Condition de fin de vague: Timer={WaveTimer:F2}, Durée={GetWaveDuration(CurrentWave):F2}, Ennemis={_game.Enemies.Count}");
                // Forcer la fin de vague et l'ouverture du shop
                EndWave();
            }
        }
        
        /// <summary>
        /// Met à jour uniquement le timer entre les vagues, sans mettre à jour les entités du jeu.
        /// Utilisée quand le joueur est dans l'écran du shop pour éviter les interactions de gameplay.
        /// </summary>
        public void UpdateBetweenWavesTimerOnly(GameTime gameTime)
        {
            // Ne faire la mise à jour que si nous sommes entre les vagues
            if (!IsBetweenWaves)
                return;
                
            // NOTE: La vague suivante ne démarre plus automatiquement 
            // C'est au joueur de décider quand démarrer la vague suivante
            // via le bouton "START NEXT WAVE" dans le shop
        }
        
        public void StartWave(int waveNumber)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] StartWave({waveNumber}) appelée");
            CurrentWave = waveNumber;
            WaveTimer = 0;
            
            // Vérifier que GameManager n'est pas null avant d'accéder à ses propriétés
            if (_game != null)
            {
                // Nettoyer TOUS les collectibles au démarrage d'une nouvelle vague
                if (_game.Collectibles != null)
                {
                    _game.Collectibles.Clear();
                    System.Diagnostics.Debug.WriteLine("[CLEANUP] Tous les collectibles ont été supprimés au démarrage de la vague");
                }
                
                // Clear any remaining enemies
                if (_game.Enemies != null)
                {
                    _game.Enemies.Clear();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ERREUR] GameManager est null dans StartWave!");
            }
            
            // Reset spawn timer
            _enemySpawnTimer = 0;
            
            // Wave is no longer ending
            IsBetweenWaves = false;
            IsReadyForNextWave = false;
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Vague {waveNumber} démarrée. IsBetweenWaves={IsBetweenWaves}");
        }
        
        /// <summary>
        /// Force le jeu à entrer dans l'état "entre deux vagues" et notifie les écouteurs.
        /// Cette méthode est appelée explicitement pour garantir l'ouverture du shop.
        /// </summary>
        private void EndWave()
        {
            // Ne rien faire si on est déjà entre deux vagues
            if (IsBetweenWaves)
            {
                Logger.Warning("[WaveManager] ForceEndWaveAndOpenShop appelée mais IsBetweenWaves est déjà true", LogCategory.Gameplay);
                return;
            }
                
            try
            {
                // Marquer que nous sommes entre les vagues
                IsBetweenWaves = true;
                IsReadyForNextWave = true;
                     
                // Déclencher l'événement OnWaveCompleted pour notifier Game1
                if (OnWaveCompleted != null)
                {
                    OnWaveCompleted.Invoke(CurrentWave);
                }
                else
                {
                    Logger.Warning("[WaveManager] OnWaveCompleted est null, personne n'est abonné à l'événement!", LogCategory.Gameplay);
                }
                
                System.Diagnostics.Debug.WriteLine($"[FORCE-SHOP] ✅ Fin de vague pour la vague {CurrentWave}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"[WaveManager] Exception dans ForceEndWaveAndOpenShop: {ex.Message}", LogCategory.Gameplay);
                Logger.Exception(ex);
            }
        }
                
        private void SpawnRandomEnemy()
        {
            // DÉBOGAGE: Vérifier si cette méthode est appelée même pendant l'état IsBetweenWaves
            System.Diagnostics.Debug.WriteLine($"[DEBUG-SPAWN] SpawnRandomEnemy appelée - IsBetweenWaves={IsBetweenWaves}, Wave={CurrentWave}");
            
            if (_game == null)
                return;
                    
            // PROTECTION: Ne pas créer d'ennemis entre les vagues
            if (IsBetweenWaves)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG-SPAWN] ⚠️ Tentative de création d'ennemi entre les vagues - BLOQUÉE!");
                return;
            }
                
            // Get viewport dimensions for spawn position calculation
            var viewport = _game.GraphicsDevice.Viewport;
            
            // Determine random edge position
            Vector2 spawnPosition;
            int edge = _random.Next(4); // 0: top, 1: right, 2: bottom, 3: left
            
            switch (edge)
            {
                case 0: // Top
                    spawnPosition = new Vector2(_random.Next(viewport.Width), -50);
                    break;
                case 1: // Right
                    spawnPosition = new Vector2(viewport.Width + 50, _random.Next(viewport.Height));
                    break;
                case 2: // Bottom
                    spawnPosition = new Vector2(_random.Next(viewport.Width), viewport.Height + 50);
                    break;
                case 3: // Left
                    spawnPosition = new Vector2(-50, _random.Next(viewport.Height));
                    break;
                default:
                    spawnPosition = new Vector2(0, 0);
                    break;
            }
            
            // Create the enemy
            BasicEnemy enemy = BasicEnemy.CreateRandomEnemy(_game, spawnPosition);
            _game.SpawnEnemy(enemy);
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG-SPAWN] ✅ Ennemi créé à la position {spawnPosition}, total: {_game.Enemies.Count}");
        }
        
        /// <summary>
        /// Réinitialise le WaveManager à son état initial
        /// </summary>
        public void Reset()
        {
            CurrentWave = 1;
            WaveTimer = 0;
            IsBetweenWaves = false;
            IsReadyForNextWave = false;
        }
    }
}