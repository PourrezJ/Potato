using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Potato.Engine;
using Potato.Core.UI;
using Potato.Core.Logging;
using Potato.Core;
using Potato.Core.Entities;
using Potato.Core.Enemies;
using Potato.Core.Weapons;
using System;
using System.Collections.Generic;
using System.IO;
using Potato.Core.UI.Screens;

namespace Potato;

public class GameManager : Game
{
    #region Singleton

    public static GameManager Instance { get; private set; }

    #endregion

    #region MonoGame Variables

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;

    #endregion

    #region Game State

    // Game states
    public enum GameState
    {
        MainMenu,
        CharacterSelection,
        Playing,
        Shopping,
        Paused
    }

    private GameState _currentGameState;
    private bool _gameStarted;
    private bool _isGameOver;
    private int _score;

    #endregion

    #region Screens and UI

    private LogViewer _logViewer;

    #endregion

    #region Game Entities

    // Entités de jeu (auparavant dans GameManager)
    public Player Player { get; private set; }
    public List<Enemy> Enemies { get; private set; }
    public List<Weapon> Weapons { get; private set; }
    public List<Collectible> Collectibles { get; private set; }
    public bool IsGameOver => _isGameOver;
    public int Score => _score;

    #endregion

    #region Game Managers

    // Gestionnaires de jeu
    private WaveManager _waveManager;
    private MapManager _mapManager;

    // Propriétés déléguées au WaveManager
    public int Wave => _waveManager?.CurrentWave ?? 0;
    public bool IsBetweenWaves => _waveManager?.IsBetweenWaves ?? false;
    public bool IsReadyForNextWave => _waveManager?.IsReadyForNextWave ?? false;
    public float RemainingWaveTime => _waveManager?.RemainingWaveTime ?? 0f;

    #endregion
    
    #region Player Selection
    
    private Player _selectedPlayer;
    private Weapon _selectedWeapon;
    
    #endregion

    #region Constructor

    public GameManager()
    {
        if (Instance != null)
            return;

        Instance = this;

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // Configure window size
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        
        // Configure logger
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string logDir = Path.Combine(baseDir, "Logs");
        
        // Initialize Logger
        Logger.Instance.Configure(LogLevel.Debug, true, true, true);
        Logger.Instance.Info("Game initialized");
        
        // Initialize game entity collections
        Enemies = new List<Enemy>();
        Weapons = new List<Weapon>();
        Collectibles = new List<Collectible>();
        _isGameOver = false;
        _score = 0;
        _gameStarted = false;
    }

    #endregion

    #region Game Lifecycle

    protected override void Initialize()
    {
        Logger.Instance.Info("BehaviourManager initialisé", LogCategory.Core);
        
        // Initialiser le GameObjectManager
        GameObjectManager.Initialize();
        Logger.Instance.Info("GameObjectManager initialisé", LogCategory.Core);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        try
        {        
            // Initialize log viewer
            _logViewer = new LogViewer(this, _font);
            Logger.Instance.Info("LogViewer initialized");
            
            try 
            {
                // Initialiser le BehaviourManager
                BehaviourManager.DiscoverBehaviours();
                Logger.Instance.Info("Behaviors discovery completed");

                // Initialiser les gestionnaires de jeu
                _waveManager = WaveManager.Instance;
                _mapManager = MapManager.Instance;
                _waveManager.OnWaveCompleted += OnWaveCompletedHandler;

                MainMenuCanvas.Instance.CreateUI();

            }
            catch (Exception discoverEx) 
            {
                Logger.Instance.Error($"Error during behavior discovery: {discoverEx.Message}");
            }
            
            Logger.Instance.Info("All screens initialized");
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error loading DefaultFont: {ex.Message}");
            _font = null;
        }       
    }

