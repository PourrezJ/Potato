using Potato.Core.Entities;
using System;

namespace Potato.Core.Items;

public class HealItem : ShopItem
{
    private int _healAmount;
    
    public HealItem(string name, string description, int cost, int healAmount)
        : base(name, description, cost)
    {
        _healAmount = healAmount;
    }
    
    public override bool Purchase(Player player)
    {
        // Soigner le joueur sans dépasser sa santé maximale
        float newHealth = player.Stats.Health + _healAmount;
        player.Stats.Health = Math.Min(newHealth, player.Stats.MaxHealth);
        
        return true;
    }
}