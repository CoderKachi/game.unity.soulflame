using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;


public class MovementComponent : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    private float moveSpeedInitial;

    [SerializeField]
    bool reset = false;

    //
    //float dashMultiplier = 2f;
    float dashStartTime;

    [SerializeField] Vector3 posOffset = new (0,1,0);

    bool isDashing = false;

    [HideInInspector]
    public Transform target;

    public float repelStrength = 3f;
    public float skinWidth = 0.001f;
    public int collideIterations = 2;
    public Collider col;
    public LayerMask collisionMask;
    public LayerMask pushMask;

    PathP path;

    [HideInInspector]
    public Vector2 moveVector;

    private void Awake()
    {
        //collisionMask = LayerMask.GetMask("Wall");
        moveSpeedInitial = moveSpeed;
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
    private void Start()
    {
        
        //Invoke("CancelPath", 5);
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new PathP(waypoints, transform.position);
        }
        else
        {
            Debug.Log("path not found");
        }
    }

    public void Move(Vector2 direction)
    {
        direction = direction.normalized;
        Vector3 vel = moveSpeed * Time.deltaTime * new Vector3(direction.x, 0, direction.y);
        Vector2 repel = Repel();
        vel += new Vector3(repel.x, 0, repel.y) * Time.deltaTime;
        transform.position += CollideAndSlide(vel, transform.position + posOffset);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
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
    
    private void Update()
    {
        if (!isDashing)
        {
            Move(moveVector);
        }
        if (reset)
        {
            reset = false;
            PathRequestManager.RequestPath(transform.position, new Vector3(7, 0.5f, 15), OnPathFound);
        }
        
        //Repel();
    }

    private Vector2 Repel()
    {
        Collider[] collisions = Physics.OverlapSphere(transform.position + new Vector3(0, 1, 0), col.bounds.extents.x, pushMask);
        Vector3 location = new Vector3(0, 0, 0);
        foreach (Collider col in collisions)
        {
            location = location + col.transform.position;
        }
        if (collisions.Length == 0)
        {
            return Vector2.zero;
        }
        location = location / collisions.Length;
        location = transform.position - location;
        return new Vector2(location.x, location.z) * repelStrength;;
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
            Vector3 vel = speedMult * dashSpeed * Time.deltaTime * new Vector3(dir.x, 0, dir.y);
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
    public void UpdatePower(int value)
    {
        moveSpeed = moveSpeedInitial + ((value+1)/ 200f);
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }
}
