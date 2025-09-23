using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Framework;

// Requires
[RequireComponent(typeof(MovementComponent), typeof(ModelComponent), typeof(CameraComponent))]
[RequireComponent(typeof(ScreenToPlaneComponent))]

public class IsoPlayerController : IsoCharacterController
{
    // Services
    private InputService InputService;

    // Components
    protected CameraComponent _CameraComponent;
    protected ScreenToPlaneComponent _ScreenToPlaneComponent;

    // Variables
    protected Vector2 _currentMousePosition;

    protected override void Awake()
    {
        base.Awake();

        InputService = Game.GetService<InputService>();

        InputService.Connect("Gameplay/Look", LookWithMouse);
        InputService.Connect("Gameplay/Move", MoveWithKeyboard);
        InputService.Connect("Gameplay/Scroll", Zoom);

        TryGetComponent<CameraComponent>(out _CameraComponent);
        TryGetComponent<ScreenToPlaneComponent>(out _ScreenToPlaneComponent);
    }

    protected override void Update()
    {
        Vector3 worldMousePosition = _ScreenToPlaneComponent.ScreenToPlane(_currentMousePosition);

        Move(_moveDirection);
        LookTowards(worldMousePosition);
    }

    void LookWithMouse(InputAction.CallbackContext context)
    {
        _currentMousePosition = context.ReadValue<Vector2>();
    }

    void MoveWithKeyboard(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    void Zoom(InputAction.CallbackContext context)
    {
        _CameraComponent.Zoom(-context.ReadValue<float>());
    }

    void OnDestroy()
    {
        InputService.Disconnect("Gameplay/Look", LookWithMouse);
        InputService.Disconnect("Gameplay/Move", MoveWithKeyboard);
        InputService.Disconnect("Gameplay/Scroll", Zoom);
    }
}
