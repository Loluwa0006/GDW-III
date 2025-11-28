using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ITCGShaderManager : MonoBehaviour
{
   [SerializeField] List<MeshRenderer> primitives;
    [SerializeField] RawImage correctionImage;
    [SerializeField] ColorCorrection colorCorrector;


    public void ApplyNewMaterial(Material material)
    {
        foreach (var primitive in primitives)
        {
            primitive.material = material;
        }
    }

    public void ChangeLookUpTable(Material material)
    {
        correctionImage.enabled = true;
        correctionImage.material = material;
        colorCorrector.correctionMaterial = material;
    }

    public void DisableColorCorrection()
    {
        correctionImage.enabled = false;
    }
}
