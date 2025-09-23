using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Framework;
using static TrailComponent;

public class Projectile : MonoBehaviour
{
    [HideInInspector]
    ProjectileType projectileType;
    BounceType bounceType = BounceType.BounceOffTargets;
    Vector3 direction;
    public bool isHoming = false;
    
    float speed = 2f;
    float lifeSpan = 10f;
    float homingRadius = 5f;
    float detectionRadiusBounce = 10f;
    float distPerIteration;
    float turnSpeedInDegrees = 360;
    float hitCooldown = 0.5f;
    float lastHitTime;
    int bounces;
    int penetrations;
    int damage;

    List<GameObject> targetsHit = new List<GameObject>();
    HealthComponent lastTargetHit;
    Transform homingTarget;
    TrailComponent line;
    Collider col;

    public bool sphereCast = false;
    bool active = true;

    [SerializeField] GameObject sprite;
    [SerializeField] GameObject model;

    float angle;

    private PoolService PoolService;
    private Transform attackPlane;



    public void ProjectileStats(Vector3 direction, float speed, int damage)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        angle = Mathf.Atan2(direction.z, direction.x) * (180 / Mathf.PI);
        sprite.transform.eulerAngles = new Vector3(0, transform.rotation.y, angle);

    }
    public void ProjectileStats(Vector3 direction, float speed, int damage, float lifeSpan, LayerMask targetMask)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        gameObject.layer = targetMask;
        this.lifeSpan = lifeSpan;
        angle = Mathf.Atan2(direction.z, direction.x) * (180 / Mathf.PI);
        sprite.transform.eulerAngles = new Vector3(0, transform.rotation.y, angle);
    }

    public void ProjectileStats(Vector3 direction, float speed, int damage, int bounces, int penetrations, float lifeSpan, LayerMask targetMask)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        this.bounces = bounces;
        this.penetrations = penetrations;
        gameObject.layer = targetMask;
        this.lifeSpan = lifeSpan;
        angle = Mathf.Atan2(direction.z, direction.x) * (180 / Mathf.PI);
        sprite.transform.eulerAngles = new Vector3(0, transform.rotation.y, angle);

    }

    public float GetBounds()
    {
        return col.bounds.extents.x;
    }

    private void Awake()
    {
        col = GetComponent<Collider>();
        line = GetComponent<TrailComponent>();
        transform.SetParent(null);
        PoolService = Game.GetService<PoolService>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastHitTime = Time.time;
        line.ClearLines();
        line.AppendLineVertex(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastHitTime > hitCooldown)
        {
            targetsHit.Clear();
            homingTarget = null;
        }
        lifeSpan -= Time.deltaTime;

        if (isHoming)
        {
            homingTarget = FindHomingTarget();
        }
            
        if (homingTarget)
        {
            TurnTowardsTarget();
        }
        Vector3 velocity = speed * Time.deltaTime * direction.normalized;
        if (active && velocity.sqrMagnitude>0) 
        {
            line.AppendLineVertex(transform.position);
            
            ShootProjectile(velocity);
            if (lifeSpan <= 0)
            {
                DisableProjectile();
            }
        }
        


    }

    void ShootProjectile(Vector3 velocity)
    {
        angle = Mathf.Atan2(direction.x, direction.z) * (180 / Mathf.PI);
        sprite.transform.eulerAngles = new Vector3(0, angle, 0);
        
        distPerIteration = velocity.magnitude;
        if (speed >= 500)
        {
            distPerIteration = Mathf.Infinity;
        }
        RaycastHit hit;
        if (sphereCast)
        {
            Physics.SphereCast(transform.position, col.bounds.extents.x, direction, out hit, distPerIteration, Physics2D.GetLayerCollisionMask(gameObject.layer));
        }
        else
        {
            Physics.Raycast(transform.position, direction, out hit, distPerIteration, Physics2D.GetLayerCollisionMask(gameObject.layer));
        }
        if (hit.collider != null)
        {
            
            if (hit.collider.GetComponent<HealthComponent>())
            {
                HealthComponent health = hit.collider.GetComponent<HealthComponent>();
                bool hitBefore = false;
                foreach (GameObject enemy in targetsHit)
                {
                    if (enemy == hit.collider.gameObject)
                    {
                        hitBefore = true;
                    }
                }
                if (!hitBefore)
                {
                    lastTargetHit = health;
                    health.TakeDamage(damage);
                    homingTarget = null;
                    targetsHit.Add(hit.collider.gameObject);
                    if (bounceType == BounceType.BounceOffTargets)
                    {
                        if (bounces > 0)
                        {
                            
                            //transform.position = AlignWithAttackPlane(hit.collider.transform.position);
                            bounces--;
                            transform.position = hit.point;
                            
                            //transform.position = AlignWithAttackPlane(hit.transform.position);
                            BounceOffTarget();
                            Projectile pro = ProjectileSpawner.Instance.SpawnProjectile(gameObject, projectileType, transform.position);
                            pro.ProjectileStats(direction, speed, damage, bounces, penetrations, lifeSpan, gameObject.layer);
                            DisableProjectile();
                            
                            
                            pro.gameObject.SetActive(true);
                            
                        }
                        else
                        {
                            transform.position = hit.point;
                            DisableProjectile();
                        }
                    }
                    else
                    {
                        if (penetrations-- <= 0)
                        {
                            transform.position = hit.point;
                            DisableProjectile();
                        }
                    }
                }

                
                transform.position += velocity;
            }
            else
            {
                HandleCollision(hit, velocity);
            }
                
        }
        else
        {
            transform.position += velocity;
        }
    }

    private void DisableProjectile()
    {
        active = false;
        gameObject.layer = 0;
        line.AppendLineVertex(transform.position);
        model.SetActive(false);
        Invoke("Remove", 3f);
    }


    void HandleCollision(RaycastHit hit, Vector3 velocity)
    {
        Vector3 snapToSurface = velocity.normalized * (hit.distance);
        velocity = velocity - snapToSurface;
        transform.position += snapToSurface;
        if (bounces > 0)
        {
            bounces--;
            direction = Vector3.Reflect(direction, hit.normal);
            if (bounceType == BounceType.BounceTowardsTargets)
            {
                BounceToNearbyTarget();
            }
            velocity = velocity.magnitude * direction.normalized;
            line.AppendLineVertex(transform.position);
            ShootProjectile(velocity);
        }
        else
        {
            gameObject.layer = 0;
            line.AppendLineVertex(transform.position);
            speed = 0;
            active = false;
            Invoke("Remove", 3f);
        }
    }
    private void 
        BounceOffTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadiusBounce, Physics2D.GetLayerCollisionMask(gameObject.layer));
        Transform nearestTarget = null;
        float targetDist = Mathf.Infinity;

        foreach (Collider col in hitColliders)
        {
            HealthComponent health = col.GetComponent<HealthComponent>();
            if (Vector3.Distance(AlignWithAttackPlane(transform.position), AlignWithAttackPlane(col.transform.position)) < targetDist && health)
            {
                if (health == lastTargetHit)
                {
                    continue;
                }
                nearestTarget = col.transform;
                targetDist = Vector3.Distance(AlignWithAttackPlane(transform.position), AlignWithAttackPlane(nearestTarget.position));
            }
        }

        if (targetDist < Mathf.Infinity)
        {
            direction = (AlignWithAttackPlane(nearestTarget.position) - AlignWithAttackPlane(transform.position)).normalized;
        }
        lastTargetHit = null;
        targetsHit.Clear();
    }
    private void BounceToNearbyTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadiusBounce, Physics2D.GetLayerCollisionMask(gameObject.layer));
        Transform nearestTarget = null;
        float targetDist = Mathf.Infinity;
        bool includesLastTarget = false;
        
        foreach (Collider col in hitColliders)
        {
            HealthComponent health = col.GetComponent<HealthComponent>();
            if (Vector3.Distance(AlignWithAttackPlane(transform.position), AlignWithAttackPlane(col.transform.position)) < targetDist && health)
            {
                if (health == lastTargetHit)
                {
                    includesLastTarget = true;
                    continue;
                }
                nearestTarget = col.transform;
                targetDist = Vector3.Distance(AlignWithAttackPlane(transform.position), AlignWithAttackPlane(nearestTarget.position));
            }
        }
        
        if (targetDist < Mathf.Infinity)
        {
            direction = (AlignWithAttackPlane(nearestTarget.position) - AlignWithAttackPlane(transform.position)).normalized;
        }
        else if (includesLastTarget)
        {
            direction = (AlignWithAttackPlane(lastTargetHit.transform.position) - AlignWithAttackPlane(transform.position)).normalized;
        }
        lastTargetHit = null;
        targetsHit.Clear();
    }

    private void Remove()
    {
        switch (projectileType)
        {
            case ProjectileType.Bullet:
                targetsHit.Clear();
                line.ClearLines();
                active = true;
                sprite.SetActive(true);
                model.SetActive(true);
                lastTargetHit = null;
                PoolService.Release<Projectile>(this, projectileType.ToString());
                break;
        }
    }
    private Vector3 AlignWithAttackPlane(Vector3 vector)
    {
        attackPlane = GameObject.FindGameObjectWithTag("AttackPlane").transform;
        return new(vector.x, attackPlane.position.y, vector.z);
    }

    private Transform FindHomingTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, homingRadius, Physics2D.GetLayerCollisionMask(gameObject.layer));
        Transform nearestTarget = null;
        float targetDist = Mathf.Infinity;
        bool includesLastTarget = false;

        foreach (Collider col in hitColliders)
        {
            HealthComponent health = col.GetComponent<HealthComponent>();
            if (Vector3.Distance(AlignWithAttackPlane(transform.position), AlignWithAttackPlane(col.transform.position)) < targetDist && health)
            {
                if (health == lastTargetHit)
                {
                    includesLastTarget = true;
                    continue;
                }
                nearestTarget = col.transform;
                targetDist = Vector3.Distance(AlignWithAttackPlane(transform.position), AlignWithAttackPlane(nearestTarget.position));
            }
        }
        if (targetDist < Mathf.Infinity)
        {
            return nearestTarget;
        }
        else if (includesLastTarget)
        {
            return lastTargetHit.transform;
        }
        return null;
    }
    private void TurnTowardsTarget()
    {
        Vector3 targetDirection = AlignWithAttackPlane(homingTarget.position-transform.position).normalized;
        targetDirection.y = 0;
        //float targetAngle = Vector3.Angle(targetDirection, Vector3.up);
        //float currentAngle = Vector3.Angle(direction, Vector3.up);
        //float finalRotation = targetAngle - currentAngle;
        //Vector3.RotateTowards()
        //if (Mathf.Abs(finalRotation) > 180)
        //{
        //}
        direction = Vector3.RotateTowards(direction, targetDirection, Mathf.Deg2Rad*turnSpeedInDegrees*Time.deltaTime, 0).normalized;

    }
}

public enum BounceType
{
    Default,
    BounceTowardsTargets,
    BounceOffTargets
}
