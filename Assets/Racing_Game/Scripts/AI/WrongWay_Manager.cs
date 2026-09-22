
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class WrongWay_Manager : MonoBehaviour
    {
        [Header("Block the road when in the wrong way")]
        [Space(14)]
        public GameObject[] blockers;
        [HideInInspector] public int currentBlocker;

        private void Start()
        {
            Transform[] temp = GetComponentsInChildren<Transform>();
            
            blockers = new GameObject[temp.Length-1];

            for (int t = 0; t < temp.Length-1; t++)
            {
                blockers[t] = temp[t+1].gameObject;
            }

            for (int a = 0; a < blockers.Length; a++)
            {
                blockers[a].GetComponent<WrongWay_Trigger>().blockerID = a;
                blockers[a].GetComponent<Renderer>().enabled = false;
            }
        }

        public void Select_Trigger(int id)
        {
            for (int a = 0; a < blockers.Length; a++)
            {
                blockers[a].GetComponent<BoxCollider>().isTrigger = true;
                blockers[a].GetComponent<WrongWay_Trigger>().showRenderer = false;
            }

            if (id == 0)
            {
                blockers[blockers.Length - 1].GetComponent<BoxCollider>().isTrigger = false;
                blockers[blockers.Length - 1].GetComponent<WrongWay_Trigger>().showRenderer = true;
            }
            else
            {
                blockers[id - 1].GetComponent<BoxCollider>().isTrigger = false;
                blockers[id - 1].GetComponent<WrongWay_Trigger>().showRenderer = true;

            }

            currentBlocker = id;
        }
    }
}