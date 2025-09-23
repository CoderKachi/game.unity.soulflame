using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static Framework;

// Requires
[RequireComponent(typeof(CameraComponent))]
public class PaperPlayerController : PaperCharacterController
{
    // Services
    private InputService InputService;

    // Components
    protected CameraComponent _CameraComponent;

    // Variables
    private Vector2 _currentMousePosition = Vector2.zero;
    private Vector2 _currentGamepadVector = Vector2.zero;
    private Vector2 _lastMovementInput = Vector2.zero;
    [SerializeField] private float _panningDeadzone = 5f;
    //private bool lerping = false;
    private bool mouseVector = true;
    [SerializeField]
    private AnimationCurve rollCurve;

    protected override void Awake()
    {
        base.Awake();

        InputService = Game.GetService<InputService>();

        TryGetComponent<CameraComponent>(out _CameraComponent);

        InputService.Connect("Gameplay/Move", Move);
        InputService.Connect("Fire", "Gameplay", Attack);
        InputService.Connect("FireSecondary", "Gameplay", AttackSecondary);
        InputService.Connect("Look", "Gameplay", LookMouse);
        InputService.Connect("LookGamepad", "Gameplay", LookGamepad);
        InputService.Connect("SwapWeapon", "Gameplay", SwapWeapon);
        InputService.Connect("Gameplay/Reload", Reload);
        InputService.Connect("Roll", Roll);
    }

    protected override void Update()
    {
        base.Update();
        Vector3 worldMousePosition;

        if (mouseVector)
        {
            worldMousePosition = _ScreenToPlaneComponent.ScreenToPlane(_currentMousePosition);
        }
        else
        {
            float screenX = Screen.width;
            float screenY = Screen.height;

            Debug.Log(screenX);

            Vector2 screenGamepadPosition = new Vector2((screenX * 0.5f) + (screenX * 0.5f * _currentGamepadVector.x), (screenY * 0.5f) + (screenY * 0.5f * _currentGamepadVector.y));
            worldMousePosition = _ScreenToPlaneComponent.ScreenToPlane(screenGamepadPosition);
        }

        LookTowards(worldMousePosition);
        if ((this.transform.position - worldMousePosition).magnitude > _panningDeadzone)
        {
            _CameraComponent.PanTowards(worldMousePosition);
        }
        else
        {
            _CameraComponent.PanTowards(null);
        }
        
    }

    void Move(InputAction.CallbackContext context)
    {
        
        Vector2 input = context.ReadValue<Vector2>();
        _lastMovementInput = input.magnitude > 0.02f ? input : _lastMovementInput;
            _MovementComponent.SetMoveVector(input);
    }

    void Roll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            
            _ = _MovementComponent.Dash(_lastMovementInput, 10f, 0.5f, rollCurve);
        }
    }
    void SwapWeapon(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _WeaponComponent.AttackCancelled(true);
            _WeaponComponent.AttackCancelled(false);
            _WeaponComponent.SwapWeapon();

            Weapon currentWeapon = _WeaponComponent.GetCurrentWeapon();
            _attackSpawn.localPosition = currentWeapon.firePointInitial;
            if (spriteArm != null)
            {
                spriteArm.sprite = currentWeapon.model;
            }
        }
    }

    void LookMouse(InputAction.CallbackContext context)
    {
        mouseVector = true;
        _currentMousePosition = context.ReadValue<Vector2>();
    }

    void LookGamepad(InputAction.CallbackContext context)
    {
        // Get mouse position projected to plane and use to calculate look vector
        mouseVector = false;
        Vector2 newGamepadVector = context.ReadValue<Vector2>();

        if (newGamepadVector.magnitude < 0.125f) return;

        Vector2 newGamepadVectorSquare = new Vector2
        (
            newGamepadVector.x * Mathf.Sqrt(1 - (newGamepadVector.y * newGamepadVector.y) / 2f),
            newGamepadVector.y * Mathf.Sqrt(1 - (newGamepadVector.x * newGamepadVector.x) / 2f)
        );

        _currentGamepadVector = newGamepadVectorSquare;

        //_currentGamepadVector = newGamepadVector.magnitude > 0.01f ? newGamepadVector : _currentGamepadVector;
    }

    void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _WeaponComponent.AttackStarted(true);
        }
        if (context.canceled)
        {
            _WeaponComponent.AttackCancelled(true);
        }
    }

    void Reload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Try to reload");
            _WeaponComponent.Reload();
        }
        if (context.canceled)
        {

        }
    }

    void AttackSecondary(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _WeaponComponent.AttackStarted(false);
        }
        if (context.canceled)
        {
            _WeaponComponent.AttackCancelled(false);
        }

    }
}
