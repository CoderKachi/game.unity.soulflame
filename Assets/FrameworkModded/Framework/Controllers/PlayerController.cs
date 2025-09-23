using UnityEngine;
using UnityEngine.InputSystem;
using static Framework;
public class PlayerController : MonoBehaviour
{
    InputService InputService;
    MovementComponent characterMovement;
    WeaponComponent characterWeapon;
    private Vector2 screenMousePos = new(0, 1);


    private void Awake()
    {
        InputService = Game.GetService<InputService>();

        characterMovement = GetComponent<MovementComponent>();
        characterWeapon = GetComponent<WeaponComponent>();
        InputService.Connect("Gameplay/Move", Move);
        InputService.Connect("Fire", "Gameplay", Attack);
        InputService.Connect("FireSecondary", "Gameplay", AttackSecondary);
        InputService.Connect("Look", "Gameplay", Look);
        InputService.Connect("SwapWeapon", "Gameplay", SwapWeapon);

    }
    private void Update()
    {
        
        //characterWeapon.SetLookVector(screenMousePos);
    }

    void Move(InputAction.CallbackContext context)
    {
 
        Vector2 input = context.ReadValue<Vector2>();
        characterMovement.SetMoveVector(input);
        
        
    }

    void SwapWeapon(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            characterWeapon.AttackCancelled(true);
            characterWeapon.AttackCancelled(false);
            characterWeapon.SwapWeapon();
        }
    }

    void Roll()
    {

    }

    void Look(InputAction.CallbackContext context)
    {
        screenMousePos = context.ReadValue<Vector2>();
    }

    void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            characterWeapon.AttackStarted(true);
        }
        if (context.canceled)
        {
            characterWeapon.AttackCancelled(true);
        }
        
    }
    void AttackSecondary(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            characterWeapon.AttackStarted(false);
        }
        if (context.canceled)
        {
            characterWeapon.AttackCancelled(false);
        }

    }

}
