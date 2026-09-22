//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALIyerEdon;

namespace ALIyerEdon
{
    public class Lap_Trigger : MonoBehaviour
    {
        public string PlayerTag = "Player";
        public string RacerTag = "Racer";

        void OnTriggerExit(Collider col)
        {
            if (col.transform.tag == PlayerTag || col.transform.tag == RacerTag)
            {
                col.GetComponent<Car_Position>().currentLap =
                         col.GetComponent<Car_Position>().currentLap + 1;
            }
        }
    }
}