using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class ShadowProjector_Settings : MonoBehaviour
    {
        void Start()
        {
            if (PlayerPrefs.GetInt("QualityLevel") >= 4)
            {
                gameObject.SetActive(false);
            }
        }
    }
}