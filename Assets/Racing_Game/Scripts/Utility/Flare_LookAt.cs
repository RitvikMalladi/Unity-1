//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Flare_LookAt : MonoBehaviour
    {
        public float updateInterval = 1f;
        Transform target;
        bool isVisible;

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            target = GameObject.FindGameObjectWithTag("Player").transform;
            transform.LookAt(target);

            while (true)
            {

                if (!isVisible)
                {
                    transform.LookAt(target);
                }

                yield return new WaitForSeconds(updateInterval);

            }
        }

        void OnBecameVisible()
        {
            isVisible = false;
        }

        void OnBecameInvisible()
        {
            isVisible = true;
        }
    }
}