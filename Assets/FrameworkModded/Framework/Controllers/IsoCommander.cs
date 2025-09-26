using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Framework;

// Requires
[RequireComponent(typeof(MovementComponent), typeof(ModelComponent), typeof(CameraComponent))]
[RequireComponent(typeof(ScreenToPlaneComponent))]

public class IsoCommander : IsoCharacterController
{
    // Services
    private InputService InputService;

    // Components
    protected CameraComponent _CameraComponent;
    protected ScreenToPlaneComponent _ScreenToPlaneComponent;

    // Variables
    protected Vector2 _currentMousePosition;
    [SerializeField] protected List<GameObject> _currentSelected;

    // Control States
    protected bool _actionIsModified = false;
    protected bool _actionIsDragging = false;
    protected Vector2 _dragStartPosition;
    protected Vector2 _dragEndPosition;
    protected float _dragThreshold = 10f; // Pixels

    // Configuration
    public LayerMask selectableLayers;
    public LayerMask controlableLayers;
    public bool panTowardsMouse;

    protected override void Awake()
    {
        base.Awake();

        InputService = Game.GetService<InputService>();

        InputService.Connect("Gameplay/ActionPrimary", ActionPrimary);
        InputService.Connect("Gameplay/ActionPrimaryModifier", ActionPrimaryModifier);
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

        if (panTowardsMouse)
        {
            _CameraComponent.PanTowards(worldMousePosition);
        }
    }

    void TrySelectUnits()
    {
        float dragDistance = Vector2.Distance(_dragStartPosition, _dragEndPosition);

        if (dragDistance < _dragThreshold)
        {
            // Select a single unit
            Ray ray = _CameraComponent.GetCamera().ScreenPointToRay(_currentMousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 999f, controlableLayers))
            {
                if (!_actionIsModified)
                {
                    // Clear if not shift clicking
                    _currentSelected.Clear();
                }

                // Add the unit to the selected list
                _currentSelected.Add(hit.collider.gameObject);
            }
            else
            {
                _currentSelected.Clear();
            }
        }
        else
        {
            // Select mutliple units

            // Fix this later, create a better method of getting all Units
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

            foreach (GameObject unit in units)
            {
                if ((controlableLayers & (1 << unit.layer)) == 0) continue;
                Renderer unitRenderer = unit.GetComponentInChildren<Renderer>();
                if (unitRenderer != null) continue;

                Bounds unitBounds = unitRenderer.bounds;
                Vector3 screenMin = _CameraComponent.GetCamera().WorldToScreenPoint(unitBounds.min);
                Vector3 screenMax = _CameraComponent.GetCamera().WorldToScreenPoint(unitBounds.max);

                Rect unitRect = Rect.MinMaxRect
                (
                    Mathf.Min(screenMin.x, screenMax.x),
                    Mathf.Min(screenMin.y, screenMax.y),
                    Mathf.Max(screenMin.x, screenMax.x),
                    Mathf.Max(screenMin.y, screenMax.y)
                );

                
            }

            _currentSelected.Clear();
        }
    }

    void ActionPrimary(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _dragStartPosition = _currentMousePosition;
                break;
            case InputActionPhase.Canceled:
                _dragEndPosition = _currentMousePosition;
                TrySelectUnits();
                break;
        }
    }

    void ActionPrimaryModifier(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _actionIsModified = true;
                break;
            case InputActionPhase.Canceled:
                _actionIsModified = false;
                break;
        }
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
        InputService.Disconnect("Gameplay/ActionPrimary", ActionPrimary);
        InputService.Disconnect("Gameplay/Look", LookWithMouse);
        InputService.Disconnect("Gameplay/Move", MoveWithKeyboard);
        InputService.Disconnect("Gameplay/Scroll", Zoom);
    }
}
