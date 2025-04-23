using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato;
using Potato.Core;
using Potato.Core.Entities;
using Potato.Core.Logging;
using System;
using System.Collections.Generic;

namespace Potato.Engine
{
    /// <summary>
    /// Gère la génération et le rendu de la carte du jeu.
    /// Cette classe est maintenant un GameBehaviour, ce qui signifie qu'elle doit être attachée à un GameObject
    /// et être initialisée par les scènes.
    /// </summary>
    public class MapManager : GameBehaviour
    {
        // Propriétés de la carte
        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }
        public Rectangle Bounds { get; private set; }
        
        // Instance statique pour la transition
        private static MapManager _instance;
        public static MapManager Instance => _instance;
        
        // Gestion des obstacles et des éléments de décor
        private List<Rectangle> _obstacles;
        private List<Rectangle> _decorations;
        
        // Gestion des zones sûres et dangereuses
        private List<Rectangle> _safeZones;
        private List<Rectangle> _dangerZones;

        // Textures pour le rendu de la carte
        private Texture2D _groundTexture;
        private Texture2D _obstacleTexture;
        private Texture2D _decorationTexture;
        private Texture2D _pixelTexture;  // Pour le rendu basique

        // Configuration de l'apparence
        private Color _backgroundColor = new Color(30, 30, 40);
        private Color _gridColor = new Color(50, 50, 60);
        private Color _obstacleColor = new Color(100, 100, 120);
        private Color _safeZoneColor = new Color(20, 150, 20, 100);  // Vert transparent
        private Color _dangerZoneColor = new Color(150, 20, 20, 100); // Rouge transparent

        // Configuration du système de grille
        private bool _useGrid = true;
        private int _gridCellSize = 64;
        
        // Générateur de nombres aléatoires
        private readonly Random _random;

        public MapManager()
        {
            _obstacles = new List<Rectangle>();
            _decorations = new List<Rectangle>();
            _safeZones = new List<Rectangle>();
            _dangerZones = new List<Rectangle>();
            _random = new Random();
            
            // Conserver une référence à l'instance pour la transition
            _instance = this;
            
            Logger.Info("MapManager instance créée - En attente d'initialisation", LogCategory.Gameplay);
        }
        
        /// <summary>
        /// Appelé lorsque le GameObject est activé
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();
            
