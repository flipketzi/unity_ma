using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class ApplyFilters : MonoBehaviour
{
    [SerializeField] private Material sobelMaterial;
    [SerializeField] private Material invertMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, rt, sobelMaterial);
        Graphics.Blit(rt, destination, invertMaterial);
        RenderTexture.ReleaseTemporary(rt);
    }
}
