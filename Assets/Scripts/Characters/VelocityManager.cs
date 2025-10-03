using UnityEngine;
using System.Collections.Generic;
public class VelocityManager : MonoBehaviour
{
    public static Vector3 MISSING_VELOCITY_VALUE = new Vector3(-1.0f, -1.0f, -1.0f);

    [SerializeField] Rigidbody _rb;

    Vector3 intervalVelocity;

    Dictionary<string, Vector3> externalVelocities = new();

    public void AddInternalVelocity(Vector3 speed)
    {
        intervalVelocity += speed;
    }

    public void AddExternalSpeed(Vector3 speed, string source)
    {
        externalVelocities[source] = speed;
    }

    public void EditExternalSpeed(string source, Vector3 newSpeed)
    {
        if (externalVelocities.ContainsKey(source))
        {
            if (newSpeed == Vector3.zero)
            {
                externalVelocities.Remove(source);
            }
            else 
            {
                externalVelocities[source] = newSpeed;
            }
            
        }
    }
    public Vector3 GetInternalSpeed()
    {
        return intervalVelocity;
    }
    public Vector3 GetExternalSpeed(string source)
    {
        if (externalVelocities.ContainsKey(source))
        {
            return externalVelocities[source];
        }
        return MISSING_VELOCITY_VALUE;
    }

    public void RemoveExternalSpeedSource(string source)
    {
        if (externalVelocities.ContainsKey(source))
        {
            externalVelocities.Remove(source);
        }
    }
    public void ClearExternalSpeed(string source)        //source is used to track which functions are calling this, as its very dangerous
    {
        externalVelocities.Clear();
    }
    public void ClearInternalSpeed(string source)
    {
        intervalVelocity = Vector3.zero;
    }

    public void OverwriteInternalSpeed(Vector3 newSpeed)
    {
        intervalVelocity = newSpeed;
    }

    public void ClampInternalVelocity(float length)
    {
        intervalVelocity = Vector3.ClampMagnitude(intervalVelocity, length);
    }


    private void FixedUpdate()
    {
        Vector3 finalVelocity = intervalVelocity;
        foreach (var v in externalVelocities.Keys)
        {
            finalVelocity += externalVelocities[v];
        }
        _rb.linearVelocity = finalVelocity;
    }
}
