//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Car_Neon : MonoBehaviour
    {
        public void Select_Neon(int id)
        {

            GameObject.FindGameObjectWithTag("Player").
                GetComponentInChildren<Neon_Loader>().Select_Neon(id);

        }
    }
}