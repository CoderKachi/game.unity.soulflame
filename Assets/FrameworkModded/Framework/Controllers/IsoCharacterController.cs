using System;
using UnityEngine;
using static Framework;

// Requires
[RequireComponent(typeof(MovementComponent), typeof(ModelComponent))]
public class IsoCharacterController : MonoBehaviour
{
    // Components
    protected MovementComponent _MovementComponent;
    protected ModelComponent _ModelComponent;

    // Variables
    [SerializeField] protected Vector2 _moveDirection = Vector2.zero;
    [SerializeField] protected Transform _moveTarget;
    [SerializeField] protected Vector3? _movePosition;

    [SerializeField] protected float _moveVectorRotation = 0f;
    [SerializeField] protected Transform _lookTarget;
    [SerializeField] protected Vector3? _lookPosition;
    [SerializeField] protected float _lookSpeed = 8f;


    protected virtual void Awake()
    {
        TryGetComponent<MovementComponent>(out _MovementComponent);
        TryGetComponent<ModelComponent>(out _ModelComponent);
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (_lookTarget != null)
        {
            _lookPosition = _lookTarget.position;
        }
        Move(_moveDirection);
        LookTowards(_lookPosition);
    }

    public virtual void LookTowards(Transform targetTransform)
    {
        _lookTarget = targetTransform;
        LookTowards(_lookTarget.position);
    }

    public virtual void LookTowards(Vector3? nullableTargetPosition)
    {
        if (_ModelComponent == null) return;

        if (nullableTargetPosition == null)
        {
            LookTowards(Vector3.zero);
        }
        else
        {
            LookTowards(nullableTargetPosition.Value);
        }
    }

    public virtual void LookTowards(Vector3 targetPosition)
    {
        if (_ModelComponent == null) return;

        Quaternion currentRotation = _ModelComponent.GetRotation();
        Quaternion targetRotation = _ModelComponent.GetRotationTowards(targetPosition);
        Quaternion smoothedRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * _lookSpeed);

        _ModelComponent.SetRotation(smoothedRotation);
    }

    protected virtual void Move(Vector2 moveVector)
    {
        if (_MovementComponent == null) return;

        if (_moveVectorRotation != 0)
        {
            Vector3 rotateMoveVector = Quaternion.AngleAxis(_moveVectorRotation, Vector3.up) * new Vector3(moveVector.x, 0, moveVector.y);
            Vector2 finalMoveVector = new Vector2(rotateMoveVector.x, rotateMoveVector.z);

            _MovementComponent.SetMoveVector(finalMoveVector);
        }
        else
        {
            _MovementComponent.SetMoveVector(moveVector);
        }
    }

    // TO DO
    // add MoveTowards(Transform)
    // add MoveTowards(Position)
}
