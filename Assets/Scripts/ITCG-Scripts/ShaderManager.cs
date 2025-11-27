using System.Collections.Generic;
using UnityEngine;

public class ShaderManager : MonoBehaviour
{
   [SerializeField] List<MeshRenderer> primitives;



    public void ApplyNewMaterial(Material material)
    {
        foreach (var primitive in primitives)
        {
            primitive.material = material;
        }
    }
}
