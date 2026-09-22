using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ALIyerEdon
{
    [ExecuteInEditMode]
    public class Imposter_Exposure : MonoBehaviour
    {
        public float exposure = 1f;
        public Color color;
        public Material[] targetMaterials;

        public bool apply = false;
        // Start is called before the first frame update
        void Start()
        {
            Apply_Imposter_Exposure(exposure);
        }

        // Update is called once per frame
        void Update()
        {
            if(apply)
            {
                Apply_Imposter_Exposure(exposure);

                apply = false;
            }
        }

        public void Apply_Imposter_Exposure(float exposureValue)
        {
            foreach(Material mat in targetMaterials)
            {
                mat.SetColor("_BaseColor", color * exposureValue);
            }
        }
    }
}