using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ALIyerEdon
{
    public class Crack_Trigger : MonoBehaviour
    {
        public string playerTag = "Player";
        public string racerTag = "Racer";
        public float destroyDelay = 5f;
        bool startDestroy = false;

        public GameObject[] crackRigids;


        // Update is called once per frame
        void OnTriggerEnter(Collider other)
        {
            if(other.transform.tag == playerTag ||
                other.transform.tag == racerTag)
            {
                for (int a = 0; a < crackRigids.Length; a++)
                {
                    if (!crackRigids[a].GetComponent<Rigidbody>())
                        crackRigids[a].AddComponent<Rigidbody>();
                }

                if (!startDestroy)
                    StartCoroutine(Destroy_Delay());
            }
        }

        IEnumerator Destroy_Delay()
        {
            startDestroy = true;

            GetComponent<AudioSource>().Play();

            yield return new WaitForSeconds(destroyDelay);
            Destroy(gameObject);
        }
    }
}