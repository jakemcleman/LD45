using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IWeapon
{
    /*
     * Implement to give the weapon primary fire behavior (Left click/right trigger)
     * returns if the weapon was able to fire or not (cool downs, ammo limits, etc may cause this to be false)
     * firer - The weapon weilder who caused the projectile to be fired
     * tryAuto - Wether the weilder is holding down the fire command, false if the input was just triggered
     */
    bool PrimaryFire(WeaponWielder firerer, bool tryAuto);

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
}