    protected override void Update(GameTime gameTime)
    {        
        // Mettre à jour tous les GameBehaviours via le BehaviourManager
        BehaviourManager.Update(gameTime);
        
        // Mettre à jour tous les GameObjects via le GameObjectManager
        GameObjectManager.Update(gameTime);

        // Update UIManager in all states
        UIManager.Instance.Update(gameTime);
        
        // Gestion spécifique en fonction de l'état du jeu
        switch (_currentGameState)
        {
            case GameState.Playing:
                if (!_isGameOver)
                {
                    UpdateGameDuringPlay(gameTime);
                }
                break;
                
            case GameState.Shopping:
                // Mettre à jour uniquement le timer entre les vagues
                UpdateBetweenWavesTimerOnly(gameTime);
                break;
        }
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        
        // Dessiner tous les GameBehaviours via le BehaviourManager
        BehaviourManager.Draw(_spriteBatch);

        // Start sprite batch
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        // Dessiner tous les GameObjects via le GameObjectManager
        GameObjectManager.Draw(_spriteBatch);
        
        // Draw game elements first (background, map, etc.)
        if (_currentGameState == GameState.Playing || 
            _currentGameState == GameState.Shopping || 
            _currentGameState == GameState.Paused)
        {
            DrawGameElements();
        }
                     
        // Draw LogViewer on top of everything if it's visible
        _logViewer?.Draw(_spriteBatch);
        
        // End sprite batch
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    protected override void OnExiting(object sender, Microsoft.Xna.Framework.ExitingEventArgs args)
    {
        // Clean up resources before exiting
        Logger.Instance.Info("Game exiting", LogCategory.Gameplay);
        
        base.OnExiting(sender, args);
    }

    #endregion

    #region Game State Management
    
    public void SetGameState(GameState gameState)
    {
        GameState previousState = _currentGameState;
        _currentGameState = gameState;
        
        Logger.Instance.Info($"Game state changed from {previousState} to: {_currentGameState}", LogCategory.Gameplay);
        
        // Note: la gestion des canvas se fait maintenant manuellement
    }
    
    public void RestartGame()
    { 
        SetGameState(GameState.MainMenu);
        ResetGame();
    }
    
    public bool HasGameStarted() => _gameStarted;
    
    /// <summary>
    /// Starts the game, called by Character Selection screen.
    /// Initializes the player and weapon, and starts the first wave.
    /// </summary>
    public void StartGame()
    {
        // Générer d'abord une nouvelle carte pour la partie
        _mapManager.GenerateMap();
        Logger.Instance.Info("Nouvelle carte générée pour la partie", LogCategory.Gameplay);
        
        // Initialize player and weapon
        _selectedPlayer.Initialize();
        Player = _selectedPlayer;
        
        // S'assurer que le joueur est bien positionné au centre de l'écran
        Player.Position = new Vector2(
            GraphicsDevice.Viewport.Width / 2,
            GraphicsDevice.Viewport.Height / 2);
        Logger.Instance.Info($"Joueur positionné au centre: {Player.Position.X}, {Player.Position.Y}", LogCategory.Gameplay);

     
        _selectedWeapon.Initialize();
        Player.AddWeapon(_selectedWeapon);
        
        // Start wave 1
        StartWave(1);
        
        // Flag game as started
        _gameStarted = true;
        
        // Set state to playing
        SetGameState(GameState.Playing);
    }
    
    public void SetSelectedPlayer(Player player)
    {
        _selectedPlayer = player;
    }
    
    public void AddSelectedWeapon(Weapon weapon)
    {
        _selectedWeapon = weapon;
    }
    
    #endregion
    
    #region Gameplay Update Methods
    
    private void UpdateGameDuringPlay(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Gestion spécifique quand on est entre deux vagues
        if (IsBetweenWaves)
        {
            HandleBetweenWavesUpdate(gameTime, deltaTime);
            return;
        }

        // Mise à jour standard pendant une vague
        UpdateGameDuringWave(gameTime, deltaTime);
        
        // Vérification de l'état du jeu
        CheckGameState();
    }
    
    private void HandleBetweenWavesUpdate(GameTime gameTime, float deltaTime)
    {
        // N'exécuter que la mise à jour des collectibles et du joueur entre les vagues
        UpdateCollectibles(deltaTime);
        Player?.Update(gameTime);
        
        // Mettre à jour uniquement les projectiles existants
        foreach (var weapon in Weapons)
        {
            weapon.UpdateProjectilesOnly(gameTime);
        }
    }
    
    private void UpdateGameDuringWave(GameTime gameTime, float deltaTime)
    {
        // Mettre à jour le WaveManager (gère timer de vague, spawn d'ennemis, etc.)
        _waveManager.Update(gameTime);

        // Update player
        Player.Update(gameTime);

        // Update weapons
        foreach (var weapon in Weapons)
        {
            weapon.Update(gameTime);
        }

        // Update enemies
        UpdateEnemies(gameTime);
        
        // Update collectibles
        UpdateCollectibles(deltaTime);
        
        // Check player collision with collectibles
        CheckCollectiblesCollision();
    }
    
    private void UpdateEnemies(GameTime gameTime)
    {
        for (int i = Enemies.Count - 1; i >= 0; i--)
        {
            Enemies[i].Update(gameTime);
            
            // Check if enemy is dead
            if (Enemies[i].IsDead)
            {
                _score += Enemies[i].ScoreValue;
                Enemies.RemoveAt(i);
            }
        }
    }
    
    private void UpdateCollectibles(float deltaTime)
    {
        for (int i = Collectibles.Count - 1; i >= 0; i--)
        {
            Collectibles[i].Update(deltaTime);
            
            // Remove inactive collectibles
            if (!Collectibles[i].IsActive)
            {
                Collectibles.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Met à jour uniquement le timer entre les vagues, sans mettre à jour les entités du jeu.
    /// Utilisée quand le joueur est dans l'écran du shop pour éviter les interactions de gameplay.
    /// </summary>
    public void UpdateBetweenWavesTimerOnly(GameTime gameTime)
    {
        _waveManager.UpdateBetweenWavesTimerOnly(gameTime);
        
        // Continuer à mettre à jour les collectibles pour les animations
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateCollectibles(deltaTime);
    }
    
    private void CheckCollectiblesCollision()
    {
        if (Player == null || Player.IsDead)
            return;
                
        for (int i = Collectibles.Count - 1; i >= 0; i--)
        {
            if (Collectibles[i].IsActive && Player.Bounds.Intersects(Collectibles[i].Bounds))
            {
                ApplyCollectibleEffect(Collectibles[i]);
                Collectibles[i].Collect();
            }
        }
    }
    
    private void ApplyCollectibleEffect(Collectible collectible)
    {
        switch (collectible.Type)
        {
            case CollectibleType.Gold:
                Player.Gold += collectible.Value;
                break;
            case CollectibleType.Experience:
                Player.AddExperience(collectible.Value);
                break;
            case CollectibleType.Health:
                Player.Stats.Health = MathHelper.Min(Player.Stats.Health + collectible.Value, Player.Stats.MaxHealth);
                break;
        }
    }
    
    private void CheckGameState()
    {
        // Check game over condition
        if (Player.IsDead)
        {
            _isGameOver = true;
        }
    }
    
    #endregion
    
    #region Wave Management
    
    public void StartWave(int waveNumber) => _waveManager.StartWave(waveNumber);

    public void StartNextWave()
    {
        SetGameState(GameState.Playing);

        StartWave(Wave + 1);
    }
    
    /// <summary>
    /// Event handler called at the end of each wave.
    /// Opens the shop and switches to Shopping state.
    /// </summary>
    private void OnWaveCompletedHandler(int waveNumber)
    {
        Logger.Instance.Info($"Wave {waveNumber} completed - Opening shop");
        
        try
        {
            // Nettoyer la carte
            CleanupMap();
            
            // Repositionner le joueur
            ResetPlayerPosition();
   
            // Change game state
            SetGameState(GameState.Shopping);

            
            Logger.Instance.Info("Shop opened successfully");
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error opening shop: {ex.Message}");
            // In case of error, continue playing
            SetGameState(GameState.Playing);
        }
    }
    
    #endregion
    
    #region Entity Management
    
    public void AddWeapon(Weapon weapon) => Weapons.Add(weapon);

    public void AddCollectible(Collectible collectible) => Collectibles.Add(collectible);

    public void SpawnEnemy(Enemy enemy) => Enemies.Add(enemy);
    
    public List<Enemy> GetEnemies() => Enemies;
    
    #endregion

    #region Rendering Methods

    /// <summary>
    /// Dessine les éléments communs de jeu pour les états Playing, Shopping et Paused
    /// </summary>
    private void DrawGameElements()
    {
        // Draw Map
        _mapManager.Draw(_spriteBatch);
        
        // Draw collectibles first (lower layer)
        foreach (var collectible in Collectibles)
        {
            collectible.Draw(_spriteBatch);
        }
        
        // Draw player with debug information
        if (Player != null)
        {
            // Force le dessin d'un marqueur visible pour le joueur (un grand cercle rouge)
            Texture2D debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            debugTexture.SetData(new[] { Color.White });
            
            // Dessiner un grand cercle rouge à la position du joueur pour le debug
            _spriteBatch.Draw(
                debugTexture,
                new Rectangle(
                    (int)Player.Position.X - 20,
                    (int)Player.Position.Y - 20,
                    40, 
                    40),
                Color.Red);
                
            // Maintenant dessiner le joueur normalement
            Player.Draw(_spriteBatch);
            
            // Log la position du joueur pour le débogage
            if (Player.Position != Vector2.Zero)
            {
                System.Diagnostics.Debug.WriteLine($"[DRAW] Position du joueur: {Player.Position.X}, {Player.Position.Y}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[DRAW] Player est null dans DrawGameElements!");
        }

        // Draw weapons
        foreach (var weapon in Weapons)
        {
            weapon.Draw(_spriteBatch);
        }

        // Draw enemies
        foreach (var enemy in Enemies)
        {
            enemy.Draw(_spriteBatch);
        }
        
        // Note: On ne dessine plus manuellement l'UI ici, car UIManager s'en charge via les canvas
    }
    
    private void DrawSimpleText(string text, Vector2 position, Color color, Texture2D pixel)
    {
        // For simplicity, we'll just draw a colored rectangle as a text substitute
        int rectWidth = text.Length * 8;
        int rectHeight = 16;
        
        _spriteBatch.Draw(
            pixel,
            new Rectangle((int)position.X, (int)position.Y, rectWidth, rectHeight),
            color);
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Réinitialise complètement le jeu
    /// </summary>
    private void ResetGame()
    {
        _isGameOver = false;
        _score = 0;
        Enemies.Clear();
        Weapons.Clear();
        Collectibles.Clear();
        
        // Réinitialiser les managers dépendants
        _waveManager.Reset();
        _mapManager.Reset();
        
        // Réinitialiser l'état du jeu
        _gameStarted = false;
    }
    
    /// <summary>
    /// Nettoie complètement la carte entre les vagues en supprimant tous les ennemis,
    /// projectiles et autres éléments temporaires.
    /// </summary>
    private void CleanupMap()
    {
        // Supprimer tous les ennemis
        if (Enemies != null)
        {
            Enemies.Clear();
        }
        
        // Nettoyer tous les projectiles des armes
        if (Weapons != null)
        {
            foreach (var weapon in Weapons)
            {
                if (weapon != null)
                {
                    weapon.ClearProjectiles();
                }
            }
        }
        
        // Conserver uniquement les collectibles d'or pour la fin de vague
        // (Supprimer tous les autres collectibles comme l'XP, etc.)
        if (Collectibles != null)
        {
            for (int i = Collectibles.Count - 1; i >= 0; i--)
            {
                if (Collectibles[i].Type != CollectibleType.Gold)
                {
                    Collectibles.RemoveAt(i);
                }
            }
            System.Diagnostics.Debug.WriteLine("[CLEANUP] Collectibles nettoyés, seul l'or de fin de vague reste");
        }
    }
    
    /// <summary>
    /// Repositionne le joueur au centre de l'écran entre les vagues.
    /// </summary>
    private void ResetPlayerPosition()
    {   
        if (Player == null)
        {
            System.Diagnostics.Debug.WriteLine("[ERREUR] Player est null dans ResetPlayerPosition!");
            return;
        }

        // Obtenir les dimensions de l'écran
        var viewport = GraphicsDevice.Viewport;
        
        // Calculer la position centrale
        Vector2 centerPosition = new Vector2(
            viewport.Width / 2,
            viewport.Height / 2);
        
        // Repositionner le joueur
        Player.Position = centerPosition;
    }
    
    #endregion
}
