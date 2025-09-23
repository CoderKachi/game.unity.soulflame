using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;


public class MovementComponentOld : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    //
    //float dashMultiplier = 2f;
    float dashStartTime;

    [SerializeField] Vector3 posOffset = new (0,1,0);

    [HideInInspector]
    public float waypointThreshold = 1f;

    bool isDashing = false;

    [HideInInspector]
    public Transform target;


    PathP path;

    float skinWidth = 0.06f;
    int collideIterations = 5;
    public Collider col;
    private LayerMask collisionMask;

    [HideInInspector]
    public Vector2 moveVector;

    private void Awake()
    {
        collisionMask = LayerMask.GetMask("Wall");
        
        Collider[] colliders = gameObject.GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            if (collider.GetType() != typeof(MeshCollider))
            {
                col = collider;
                return;
            }
        }

    }

    public void Move(Vector2 direction)
    {
        direction = direction.normalized;
        Vector3 vel = moveSpeed * Time.deltaTime * new Vector3(direction.x, 0, direction.y);
        transform.position += CollideAndSlide(vel, transform.position + posOffset);
    }

    public void MoveTo(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(destination.x, destination.z)) >= moveSpeed * Time.deltaTime)
        {
            Vector3 vel = moveSpeed * Time.deltaTime * new Vector3(direction.x, 0, direction.z);
            transform.position += CollideAndSlide(vel, transform.position + posOffset);
        }
        else
        {
            transform.position = new Vector3 (destination.x, transform.position.y, destination.z);
        }
    }

    public void SetMoveVector(Vector2 input)
    {
        moveVector = input;
    }

    private void Start()
    {
        //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        //Invoke("CancelPath", 5);
    }
    private void Update()
    {
        if (!isDashing)
        {
            Move(moveVector);
        }

        
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new PathP(waypoints, transform.position);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }
    
    public bool PathActive()
    {
        if (path != null)
        {
            return true;
        }
        else return false;
    }
    
    public bool RequestPath(Transform _target)
    {
        target = _target;
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        if (path != null) return true;
        return false;
    }

    public void CancelPath()
    {
        StopCoroutine("FollowPath");
        path = null;
    }

    IEnumerator FollowPath()
    {
        int i = 0;
        Vector2 destination;
        Vector2 playerPos;

        for (i = 0; i < path.lookPoints.Length;)
        {
            destination = new Vector2(path.lookPoints[i].x, path.lookPoints[i].z);
            playerPos = new Vector2(transform.position.x, transform.position.z);
            if (Vector2.Distance(destination, playerPos) > waypointThreshold)
            {
                MoveTo(path.lookPoints[i]);
            }
            else
            {
                i++;
            }
            yield return null;
        }

        CancelPath();
    }

    public async UniTask Dash(Vector3 dir, float dashSpeed, float dashDuration)
    {
        if (isDashing) return;
        isDashing = true;
        dashStartTime = Time.time;
        while (isDashing && Time.time - dashStartTime < dashDuration)
        {
            //await UniTask.Delay((int)(10f));
            dir = dir.normalized;
            Vector3 vel = dashSpeed * Time.deltaTime * new Vector3(dir.x, 0, dir.y);
            transform.position += CollideAndSlide(vel, transform.position + posOffset);
            await UniTask.Yield();
        }
        isDashing = false;
    }
    public async UniTask Dash(Vector3 dir, float dashSpeed, float dashDuration, AnimationCurve curve)
    {
        if (isDashing) return;
        isDashing = true;
        dashStartTime = Time.time;
        Move(Vector2.zero);
        float dashDurationCurrent = Time.time - dashStartTime;
        while (isDashing && dashDurationCurrent < dashDuration)
        {
            dashDurationCurrent = Time.time - dashStartTime;
            float speedMult = curve.Evaluate(dashDurationCurrent / dashDuration);
            //await UniTask.Delay((int)(10f));
            dir = dir.normalized;
            Vector3 vel = speedMult*dashSpeed * Time.deltaTime * new Vector3(dir.x, 0, dir.y);
            transform.position += CollideAndSlide(vel, transform.position + posOffset);
            await UniTask.Yield();
        }
        isDashing = false;
    }

    Vector3 CollideAndSlide(Vector3 vel, Vector3 pos)
    {
        Vector3 resultant = new Vector3(0, 0, 0);
        bool bounced = false;
        for (int i = 0; i <= collideIterations; i++)
        {
            bool b = false;
            float dist = vel.magnitude + skinWidth;

            RaycastHit hit;
            if (Physics.SphereCast(pos, col.bounds.extents.x, vel.normalized, out hit, dist, collisionMask))
            {
                
                b = true;
                Vector3 snapToSurface = vel.normalized * (hit.distance - skinWidth);
                vel = vel - snapToSurface;
                if (snapToSurface.magnitude <= skinWidth)
                {
                    snapToSurface = Vector3.zero;
                }

                float mag = vel.magnitude;
                vel = Vector3.ProjectOnPlane(vel, hit.normal).normalized;
                vel *= mag;
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                
                resultant += snapToSurface;
                pos += snapToSurface;
                bounced = true;
            }
            if (!b) break;
        }
        resultant = new Vector3(resultant.x, resultant.y, resultant.z);
        vel = new Vector3(vel.x, vel.y, vel.z);
        if (bounced) return resultant+vel;
        else return vel;
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }
}
