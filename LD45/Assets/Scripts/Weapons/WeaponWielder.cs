using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct WeaponChangeEvent
{
    // Weapon that is being changed to
    public IWeapon nextWeapon;

    // Weapon that is being changed from
    // WARNING - may be null when a weapon is first equiped
    public IWeapon prevWeapon;
}

[System.Serializable]
public class OnWeaponChangeEvent : UnityEvent<WeaponChangeEvent>
{
}

public class WeaponWielder : MonoBehaviour
{
    public GameObject startingWeapon;

    private IWeapon currWeapon;

    public OnWeaponChangeEvent onWeaponChangeEvent;

    public IWeapon CurrentWeapon
    {
        get { return currWeapon; }
        set
        {
            WeaponChangeEvent weaponEvent;
            weaponEvent.prevWeapon = currWeapon;
            weaponEvent.nextWeapon = value;
            if (onWeaponChangeEvent != null)
            {
                onWeaponChangeEvent.Invoke(weaponEvent);
            }

            currWeapon = value;
        }
    }

    public void FirePrimary(bool fireAuto)
    {
        CurrentWeapon.PrimaryFire(this, fireAuto);
    }

    private void Start()
    {
        CurrentWeapon = startingWeapon.GetComponent<IWeapon>();
    }
}
