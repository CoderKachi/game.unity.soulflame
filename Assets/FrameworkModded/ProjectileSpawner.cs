using UnityEngine;
using static Framework;


public class ProjectileSpawner : MonoBehaviour
{
    public static ProjectileSpawner Instance;
    private PoolService PoolService;
    [SerializeField]
    Projectile bullet;
        
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        PoolService = Game.GetService<PoolService>();
    }

    private void Start()
    {
        
        PoolService.CreatePool<Projectile>(bullet, ProjectileType.Bullet.ToString(), 32);
    }

    public Projectile SpawnProjectile(GameObject parent, ProjectileType projectile, Vector3 firePoint)
    {
        Projectile pro;
        switch (projectile) {
            case ProjectileType.Bullet:
                pro = PoolService.Get<Projectile>(projectile.ToString());
                firePoint = CheckWalls(parent, firePoint, pro);
                pro.transform.position = firePoint;
                pro.transform.rotation = parent.transform.rotation;
                return pro;
        }
        return null; 
    }

    private Vector3 CheckWalls(GameObject parent, Vector3 firepoint, Projectile projectile)
    {
        LayerMask wallMask = LayerMask.GetMask("Wall");
        Vector3 parentPos = new(parent.transform.position.x, 1, parent.transform.position.z);
        RaycastHit hit;
        if (projectile.sphereCast)
        {
            if (Physics.SphereCast(parentPos, projectile.GetBounds(), firepoint - parentPos, out hit, Vector3.Distance(parentPos, firepoint), wallMask))
            {
                firepoint = (hit.point - parentPos) * 0.9f + parentPos;

            }
        }
        else
        {
            if (Physics.Raycast(parentPos, firepoint - parentPos, out hit, Vector3.Distance(parentPos, firepoint), wallMask))
            {
                firepoint = (hit.point - parentPos) * 0.9f + parentPos;

            }
        }

        return firepoint;
    }
}
public enum ProjectileType
{
    Bullet
}
