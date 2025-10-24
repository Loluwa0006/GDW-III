using UnityEngine;

public class ColorCorrection : MonoBehaviour
{
    public Material correctionMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (correctionMaterial == null)
        {
            Graphics.Blit(source, destination);
        }
        else
        {
            Graphics.Blit(source,destination, correctionMaterial);
        }
    }

}
