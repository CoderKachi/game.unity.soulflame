using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static UnityEngine.ParticleSystem;
using static Framework;

[CreateAssetMenu(fileName = "Attack Config", menuName = "Attacks/Attack Config", order = 1)]
public class AttackScriptable : ScriptableObject
{
    [Header("Basic Stats")]   
    public float attackSpeed = 0.25f;
    public int attackAmmoComsumption = 1;
    protected float lastAttacktime = Mathf.NegativeInfinity;
    public int damage = 1;
    [Header("Attack Cone and Count")]
    public float spreadAngleDeg;
    public bool randomSpread = false;
    public int attacksPerBurst = 1;
    public int bursts = 1;
    public float burstDelay = 0.2f;
    
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public Weapon weapon;

    public void ResetTime(float time)
    {
        lastAttacktime = time;
    }

    public virtual void AttackStart(GameObject parent)
    {

    }

    public virtual void AttackUpdate(GameObject parent)
    {

    }

    public virtual void AttackEnd(GameObject parent)
    {

    }


    public virtual void WeaponUpdate()
    {
        
    }

    protected virtual void Attack(GameObject parent, int i)
    {

    }

    protected virtual void BurstStarted()
    {
        EventService EventService = Game.GetService<EventService>();
    }

    protected virtual async UniTask<bool> TryAttack(GameObject parent)
    {
        bool skipDelay = false;
        if (Time.time - lastAttacktime < attackSpeed) return false;
        while (isAttacking)
        {
            if (weapon.magCurrent <= 0 || weapon.GetState() != WeaponState.Idle)
            {
                await UniTask.Yield();
                continue;
            }

            weapon.SetState(WeaponState.Attacking);

            for (int j = 0; j < bursts; j++)
            {
                BurstStarted();
                for (int i = 0; i < attacksPerBurst; i++)
                {
                    Attack(parent, i);
                }

                weapon.UseAmmo(attackAmmoComsumption);
                if (weapon.GetMag() <= 0)
                {
                    skipDelay = true;
                    break;
                }
                await UniTask.Delay((int)(burstDelay * 1000f));
            }
            if (weapon.reloadQueued) skipDelay = true;
            lastAttacktime = Time.time;
            if (!skipDelay)
            {
                await UniTask.Delay((int)(attackSpeed * 1000f));
            }

            weapon.SetState(WeaponState.Idle);
            if (weapon.reloadQueued || weapon.GetMag() <= 0)
            {
                _ = weapon.TryReload();
            }
            skipDelay = false;
            await UniTask.Yield();
        }

        weapon.SetState(WeaponState.Idle);
        return true;
    }
}
