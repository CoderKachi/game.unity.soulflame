using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum UnitFaction
{
    None,
    Neutral,
    Flame,
    Monster,
}

public class UnitService : MonoBehaviour, IFrameworkService
{
    [SerializeField] private List<IsoUnitController> _units = new();

    public void Setup()
    {

    }

    public void RegisterUnit(IsoUnitController unit)
    {
        _units.Add(unit);
    }

    public void UnregisterUnit(IsoUnitController unit)
    {
        _units.Remove(unit);
    }

    public List<IsoUnitController> GetUnits(UnitFaction? unitFaction = null)
    {
        // UnitFaction should be used for filtering later on

        // Return a copy
        List<IsoUnitController> clonedList = new List<IsoUnitController>(_units);
        return clonedList;
    }
}
