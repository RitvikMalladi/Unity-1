//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Suspension_Loader : MonoBehaviour
    {
        public int carID;
        Vector3 originalPosition;

        void Awake()
        {

            originalPosition = transform.localPosition;

            transform.localPosition = new Vector3(originalPosition.x,
                     originalPosition.y +
                     PlayerPrefs.GetFloat("Car" + carID.ToString() + "Suspension"),
                     originalPosition.z);

        }

        public void Select_Suspension(float value)
        {

            PlayerPrefs.SetFloat("Car" + carID.ToString() + "Suspension", value);

            transform.localPosition = new Vector3(originalPosition.x,
                      originalPosition.y + value,
                      originalPosition.z);

        }
    }
}