            // Initialiser les ressources lors de l'activation
            InitializeResources();
            Logger.Info("MapManager activé", LogCategory.Gameplay);
        }
        
        /// <summary>
        /// Appelé lorsque le GameObject est désactivé
        /// </summary>
        public override void OnDisable()
        {
            base.OnDisable();
            
            // Nettoyer les ressources
            Cleanup();
            Logger.Info("MapManager désactivé", LogCategory.Gameplay);
        }
        
        /// <summary>
        /// Nettoie les ressources du MapManager
        /// </summary>
        private void Cleanup()
        {
            if (_groundTexture != null)
            {
                _groundTexture.Dispose();
                _groundTexture = null;
            }
            
            if (_pixelTexture != null)
            {
                _pixelTexture.Dispose();
                _pixelTexture = null;
            }
            
            _obstacles.Clear();
            _decorations.Clear();
            _safeZones.Clear();
            _dangerZones.Clear();
        }

        /// <summary>
        /// Initialise les ressources du MapManager
        /// </summary>
        private void InitializeResources()
        {
            // Définir les dimensions de la carte basées sur la fenêtre de jeu
            UpdateMapDimensions();

            // Créer une texture de pixel pour le rendu basique
            _pixelTexture = new Texture2D(GameManager.Instance.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            // Créer une texture de sol
            CreateGroundTexture();

            Logger.Info($"MapManager initialisé - Dimensions: {MapWidth}x{MapHeight}", LogCategory.Gameplay);
        }

        /// <summary>
        /// Crée une texture de sol simple pour la carte
        /// </summary>
        private void CreateGroundTexture()
        {
            // Définir la taille de la texture de base
            int textureSize = 128;
            
            // Créer une nouvelle texture
            _groundTexture = new Texture2D(GameManager.Instance.GraphicsDevice, textureSize, textureSize);
            
            // Préparer les données de couleurs pour la texture
            Color[] colorData = new Color[textureSize * textureSize];
            
            // Remplir la texture avec un motif de sol simple
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    // Calculer l'index dans le tableau de couleurs
                    int index = y * textureSize + x;
                    
                    // Créer une légère variation de couleur pour un effet de texture
                    int variation = _random.Next(-10, 10);
                    Color baseColor = new Color(
                        MathHelper.Clamp(40 + variation, 20, 60),
                        MathHelper.Clamp(40 + variation, 20, 60),
                        MathHelper.Clamp(50 + variation, 30, 70)
                    );
                    
                    // Ajouter un peu de bruit pour rendre la texture plus naturelle
                    if (_random.Next(100) < 15)
                    {
                        // Taches plus claires ou plus sombres
                        baseColor = _random.Next(2) == 0 
                            ? new Color(60, 60, 70) 
                            : new Color(30, 30, 40);
                    }
                    
                    colorData[index] = baseColor;
                }
            }
            
            // Appliquer les données de couleurs à la texture
            _groundTexture.SetData(colorData);
            
            Logger.Info("Texture de sol générée avec succès", LogCategory.Gameplay);
        }

        /// <summary>
        /// Met à jour les dimensions de la carte en fonction de la fenêtre de jeu.
        /// </summary>
        public void UpdateMapDimensions()
        {
            if (GameManager.Instance != null)
            {
                MapWidth = GameManager.Instance.GraphicsDevice.Viewport.Width;
                MapHeight = GameManager.Instance.GraphicsDevice.Viewport.Height;
                Bounds = new Rectangle(0, 0, MapWidth, MapHeight);
                
                Logger.Debug($"Dimensions de la carte mises à jour: {MapWidth}x{MapHeight}", LogCategory.Gameplay);
            }
        }

        /// <summary>
        /// Génère une nouvelle carte avec des obstacles et des décorations.
        /// </summary>
        public void GenerateMap(int obstacleCount = 10, int decorationCount = 15)
        {
            if (GameManager.Instance == null)
            {
                Logger.Error("Tentative de génération de carte sans initialisation", LogCategory.Gameplay);
                return;
            }

            // Nettoyer la carte actuelle
            _obstacles.Clear();
            _decorations.Clear();
            _safeZones.Clear();
            _dangerZones.Clear();

            // Générer une zone sûre au centre pour le joueur
            int safeCenterSize = 200;
            Rectangle centerSafeZone = new Rectangle(
                MapWidth / 2 - safeCenterSize / 2,
                MapHeight / 2 - safeCenterSize / 2,
                safeCenterSize,
                safeCenterSize
            );
            _safeZones.Add(centerSafeZone);

            // Générer des obstacles aléatoirement
            for (int i = 0; i < obstacleCount; i++)
            {
                GenerateObstacle();
            }

            // Générer des décorations aléatoirement
            for (int i = 0; i < decorationCount; i++)
            {
                GenerateDecoration();
            }

            // Générer une ou plusieurs zones dangereuses
            GenerateDangerZones();

            Logger.Info($"Nouvelle carte générée avec {_obstacles.Count} obstacles et {_decorations.Count} décorations", LogCategory.Gameplay);
        }

        /// <summary>
        /// Génère un obstacle aléatoire sur la carte.
        /// </summary>
        private void GenerateObstacle()
        {
            // Définir des dimensions aléatoires pour l'obstacle
            int width = _random.Next(30, 100);
            int height = _random.Next(30, 100);
            
            // Définir une position aléatoire qui s'aligne sur la grille si nécessaire
            int x = _useGrid 
                ? _random.Next(0, MapWidth / _gridCellSize) * _gridCellSize 
                : _random.Next(0, MapWidth - width);
                
            int y = _useGrid 
                ? _random.Next(0, MapHeight / _gridCellSize) * _gridCellSize 
                : _random.Next(0, MapHeight - height);

            Rectangle obstacle = new Rectangle(x, y, width, height);

            // Vérifier si l'obstacle chevauche une zone sûre ou un autre obstacle
            bool isOverlapping = false;
            
            foreach (var safeZone in _safeZones)
            {
                if (obstacle.Intersects(safeZone))
                {
                    isOverlapping = true;
                    break;
                }
            }

            foreach (var existingObstacle in _obstacles)
            {
                if (obstacle.Intersects(existingObstacle))
                {
                    isOverlapping = true;
                    break;
                }
            }

            // Ajouter l'obstacle s'il ne chevauche rien
            if (!isOverlapping)
            {
                _obstacles.Add(obstacle);
            }
        }

        /// <summary>
        /// Génère une décoration aléatoire sur la carte.
        /// </summary>
        private void GenerateDecoration()
        {
            // Définir des dimensions aléatoires pour la décoration
            int size = _random.Next(10, 30);
            
            // Définir une position aléatoire
            int x = _random.Next(0, MapWidth - size);
            int y = _random.Next(0, MapHeight - size);

            Rectangle decoration = new Rectangle(x, y, size, size);
            
            // Ajouter la décoration sans vérification de chevauchement (les décorations peuvent se chevaucher)
            _decorations.Add(decoration);
        }

        /// <summary>
        /// Génère des zones dangereuses sur la carte.
        /// </summary>
        private void GenerateDangerZones()
        {
            // Déterminer le nombre de zones dangereuses
            int dangerZoneCount = _random.Next(1, 4);
            
            for (int i = 0; i < dangerZoneCount; i++)
            {
                int width = _random.Next(100, 250);
                int height = _random.Next(100, 250);
                int x = _random.Next(0, MapWidth - width);
                int y = _random.Next(0, MapHeight - height);

                Rectangle dangerZone = new Rectangle(x, y, width, height);
                
                // Vérifier si la zone dangereuse chevauche une zone sûre
                bool isOverlappingSafe = false;
                foreach (var safeZone in _safeZones)
                {
                    if (dangerZone.Intersects(safeZone))
                    {
                        isOverlappingSafe = true;
                        break;
                    }
                }
                
                // Ajouter la zone dangereuse seulement si elle ne chevauche pas une zone sûre
                if (!isOverlappingSafe)
                {
                    _dangerZones.Add(dangerZone);
                }
            }
        }

        /// <summary>
        /// Vérifie si une position est à l'intérieur d'un obstacle.
        /// </summary>
        public bool IsPositionInObstacle(Vector2 position)
        {
            foreach (var obstacle in _obstacles)
            {
                if (obstacle.Contains(position))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Vérifie si une position est à l'intérieur d'une zone dangereuse.
        /// </summary>
        public bool IsPositionInDangerZone(Vector2 position)
        {
            foreach (var dangerZone in _dangerZones)
            {
                if (dangerZone.Contains(position))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Vérifie si une entité peut se déplacer vers une position donnée.
        /// </summary>
        public bool CanMoveTo(Entity entity, Vector2 newPosition)
        {
            /*
            // Créer un rectangle pour représenter l'entité à la nouvelle position
            Rectangle entityRect = new Rectangle(
                (int)newPosition.X - entity.Width / 2,
                (int)newPosition.Y - entity.Height / 2,
                entity.Width,
                entity.Height
            );
            
            // Vérifier si l'entité entre en collision avec un obstacle
            foreach (var obstacle in _obstacles)
            {
                if (entityRect.Intersects(obstacle))
                {
                    return false;
                }
            }
            
            // Vérifier si l'entité sort des limites de la carte
            if (!Bounds.Contains(entityRect))
            {
                return false;
            }
            */
            return true;
        }

        /// <summary>
        /// Met à jour la carte et les effets liés aux zones.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            Player player = Player.Local;

            if (player == null)
                return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Vérifier si le joueur est dans une zone dangereuse et appliquer des effets
            if (IsPositionInDangerZone(player.Position))
            {
                // Infliger des dégâts au joueur s'il reste dans une zone dangereuse
                player.TakeDamage(1 * deltaTime); // Dégâts graduels
                
                // Ajouter des effets visuels ou sonores si nécessaire
            }
        }

        /// <summary>
        /// Dessine la carte et tous ses éléments.
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            if (GameManager.Instance == null)
                return;

            // Dessiner le sol texturé
            if (_groundTexture != null)
            {
                // Dessiner le sol texturé en répétant la texture pour couvrir toute la carte
                for (int y = 0; y < MapHeight; y += _groundTexture.Height)
                {
                    for (int x = 0; x < MapWidth; x += _groundTexture.Width)
                    {
                        spriteBatch.Draw(
                            _groundTexture,
                            new Rectangle(x, y, _groundTexture.Width, _groundTexture.Height),
                            Color.White);
                    }
                }
            }
            else
            {
                // Fallback: dessiner l'arrière-plan uni si la texture n'est pas disponible
                spriteBatch.Draw(_pixelTexture, Bounds, _backgroundColor);
            }
            
            // Dessiner la grille si activée
            if (_useGrid)
            {
                DrawGrid(spriteBatch);
            }
            
            // Dessiner les zones dangereuses
            foreach (var dangerZone in _dangerZones)
            {
                spriteBatch.Draw(_pixelTexture, dangerZone, _dangerZoneColor);
            }
            
            // Dessiner les zones sûres
            foreach (var safeZone in _safeZones)
            {
                spriteBatch.Draw(_pixelTexture, safeZone, _safeZoneColor);
            }
            
            // Dessiner les décorations
            foreach (var decoration in _decorations)
            {
                spriteBatch.Draw(_pixelTexture, decoration, Color.DarkGray);
            }
            
            // Dessiner les obstacles
            foreach (var obstacle in _obstacles)
            {
                spriteBatch.Draw(_pixelTexture, obstacle, _obstacleColor);
            }
        }

        /// <summary>
        /// Dessine une grille sur la carte.
        /// </summary>
        private void DrawGrid(SpriteBatch spriteBatch)
        {
            // Dessiner les lignes verticales
            for (int x = 0; x < MapWidth; x += _gridCellSize)
            {
                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle(x, 0, 1, MapHeight),
                    _gridColor);
            }
            
            // Dessiner les lignes horizontales
            for (int y = 0; y < MapHeight; y += _gridCellSize)
            {
                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle(0, y, MapWidth, 1),
                    _gridColor);
            }
        }

        /// <summary>
        /// Trouve une position de spawn sûre pour une entité.
        /// </summary>
        public Vector2 FindSafeSpawnPosition(int entityWidth, int entityHeight)
        {
            // Essayer plusieurs positions jusqu'à en trouver une valide
            for (int attempts = 0; attempts < 100; attempts++)
            {
                int x = _random.Next(entityWidth/2, MapWidth - entityWidth/2);
                int y = _random.Next(entityHeight/2, MapHeight - entityHeight/2);
                Vector2 position = new Vector2(x, y);
                
                if (!IsPositionInObstacle(position) && !IsPositionInDangerZone(position))
                {
                    return position;
                }
            }
            
            // Si aucune position sûre n'est trouvée, retourner le centre de la carte
            return new Vector2(MapWidth / 2, MapHeight / 2);
        }

        /// <summary>
        /// Réinitialise complètement la carte.
        /// </summary>
        public void Reset()
        {
            _obstacles.Clear();
            _decorations.Clear();
            _safeZones.Clear();
            _dangerZones.Clear();
            
            // Générer une nouvelle carte
            GenerateMap();
            
            Logger.Info("Carte réinitialisée", LogCategory.Gameplay);
        }
    }
}