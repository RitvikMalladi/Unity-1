//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Roof_Loader : MonoBehaviour
    {
        public int carID;
        public GameObject[] roofs;

        private void Start()
        {
            Select_Roof(PlayerPrefs.GetInt("Car" + carID.ToString() + "Roof"));
        }

        public void Select_Roof(int id)
        {
            for (int a = 0; a <= roofs.Length - 1; a++)
            {
                roofs[a].SetActive(false);
            }
            roofs[id].SetActive(true);

            PlayerPrefs.SetInt("Car" + carID.ToString() + "Roof", id);

        }
    }
}