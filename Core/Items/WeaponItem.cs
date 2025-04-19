using Potato.Core.Entities;
using Potato.Core.Weapons;

namespace Potato.Core.Items;
    

public enum WeaponType
{
    Melee,
    Ranged
}

public class WeaponItem : ShopItem
{
    private WeaponType _weaponType;
    private float _powerMultiplier;
    
    public WeaponItem(string name, string description, int cost, WeaponType weaponType, float powerMultiplier)
        : base(name, description, cost)
    {
        _weaponType = weaponType;
        _powerMultiplier = powerMultiplier;
    }
    
    public override bool Purchase(Player player)
    {
        Weapon weapon = null;
        
        // Créer le type d'arme approprié
        switch (_weaponType)
        {
            case WeaponType.Melee:
                weapon = new MeleeWeapon(Name);
                break;
            case WeaponType.Ranged:
                weapon = new RangedWeapon(Name);
                break;
        }
        
        if (weapon != null)
        {
            // Utiliser la méthode Upgrade pour augmenter les dégâts au lieu de modifier directement
            for (int i = 0; i < 3; i++)  // Augmenter plusieurs fois pour simuler l'effet multiplicateur
            {
                weapon.Upgrade();
            }
            
            // Initialiser l'arme
            weapon.Initialize();
            
            // Ajouter l'arme au joueur
            player.AddWeapon(weapon);
            
            return true;
        }
        
        return false;
    }
}
