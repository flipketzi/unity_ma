using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StuPro{

    public class EdgeRenderer : MonoBehaviour
    {
        Texture2D outputTexture2D;
        [SerializeField] bool edgeRendering = true;
        [SerializeField] RenderTexture inputTexture;
        [SerializeField] RenderTexture targetTexture;

        private Texture2D toTexture2D(RenderTexture rt){
            Texture2D t = new Texture2D(512, 512, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            t.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            t.Apply();
            return t;
        }

        // Start is called before the first frame update
        void Start()
        {
            outputTexture2D = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.RGB24, false);
        }

        // Update is called once per frame
        void Update()
        {
            Texture2D inputTexture2D = toTexture2D(inputTexture);
            for(int i=0; i<inputTexture2D.width; i++){
                for(int j=0; j<inputTexture2D.height; j++){
                    Color pixelColor = inputTexture2D.GetPixel(i, j);
                    outputTexture2D.SetPixel(i, j, new Color(pixelColor.grayscale, pixelColor.grayscale, pixelColor.grayscale, 1));
                }
            }
            Graphics.Blit(outputTexture2D, targetTexture);
        }
    }
}

