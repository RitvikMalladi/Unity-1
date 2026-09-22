//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Custume_Item_Loader : MonoBehaviour
    {
        public int carID;
        public string itemName = "Item 0";
        public GameObject[] items;

        private void Start()
        {
            Select_Item(PlayerPrefs.GetInt("Car" + carID.ToString() + itemName));
        }

        public void Select_Item(int id)
        {
            for (int a = 0; a <= items.Length - 1; a++)
            {
                items[a].SetActive(false);
            }
            items[id].SetActive(true);

            PlayerPrefs.SetInt("Car" + carID.ToString() + itemName, id);

        }
    }
}