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
    public class Speed_Limiter : MonoBehaviour
    {
        [Header("Speed limiter on trigger stay")]
        [Space(10)]
        public string PlayerTag = "Player";
        public float playerSpeed = 40f;

        public string RacerTag = "Racer";
        public float racerSpeed = 40f;

        void OnTriggerStay(Collider col)
        {
            // player speed limit
            if (col.transform.tag == PlayerTag)
            {
                if (col.GetComponent<EasyCarController>())
                    col.GetComponent<EasyCarController>().maxSpeed = playerSpeed;
            }

            // racer speed limit
            if (col.transform.tag == RacerTag)
            {
                if (col.GetComponent<EasyCarController>())
                    col.GetComponent<EasyCarController>().maxSpeed = racerSpeed;

                // Disable racer nitro on speed limiters
                if (col.transform.tag == RacerTag)
                {
                    if (col.GetComponent<Racer_Nitro>())
                        col.GetComponent<Racer_Nitro>().inSpeedLimiter = true;
                }
            }
        }
        void OnTriggerExit(Collider col)
        {
            // Reset speed limit
            if (col.transform.tag == PlayerTag || col.transform.tag == RacerTag)
            {
                if (col.GetComponent<EasyCarController>())
                    col.GetComponent<EasyCarController>().maxSpeed =
                        col.GetComponent<EasyCarController>().originalMaxSpeed;

                // Enable racer nitro on speed limiters
                if (col.transform.tag == RacerTag)
                {
                    if (col.GetComponent<Racer_Nitro>())
                        col.GetComponent<Racer_Nitro>().inSpeedLimiter = false;
                }
            }
        }
    }
}