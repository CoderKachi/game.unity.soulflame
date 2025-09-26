using System;
using UnityEngine;

public class ModelComponent : MonoBehaviour
{
    // Variables
    public GameObject model;
    public VectorBool3 freezeRotation;
    public bool flipForward = false;

    public virtual void SetRotation(Quaternion setRotation)
    {
        if (model == null) return;
        
        Vector3 setEuler = setRotation.eulerAngles;

        if (freezeRotation.x) setEuler.x = 0;
        if (freezeRotation.y) setEuler.y = 0;
        if (freezeRotation.z) setEuler.z = 0;

        model.transform.rotation = Quaternion.Euler(setEuler);
    }

    public virtual Quaternion GetRotation()
    {
        //if (model == null) throw new Exception("ModelComponent has no Model set!");
        if (model == null) return default;

        return model.transform.rotation;
    }

    public virtual Quaternion GetRotationTowards(Vector3 targetPosition)
    {
        //if (model == null) throw new Exception("ModelComponent has no Model set!");
        if (model == null) return default;

        Vector3 direction = targetPosition - model.transform.position;

        if (direction.magnitude < 0.001f) return GetRotation();

        direction = direction.normalized;

        if (flipForward) direction = -direction;

        return Quaternion.LookRotation(direction);
    }
}