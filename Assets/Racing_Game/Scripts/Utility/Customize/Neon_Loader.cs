//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Neon_Loader : MonoBehaviour
    {
        public int carID;
        public Color[] neons;

        private void Start()
        {
            Select_Neon(PlayerPrefs.GetInt("Car" + carID.ToString() + "Neon"));
        }

        public void Select_Neon(int id)
        {
            GetComponent<Light>().color = neons[id];
            PlayerPrefs.SetInt("Car" + carID.ToString() + "Neon", id);

        }
    }
}