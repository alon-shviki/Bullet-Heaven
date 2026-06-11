using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Client.Game.Entities;

public interface ISecondaryWeapon
{
    int  Update(double dt, Player player, EntityPool<Enemy> enemies, WeaponStats stats);
    void Reset();
}
