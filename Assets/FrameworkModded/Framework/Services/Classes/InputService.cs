using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : MonoBehaviour, IFrameworkService
{
    [SerializeField] private InputActionAsset _inputActionAsset;
    [SerializeField] private PlayerInput _playerInput;

    public void Setup()
    {
        // Load in the default InputActionAsset
        _inputActionAsset = Resources.Load<InputActionAsset>("FrameworkInput");
        _playerInput = gameObject.AddComponent<PlayerInput>();
        _playerInput.actions = _inputActionAsset;
        _playerInput.defaultActionMap = _inputActionAsset.actionMaps[0].name;
        _playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
    }

    public void Connect(string actionPath, System.Action<InputAction.CallbackContext> callback) 
    {
        InputAction action = _playerInput.actions[actionPath];
        if (action == null)
        {
            throw new ArgumentException($"'{actionPath}' does not exist in '{_inputActionAsset.name}'.");
        }
        action.performed += callback;
        action.canceled += callback;
        action.Enable();
    }

    public void Connect(string actionName, string mapName, System.Action<InputAction.CallbackContext> callback) 
    {
        string actionPath = mapName + "/" + actionName;
        Connect(actionPath, callback);
    }

    public void Disconnect(string actionPath, System.Action<InputAction.CallbackContext> callback)
    {
        if (_playerInput != null)
        {
            InputAction action = _playerInput.actions[actionPath];
            if (action == null)
            {
                throw new ArgumentException($"'{actionPath}' does not exist in '{_inputActionAsset.name}'.");
            }

            action.performed -= callback;
            action.canceled -= callback;
            action.Disable();
        }
        else
        {
            
        }
    }

    public void Disconnect(string actionName, string mapName, System.Action<InputAction.CallbackContext> callback) 
    {
        string actionPath = mapName + "/" + actionName;
        Disconnect(actionPath, callback);
    }

    public void SwapActionMap(string mapNameOrID)
    {
        _playerInput.SwitchCurrentActionMap(mapNameOrID);
    }

    public bool IsActionActive(string actionPath)
    {
        InputAction action = _playerInput.actions[actionPath];
        if (action == null)
        {
            throw new ArgumentException($"'{actionPath}' does not exist in '{_inputActionAsset.name}'.");
        }

        return action.phase == InputActionPhase.Started || action.phase == InputActionPhase.Performed;
    }

    public bool IsActionActive(string actionName, string mapName) 
    {
        string actionPath = mapName + "/" + actionName;
        return IsActionActive(actionPath);
    }
}
