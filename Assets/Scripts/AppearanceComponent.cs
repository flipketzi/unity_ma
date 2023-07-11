using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearanceComponent : MonoBehaviour
{
    [SerializeField] public List<Material> materialList = new List<Material>();
    [SerializeField] public bool colorVariance = false;
    [SerializeField] public bool uniformColorVariance = false;
    [SerializeField] public float materialColorVariance = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
