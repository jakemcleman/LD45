using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponWielder))]
public class PlayerWeaponController : MonoBehaviour
{
    public GameObject[] weapons;

    public bool[] weaponUnlocked;

    private int curWeaponIndex;
    private WeaponWielder wielder;
    private ProgressBar reloadUI;

    private Transform headTransform;

    private float maxWeaponAimAdjustRange = 200;

    private void Start()
    {
        wielder = GetComponent<WeaponWielder>();
        reloadUI = GameObject.Find("ReloadIndicator").GetComponent<ProgressBar>();

        curWeaponIndex = 0;
        ChangeToWeapon(curWeaponIndex);

        wielder.onWeaponReloadEvent.AddListener(OnWeaponReload);

        headTransform = GetComponentInChildren<Camera>().transform;
    }

    private void Update()
    {
        if (MenuController.Paused) return;

        PointWeaponAtCrosshair();

        if (Input.GetButtonDown("Fire1"))
        {
            wielder.FirePrimary(false);
        }
        else if (Input.GetButton("Fire1"))
        {
            wielder.FirePrimary(true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            wielder.Reload();
        }

        // Test weapon switching code - if this is a real thing we want do this better with a real input button
        if (Input.GetKeyDown(KeyCode.E))
        {
            ChangeToWeapon(curWeaponIndex + 1);
        }
    }

    private void PointWeaponAtCrosshair()
    {
        RaycastHit hit;
        Transform curWeapon = weapons[curWeaponIndex].transform;
        Vector3 toHitPoint = headTransform.forward;

        if(Physics.Raycast(headTransform.position, headTransform.forward, out hit, maxWeaponAimAdjustRange, ~(1 << 9)))
        {
            toHitPoint = (hit.point - curWeapon.position).normalized;
            
        }
        
        curWeapon.forward = toHitPoint;
    }

    public void UnlockWeapon(int index)
    {
        if(!weaponUnlocked[index])
        {
            weaponUnlocked[index] = true;
            if(index > 0) weaponUnlocked[0] = false;
            ChangeToWeapon(index);
        }
    }

    private void ChangeToWeapon(int index)
    {
        foreach(MeshRenderer mr in weapons[curWeaponIndex].GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }

        curWeaponIndex = index;
        if (curWeaponIndex >= weapons.Length) curWeaponIndex = 0;
        if (curWeaponIndex < 0) curWeaponIndex = weapons.Length - 1;

        while(!weaponUnlocked[curWeaponIndex])
        {
            ChangeToWeapon(curWeaponIndex + 1);
        }
        
        foreach(MeshRenderer mr in weapons[curWeaponIndex].GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = true;
        }

        IWeapon newWep = weapons[curWeaponIndex].GetComponent<IWeapon>();
        if(newWep == null) Debug.LogError("New weapon is not a weapon, does not implement IWeapon");
        wielder.CurrentWeapon = newWep;
    }

    private void OnWeaponReload()
    {
        reloadUI.Fill = wielder.CurrentWeapon.GetReloadProgress();
        StartCoroutine("ReloadIndicator");
    }

    IEnumerator ReloadIndicator()
    {
        while (wielder.CurrentWeapon.GetReloadProgress() < 1)
        {
            reloadUI.Fill = wielder.CurrentWeapon.GetReloadProgress();
            yield return null;
        }

        yield return new WaitForSeconds(.2f);
        reloadUI.Fill = 0;
    }
}
