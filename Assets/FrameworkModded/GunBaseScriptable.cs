using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[CreateAssetMenu(fileName = "Gun Attack Base", menuName = "Attacks/Gun Base", order = 0)]
public class GunBaseScriptable : AttackScriptable
{
    public ProjectileType projectile;

    public int penetrations = 0;
    public int bounces = 0;
    
    [Range(0f, 500f)]
    public float projectileSpeed = 100f;
    public float projectileLifespan = 5f;

    public override void AttackStart(GameObject parent)
    {
        isAttacking = true;
        _ = TryAttack(parent);
    }
    
    public override void AttackUpdate(GameObject parent)
    {

    }

    protected override void Attack(GameObject parent, int i)
    {
        lastAttacktime = Time.time;
        Vector3 dir = new(weapon.lookVector.x, 0, weapon.lookVector.y);
        if (randomSpread)
        {
            var angle = Random.Range(-spreadAngleDeg / 2, spreadAngleDeg / 2);
            dir = Quaternion.AngleAxis(angle, Vector3.up) * dir;
        }
        else
        {
            var angle = (spreadAngleDeg / 2 - (spreadAngleDeg / attacksPerBurst) * i) - spreadAngleDeg / (attacksPerBurst * 2);
            dir = Quaternion.AngleAxis(angle, Vector3.up) * dir;
        }

        Projectile pro = ProjectileSpawner.Instance.SpawnProjectile(parent, projectile, weapon.firepoint.position);
        pro.ProjectileStats(dir, projectileSpeed, damage, bounces, penetrations, projectileLifespan, parent.layer);
        pro.gameObject.SetActive(true);
    }

    public override void AttackEnd(GameObject parent)
    {
        isAttacking = false;
    }

    public static Vector2 rotate(Vector2 v, float delta)
    {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }
}
