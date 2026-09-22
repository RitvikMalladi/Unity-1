using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ALIyerEdon
{
    public class ObjectDistanceCulling : MonoBehaviour
    {

        Transform targetPlayer;
        public GameObject targetObject;
        public float updateDelay = 1f;
        public float cullDistance = 150f;

        [Header("Debug")]
        [Space(2)]
        public float currentDistance;

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            targetPlayer = GameObject.FindWithTag("Player").transform;

            while (true)
            {
                currentDistance = Vector3.Distance(transform.position, targetPlayer.position);

                if (currentDistance > cullDistance)
                    targetObject.SetActive(false);
                else
                    targetObject.SetActive(true);

                yield return new WaitForSeconds(updateDelay);
            }
        }
    }
}