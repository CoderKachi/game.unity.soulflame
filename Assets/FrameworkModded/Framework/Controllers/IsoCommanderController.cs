using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Framework;

// Requires
[RequireComponent(typeof(MovementComponent), typeof(ModelComponent), typeof(CameraComponent))]
[RequireComponent(typeof(ScreenToPlaneComponent))]

public class IsoCommanderController : IsoCharacterController
{
    // Services
    private InputService InputService;
    private UnitService UnitService;

    // Components
    protected CameraComponent _CameraComponent;
    protected ScreenToPlaneComponent _ScreenToPlaneComponent;

    // Variables
    protected Vector2 _currentMousePosition;
    [SerializeField] protected List<IsoUnitController> _currentSelected;

    // Control States
    protected bool _actionIsModified = false;
    protected bool _actionIsDragging = false;
    protected Vector2 _dragStartPosition;
    protected Vector2 _dragEndPosition;
    protected float _dragThreshold = 10f; // Pixels

    // Debugging
    protected Texture2D _uiTexture;
    [SerializeField] protected Color _dragColor = new Color(255, 255, 255, 50);
    [SerializeField] protected Color _selectedColor = new Color(0, 255, 0, 50);
    [SerializeField] protected Color _controllableColor = new Color(0, 0, 255, 50);

    // Configuration
    public LayerMask selectableLayers;
    public LayerMask controllableLayers;
    public bool panTowardsMouse;

    protected override void Awake()
    {
        base.Awake();

        InputService = Game.GetService<InputService>();
        UnitService = Game.GetService<UnitService>();

        InputService.Connect("Gameplay/ActionPrimary", ActionPrimary);
        InputService.Connect("Gameplay/ActionPrimaryModifier", ActionPrimaryModifier);
        InputService.Connect("Gameplay/Look", LookWithMouse);
        InputService.Connect("Gameplay/Move", MoveWithKeyboard);
        InputService.Connect("Gameplay/Scroll", Zoom);

        TryGetComponent<CameraComponent>(out _CameraComponent);
        TryGetComponent<ScreenToPlaneComponent>(out _ScreenToPlaneComponent);

        // 1x1 white texture
        _uiTexture = new Texture2D(1, 1);
        _uiTexture.SetPixel(0, 0, Color.white);
        _uiTexture.Apply();
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

    bool IsControllable(IsoUnitController unit)
    {
        // Convert unit.layer to a bitmask and AND with controllable mask
        return (controllableLayers.value & (1 << unit.gameObject.layer)) != 0;
    }

    void TrySelectUnits()
    {
        float dragDistance = Vector2.Distance(_dragStartPosition, _dragEndPosition);

        if (dragDistance < _dragThreshold)
        {
            // Select a single unit
            Ray ray = _CameraComponent.GetCamera().ScreenPointToRay(_currentMousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 999f, controllableLayers))
            {
                // Clear if not shift clicking
                if (!_actionIsModified) _currentSelected.Clear();

                IsoUnitController unit = hit.collider.gameObject.GetComponent<IsoUnitController>();
                if (unit == null) return;

                if (!_currentSelected.Contains(unit))
                {
                    _currentSelected.Add(unit);
                }
            }
            else
            {
                _currentSelected.Clear();
            }
        }
        else
        {
            // Select multiple units
            Rect selectionRect = ScreenToRect(_dragStartPosition, _dragEndPosition);

            // Clear if not shift clicking
            if (!_actionIsModified) _currentSelected.Clear();

            List<IsoUnitController> _units = UnitService.GetUnits();

            foreach (IsoUnitController unit in _units)
            {
                if (!IsControllable(unit)) continue;
                Renderer unitRenderer = unit.gameObject.GetComponent<Renderer>();
                if (unitRenderer == null) continue;
                Rect unitRect = RendererBoundsToRect(unitRenderer, _CameraComponent.GetCamera());

                if (selectionRect.Overlaps(unitRect, true))
                {
                    if (!_currentSelected.Contains(unit))
                    {
                        _currentSelected.Add(unit);
                    }
                }
            }
        }
    }

    void ActionPrimary(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _dragStartPosition = _currentMousePosition;
                _actionIsDragging = true;
                break;
            case InputActionPhase.Canceled:
                _dragEndPosition = _currentMousePosition;
                _actionIsDragging = false;
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

    Rect ScreenToRect(Vector2 startPosition, Vector2 endPosition)
    {
        // Calculate Left Corner of Rect
        float x = Mathf.Min(startPosition.x, endPosition.x);
        float y = Mathf.Min(Screen.height - startPosition.y, Screen.height - endPosition.y); // flip Y

        float width = Mathf.Abs(endPosition.x - startPosition.x);
        float height = Mathf.Abs(endPosition.y - startPosition.y);

        return new Rect(x, y, width, height);
    }

    Rect RendererBoundsToRect(Renderer renderer, Camera cam)
    {
        Bounds b = renderer.bounds;

        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(b.min.x, b.min.y, b.min.z);
        corners[1] = new Vector3(b.min.x, b.min.y, b.max.z);
        corners[2] = new Vector3(b.min.x, b.max.y, b.min.z);
        corners[3] = new Vector3(b.min.x, b.max.y, b.max.z);
        corners[4] = new Vector3(b.max.x, b.min.y, b.min.z);
        corners[5] = new Vector3(b.max.x, b.min.y, b.max.z);
        corners[6] = new Vector3(b.max.x, b.max.y, b.min.z);
        corners[7] = new Vector3(b.max.x, b.max.y, b.max.z);

        float xMin = float.MaxValue;
        float yMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMax = float.MinValue;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 screenPoint = cam.WorldToScreenPoint(corners[i]);

            screenPoint.y = Screen.height - screenPoint.y;

            xMin = Mathf.Min(xMin, screenPoint.x);
            yMin = Mathf.Min(yMin, screenPoint.y);
            xMax = Mathf.Max(xMax, screenPoint.x);
            yMax = Mathf.Max(yMax, screenPoint.y);
        }

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    void OnGUI()
    {
        Rect dragRect = new Rect(0, 0, 0, 0);

        if (_actionIsDragging)
        {
            dragRect = ScreenToRect(_dragStartPosition, Mouse.current.position.ReadValue());

            // Draw selection box
            Color oldColor = GUI.color;
            GUI.color = _dragColor;
            GUI.DrawTexture(dragRect, _uiTexture);
            GUI.color = oldColor;
        }

        foreach (IsoUnitController unit in UnitService.GetUnits())
        {
            if (!IsControllable(unit)) continue;
            Renderer unitRenderer = unit.GetComponent<Renderer>();
            if (unitRenderer == null) continue;
            Rect unitRect = RendererBoundsToRect(unitRenderer, _CameraComponent.GetCamera());

            // Draw unit box
            Color cacheColor = GUI.color;

            if (_currentSelected.Contains(unit) || dragRect.Overlaps(unitRect, true))
            {
                GUI.color = _selectedColor;
            }
            else
            {
                GUI.color = _controllableColor;
            }
            GUI.DrawTexture(unitRect, _uiTexture);
            GUI.color = cacheColor;
        }
    }
}
