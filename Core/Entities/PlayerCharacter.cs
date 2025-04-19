using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Potato.Core.Entities
{
    public class PlayerCharacter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Color Color { get; set; }
        public Dictionary<StatType, float> StatModifiers { get; set; }
        public bool IsLocked { get; set; }
        
        public PlayerCharacter(string name, string description, Color color)
        {
            Name = name;
            Description = description;
            Color = color;
            StatModifiers = new Dictionary<StatType, float>();
            IsLocked = false;
        }
        
        public Player CreatePlayer()
        {
            Player player = new Player(this);
            return player;
        }
    }
    
    public enum StatType
    {
        MaxHealth,
        Speed,
        Damage,
        AttackSpeed,
        Range,
        CriticalChance,
        CriticalDamage,
        Harvesting,
        Engineering,
        Luck
    }
}