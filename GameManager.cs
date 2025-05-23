﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Potato.Engine;
using Potato.Core.UI;
using Potato.Core.Logging;
using Potato.Core;
using Potato.Core.Entities;
using Potato.Core.Enemies;
using Potato.Core.Weapons;
using Potato.Core.Scenes;
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
        Logger.Configure(LogLevel.Debug, true, false, false);
        Logger.Info("Game initialized");
        
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
        Logger.Info("BehaviourManager initialisé", LogCategory.Core);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        try
        {             
            // Initialiser le BehaviourManager
            //BehaviourManager.DiscoverBehaviours();
            //Logger.Info("Behaviors discovery completed");

            // Initialiser le UIManager
            UIManager.Initialize();

            // // Initialize log viewer
            // _logViewer = new LogViewer(this);
            // Logger.Info("LogViewer initialized");

            // Initialiser le GameObjectManager
            GameObjectManager.Initialize();
            Logger.Info("GameObjectManager initialisé", LogCategory.Core);
            
            // Initialiser le SceneManager
            SceneManager.Initialize();
            Logger.Info("SceneManager initialisé", LogCategory.Core);

            // Initialiser les gestionnaires de jeu
            // _waveManager = WaveManager.Instance;
            // _mapManager = MapManager.Instance;
            // _waveManager.OnWaveCompleted += OnWaveCompletedHandler;

            // Créer et enregistrer les scènes principales du jeu
            CreateAndRegisterScenes();
                 
            MainMenuCanvas.Current.IsVisible = true;

            Logger.Info("All screens initialized");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error during content loading: {ex.Message}", LogCategory.Core);
        }       
    }
    
    /// <summary>
    /// Crée et enregistre les scènes principales du jeu
    /// </summary>
    private void CreateAndRegisterScenes()
    {
        // Créer les scènes principales avec les classes spécifiques
        var mainMenuScene = new MainMenuScene();    
        var characterSelectionScene = new CharacterSelectionScene();
        /*
        var gameplayScene = new GameplayScene();
        var shopScene = new ShopScene();
        */
        // Enregistrer les scènes dans le SceneManager
        SceneManager.RegisterScene(mainMenuScene);    
        SceneManager.RegisterScene(characterSelectionScene);
        /*
        SceneManager.RegisterScene(gameplayScene);
        SceneManager.RegisterScene(shopScene);
        */
        // Charger initialement la scène du menu principal
        SceneManager.LoadScene("MainMenu", 0f); // Transition immédiate
        
        Logger.Info("Scènes créées et enregistrées avec succès", LogCategory.Core);
    }

    protected override void Update(GameTime gameTime)
    {                
        // Mettre à jour tous les GameObjects via le GameObjectManager
        GameObjectManager.Update(gameTime);

        // Mettre à jour tous les GameBehaviours via le BehaviourManager
        BehaviourManager.Update(gameTime);

        UIManager.Update(gameTime);
     
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
        
        // End sprite batch
        _spriteBatch.End();
        
        // Dessiner l'UI avec UIManager - AJOUT IMPORTANT pour afficher le menu
        UIManager.Draw(_spriteBatch);
                     
        // Draw LogViewer on top of everything if it's visible
        _logViewer?.Draw(_spriteBatch);

        base.Draw(gameTime);
    }
    
    protected override void OnExiting(object sender, Microsoft.Xna.Framework.ExitingEventArgs args)
    {
        // Clean up resources before exiting
        Logger.Info("Game exiting", LogCategory.Gameplay);
        
        base.OnExiting(sender, args);
    }

    #endregion

    #region Game State Management
    
    public void SetGameState(GameState gameState)
    {
        GameState previousState = _currentGameState;
        _currentGameState = gameState;
        
        Logger.Info($"Game state changed from {previousState} to: {_currentGameState}", LogCategory.Gameplay);  
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
        // Nous n'avons plus besoin de générer la carte ici
        // car cela sera géré par le MapManager lorsqu'il sera activé par la GameplayScene
        // La méthode GenerateMap sera appelée plus bas
        
        // Initialize player and weapon
        _selectedPlayer.Initialize();
        Player = _selectedPlayer;
        
        // S'assurer que le joueur est bien positionné au centre de l'écran
        Player.Position = new Vector2(
            GraphicsDevice.Viewport.Width / 2,
            GraphicsDevice.Viewport.Height / 2);
        Logger.Info($"Joueur positionné au centre: {Player.Position.X}, {Player.Position.Y}", LogCategory.Gameplay);

     
        _selectedWeapon.Initialize();
        Player.AddWeapon(_selectedWeapon);
        
        // Nous n'avons plus besoin d'activer explicitement le WaveManager ici
        // car cela est maintenant géré par la GameplayScene
        
        // Générer une nouvelle carte pour la partie avant de démarrer la vague
        // Cela sera fait quand le MapManager sera activé, mais on prépare déjà la carte
        _mapManager.GenerateMap();
        
        // Start wave 1
        StartWave(1);
        
        // Flag game as started
        _gameStarted = true;
        
        // Set state to playing - cela va charger la scène GameplayScene
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
        Logger.Info($"Wave {waveNumber} completed - Opening shop");
        
        try
        {
            // Nettoyer la carte
            CleanupMap();
            
            // Repositionner le joueur
            ResetPlayerPosition();
   
            // Change game state
            SetGameState(GameState.Shopping);

            
            Logger.Info("Shop opened successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error opening shop: {ex.Message}");
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
        // _waveManager.Reset();
        // _waveManager.Deactivate(); // Désactiver le WaveManager pour qu'il n'essaie pas de générer des ennemis
        // _mapManager.Reset();
        
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
