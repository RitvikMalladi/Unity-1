using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class CameraRotator : MonoBehaviour
    {
        //Assign a GameObject in the Inspector to rotate around
        public GameObject target;

        public float speed = 20f;

        void Update()
        {
            // Spin the object around the target at 20 degrees/second.
            transform.RotateAround(target.transform.position, Vector3.up, speed * Time.deltaTime);
        }
    }
}