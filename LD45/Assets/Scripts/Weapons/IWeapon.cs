using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IWeapon
{
    /*
     * Implement to give the weapon its primary fire behavior (Left click/right trigger)
     * returns if the weapon was able to fire or not (cool downs, ammo limits, etc may cause this to be false)
     * firer - The weapon weilder who caused the projectile to be fired
     * tryAuto - Wether the weilder is holding down the fire command, false if the input was just triggered
     */
    bool PrimaryFire(WeaponWielder firer, bool tryAuto);

    /*
     * Implement to give the weapon its reload behavior
     * returns if the weapon was able to reload or not
     * firer - The weapon weilder who caused the projectile to be fired
     */
    bool Reload(WeaponWielder firer);

    /*
     *  Name for the weapon to show on HUD/whatever
     */
    string GetDisplayName();

    /*
     *  Weapon name for programming purposes (should be stable and not change unless 100% necessary)
     */
    string GetInternalName();

    /*
     *  Remaining ammunition for current weapon, percent range 0.0 - 1.0
     */
    float GetCurrentAmmoRatio();

    /*
     *  Remaining ammunition in # of shots
     */
    float GetCurrentAmmo();

    /*
     *  Maximum ammunition this weapon can hold in # of shots
     */
    float GetMaxAmmo();

    /*
     * Reload Progress
     */
    float GetReloadProgress();

     *  Speed at which projectiles this weapon fires will fly
     *  (for AI to accurately lead targets)
     */
    float GetProjectileSpeed();
}