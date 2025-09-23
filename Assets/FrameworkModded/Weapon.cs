using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Weapon Base", menuName = "Weapons/Weapon Base", order = 0)]
public class Weapon : ScriptableObject
{
    [Header("Attack Scriptable Objects")]
    public AttackScriptable primaryFire;
    public AttackScriptable secondaryFire;
    [Header("Ammo and Reloading")]
    public int ammoMax = 120;
    public int ammoStored = 60;
    public float ammoRegenRate = 2;
    public float reloadTime = 0.4f;
    public int magCapcity = 30;
    public int magCurrent = 30;
    public bool unlimitedAmmo = false;
    [Header("Model and Spawnpoints")]
    public Vector2 firePointInitial;
    public Sprite model;
    public Sprite worldModel;


    private WeaponState weaponState = WeaponState.Idle;
    private float ammoRegenTick = Mathf.NegativeInfinity;
    
    [HideInInspector] public bool reloadQueued;
    [HideInInspector] public Transform firepoint;
    [HideInInspector] public Vector2 lookVector;
    

    public virtual async UniTask<bool> TryReload()
    {
        if (weaponState != WeaponState.Idle)
        {
            reloadQueued = true;
            return false;
        }
        if (magCurrent == magCapcity || ammoStored < 1) return false;
        reloadQueued = false;

        weaponState = WeaponState.Reloading;
        await UniTask.Delay((int)(reloadTime * 1000f));
        weaponState = WeaponState.Idle;

        if (unlimitedAmmo)
        {
            magCurrent = magCapcity;
        }
        else
        {
            int ammoNeeded = Mathf.Min(ammoStored, magCapcity - magCurrent);
            magCurrent += ammoNeeded;
            ammoStored -= ammoNeeded;
        }
        return true;
    }
    public int GetMag()
    {
        return magCurrent;
    }
    public void SetMag(int value)
    {
        magCurrent = value;
    }
    public void UseAmmo(int value)
    {
        magCurrent -= value;
    }


    public void SetState(WeaponState setWeaponState)
    {
        weaponState = setWeaponState;
    }

    public WeaponState GetState()
    {
        return weaponState;
    }

    public void RegenAmmo()
    {
        if (Time.time - ammoRegenTick < 1f / ammoRegenRate) return;
        ammoRegenTick = Time.time;
        ammoStored = Mathf.Min(ammoMax, ammoStored + 1);
    }
    
}