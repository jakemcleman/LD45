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

    public UnityEvent onWeaponReloadEvent;

    private string weapon_path = "event:/SFX/Weapons/";

    public IWeapon CurrentWeapon
    {
        get { return currWeapon; }
        set
        {
            // Don't send an event unless the thing is actually changing
            if (currWeapon != value)
            {
                WeaponChangeEvent weaponEvent;
                weaponEvent.prevWeapon = currWeapon;
                weaponEvent.nextWeapon = value;
                if (onWeaponChangeEvent != null)
                {
                    onWeaponChangeEvent.Invoke(weaponEvent);
                }

                Debug.Log("Weapon changed to " + value.GetDisplayName());
            }

            currWeapon = value;
        }
    }

    public void FirePrimary(bool fireAuto)
    {
        if (CurrentWeapon.PrimaryFire(this, fireAuto))
        {
            // Cannot set local parameters without an event instance, so we make one here instead of using PlayOneShot
            // This is probably not the most efficient way to do this, but welcome to game jams
            FMOD.Studio.EventInstance fire = FMODUnity.RuntimeManager.CreateInstance(weapon_path + CurrentWeapon.GetInternalName() + "_Fire");
            fire.setParameterByName(CurrentWeapon.GetInternalName() + "_Ammo", CurrentWeapon.GetCurrentAmmoRatio());
            fire.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
            fire.start();
            fire.release();
        }
    }

    public void Reload()
    {
        FMODUnity.RuntimeManager.PlayOneShot(weapon_path + CurrentWeapon.GetInternalName() + "_Reload",
                                             this.transform.position);

        CurrentWeapon.Reload(this);
        onWeaponReloadEvent.Invoke();
    }

    private void Start()
    {
        CurrentWeapon = startingWeapon.GetComponent<IWeapon>();
    }
}
