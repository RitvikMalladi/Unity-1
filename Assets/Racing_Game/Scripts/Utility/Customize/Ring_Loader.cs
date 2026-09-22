//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Ring_Loader : MonoBehaviour
    {
        public int carID;
        public GameObject[] rings_1;
        public GameObject[] rings_2;
        public GameObject[] rings_3;
        public GameObject[] rings_4;

        private void Start()
        {
            Select_Ring(PlayerPrefs.GetInt("Car" + carID.ToString() + "Ring"));
        }

        public void Select_Ring(int id)
        {
           /* for (int a = 0; a <= rings_1.Length - 1; a++)
            {
                rings_1[a].SetActive(false);
                rings_2[a].SetActive(false);
                rings_3[a].SetActive(false);
                rings_4[a].SetActive(false);
            }
            rings_1[id].SetActive(true);
            rings_2[id].SetActive(true);
            rings_3[id].SetActive(true);
            rings_4[id].SetActive(true);

            PlayerPrefs.SetInt("Car" + carID.ToString() + "Ring", id);
           */
        }
    }
}