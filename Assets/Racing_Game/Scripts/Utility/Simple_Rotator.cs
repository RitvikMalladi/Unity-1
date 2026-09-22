//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Simple_Rotator : MonoBehaviour
    {
        public float speed = 5f;
        public Vector3 angle;


        public void Update()
        {
            transform.Rotate(angle, speed * Time.deltaTime);
        }
    }
}