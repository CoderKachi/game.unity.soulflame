using System;
using UnityEngine;
using static Framework;

// Requires
[RequireComponent(typeof(MovementComponent), typeof(WeaponComponent), typeof(ScreenToPlaneComponent))]
public class PaperCharacterController : MonoBehaviour
{
    // Components
    protected MovementComponent _MovementComponent;
    protected WeaponComponent _WeaponComponent;
    protected ScreenToPlaneComponent _ScreenToPlaneComponent;

    // Variables
    protected Vector2 _lookVector = Vector2.up;
    protected Vector2 _lookVectorWeapon = Vector2.up;
    protected Transform _attackSpawn;
    protected Transform _attackSpawnProjected;

    public GameObject characterArm;
    public SpriteRenderer spriteBody;
    public SpriteRenderer spriteArm;

    protected virtual void Awake()
    {
        TryGetComponent<MovementComponent>(out _MovementComponent);
        TryGetComponent<ScreenToPlaneComponent>(out _ScreenToPlaneComponent);
        TryGetComponent<WeaponComponent>(out _WeaponComponent);

        ResolveAttackSpawn();

        _ScreenToPlaneComponent.SetMask(LayerMask.GetMask("Mouse"));
        _WeaponComponent.SetAttackSpawn(_attackSpawnProjected);
    }

    protected virtual void Start()
    {
        _attackSpawn.localPosition = _WeaponComponent.GetCurrentWeapon().firePointInitial;

        if (spriteArm != null)
        {
            Weapon currentWeapon = _WeaponComponent.GetCurrentWeapon();
            if (currentWeapon != null)
            {
                spriteArm.sprite = currentWeapon.model;
            }
        }
    }

    protected virtual void ResolveAttackSpawn()
    {
        _attackSpawn = (_attackSpawn == null) ? new GameObject("AttackSpawn").transform : _attackSpawn;
        _attackSpawnProjected = (_attackSpawnProjected == null) ? new GameObject("AttackSpawnProjected").transform : _attackSpawnProjected;
        _attackSpawnProjected.transform.SetParent(transform);

        // Resolve gameObjectArm
        characterArm = (characterArm == null) ? transform.Find("Arm").gameObject : characterArm;
        if (characterArm != null)
        {
            _attackSpawn.transform.SetParent(characterArm.transform);
        }
        else
        {
            _attackSpawn.transform.SetParent(transform);
        }
    }

    protected virtual void FlipSprite(bool flipX)
    {
        if (spriteBody != null)
        {
            spriteBody.flipX = flipX;
        }

        if (spriteArm != null)
        {
            //spriteArm.flipX = flipX;
            if (flipX)
            {
                spriteArm.transform.localScale = new(spriteArm.transform.localScale.x, -MathF.Abs(spriteArm.transform.localScale.y), spriteArm.transform.localScale.z);

            }
            else
            {
                spriteArm.transform.localScale = new(spriteArm.transform.localScale.x, MathF.Abs(spriteArm.transform.localScale.y), spriteArm.transform.localScale.z);
            }
            
        }
    }

    protected virtual void Update()
    {
        // Resolve Arm and Sprite rotation
        float currentAngle = Mathf.Atan2(_lookVector.y, _lookVector.x) * Mathf.Rad2Deg;

        if (currentAngle > 90 || currentAngle < -90)
        {
            _attackSpawn.localPosition = new(MathF.Abs(_attackSpawn.localPosition.x), _attackSpawn.localPosition.y, _attackSpawn.localPosition.z);
            characterArm.transform.eulerAngles = new(45, characterArm.transform.rotation.y, currentAngle);
            currentAngle += 180;
            FlipSprite(true);
        }
        else
        {
            _attackSpawn.localPosition = new(MathF.Abs(_attackSpawn.localPosition.x), _attackSpawn.localPosition.y, _attackSpawn.localPosition.z);
            characterArm.transform.eulerAngles = new(45, characterArm.transform.rotation.y, currentAngle);
            FlipSprite(false);
        }

        // Set weapon look vector
        _attackSpawnProjected.position = _ScreenToPlaneComponent.WorldToPlane(_attackSpawn.position);
        _WeaponComponent.SetAttackDirecton(_lookVector);
    }

    public virtual void LookTowards(Vector3 targetPosition)
    {
        // This is intended to be used for the XZ Plane
        Vector2 targetPositionXZ = new(targetPosition.x, targetPosition.z);
        Vector2 currentPositionXZ = new(transform.position.x, transform.position.z);
        _lookVector = (targetPositionXZ - currentPositionXZ).normalized;
    }

    protected virtual void Move(Vector2 moveVector)
    {
        _MovementComponent.SetMoveVector(moveVector);
    }

    public virtual MovementComponent GetMovementComponent()
    {
        return _MovementComponent;
    }

    public virtual WeaponComponent GetWeaponComponent()
    {
        return _WeaponComponent;
    }

    protected virtual void Roll()
    {

    }
}
