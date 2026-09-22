//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Spoiler_Loader : MonoBehaviour
    {
        public int carID;
        public GameObject[] spoilers;

        private void Start()
        {
            Select_Spoiler(PlayerPrefs.GetInt("Car" + carID.ToString() + "Spoiler"));
        }

        public void Select_Spoiler(int id)
        {
            for (int a = 0; a <= spoilers.Length - 1; a++)
            {
                spoilers[a].SetActive(false);
            }
            spoilers[id].SetActive(true);

            PlayerPrefs.SetInt("Car" + carID.ToString() + "Spoiler", id);

        }
    }
}