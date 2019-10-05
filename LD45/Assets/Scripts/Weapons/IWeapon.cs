using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IWeapon
{
    /*
     * Implement to give the weapon primary fire behavior (Left click/right trigger)
     * returns the projectiles/effects it created in case the firer cares about them
     * firer - The weapon weilder who caused the projectile to be fired
     * tryAuto - Wether the weilder is holding down the fire command, false if the input was just triggered
     */
    GameObject[] PrimaryFire(WeaponWielder firerer, bool tryAuto);

    /*
     *  Name for the weapon to show on HUD/whatever
     */
    string GetDisplayName();
}