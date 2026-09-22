
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class WrongWay_Trigger : MonoBehaviour
    {
        public int blockerID;
        [HideInInspector] public bool showRenderer;
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            // Ignore road blocker for racers , enable only for player
            GameObject[] racers = GameObject.FindGameObjectsWithTag("Racer");

            for (int a = 0; a < racers.Length; a++)
            {
                Collider[] col = racers[a].GetComponentsInChildren<Collider>();

                for (int c = 0; c < col.Length; c++)
                    Physics.IgnoreCollision(col[c], GetComponent<Collider>());
            }

            // Check to enable blocker mesh renderer(red material) when the player is close to the blocker
            while (true)
            {
                // Check every seconds
                yield return new WaitForSeconds(1f);

                if (showRenderer)
                {
                    if (Vector3.Distance(transform.position,
                        GameObject.FindGameObjectWithTag("Player").transform.position) < 70f)
                        GetComponent<MeshRenderer>().enabled = true;
                    else
                        GetComponent<MeshRenderer>().enabled = false;
                }
                else
                    GetComponent<MeshRenderer>().enabled = false;
            }
        }

        void OnTriggerExit(Collider col)
        {
            if (col.tag == "Player")
                GetComponentInParent<WrongWay_Manager>().Select_Trigger(blockerID);
        }
    }
}