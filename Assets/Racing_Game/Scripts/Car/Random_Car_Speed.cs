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
    public class Random_Car_Speed : MonoBehaviour
    {
        public float delayInterval = 5f;
        public float speedMin = 40f;
        public float speedMax = 110f;

        IEnumerator Start()
        {
            while (true)
            {

                if (GetComponent<EasyCarController>())
                {
                    float randomSpeed = Random.Range(speedMin, speedMax);
                    GetComponent<EasyCarController>().maxSpeed = randomSpeed;
                    GetComponent<EasyCarController>().originalMaxSpeed = randomSpeed;
                }

                yield return new WaitForSeconds(delayInterval);
            }
        }
    }
}