using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ALIyerEdon
{
    public class CustomCameraView : MonoBehaviour
    {
        public float distance;
        public float height;
        public float heightOffset;

        void Start()
        {
            if (FindObjectOfType<SmoothFollow>())
            {
                FindObjectOfType<SmoothFollow>().distance = distance;
                FindObjectOfType<SmoothFollow>().height = height;
                FindObjectOfType<SmoothFollow>().offset = 
                    new Vector3(0, heightOffset, 0);
            }
            if (FindObjectOfType<SmoothFollow2>())
            {
                FindObjectOfType<SmoothFollow2>().distance = distance;
                FindObjectOfType<SmoothFollow2>().height = height;
                FindObjectOfType<SmoothFollow2>().Angle = heightOffset;
            }
        }
    }
}