using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class ApplyFilters : MonoBehaviour
{
    [SerializeField] public Material material;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(material == null){
            Graphics.Blit(source, destination);
        }
        //RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
        //Material invertMaterial = new Material (invertShader);
        Graphics.Blit(source, destination, material);
        //Graphics.Blit(source, destination, sobelMaterial);
        //RenderTexture.ReleaseTemporary(rt);
    }
}
