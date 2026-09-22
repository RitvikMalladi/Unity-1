//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Car_Ring : MonoBehaviour
    {
        public void Select_Ring(int id)
        {

            GameObject.FindGameObjectWithTag("Player").
                GetComponentInChildren<Ring_Loader>().Select_Ring(id);

        }
    }
}