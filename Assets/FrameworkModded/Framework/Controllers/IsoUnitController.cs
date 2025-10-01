using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Framework;

// Requires
[RequireComponent(typeof(MovementComponent), typeof(ModelComponent))]

public class IsoUnitController : IsoCharacterController
{
    // Services
    private UnitService UnitService;

    // Components

    // Variables

    // Configuration

    protected override void Awake()
    {
        base.Awake();
        UnitService = Game.GetService<UnitService>();
    }

    protected override void Start()
    {
        base.Start();
        UnitService.RegisterUnit(this);
    }

    protected override void Update()
    {
        Move(_moveDirection);
    }

    void OnDestroy()
    {
        // Disconnect from connections
    }
}
