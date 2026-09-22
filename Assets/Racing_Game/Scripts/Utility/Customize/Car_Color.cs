//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Car_Color : MonoBehaviour
    {
        public int carID;

        public bool randomColor;

        public int materialIndex;

        int colorID;

        public Color32[] colors;


        // Start is called before the first frame update
        void Start()
        {
            colorID = PlayerPrefs.GetInt("CarColor" + carID.ToString());

            GetComponent<MeshRenderer>().sharedMaterials[materialIndex]
                .color = colors[colorID];
            /*
            if(randomColor)
            {
                int random = (int)(Random.Range(0, 6f));

                GetComponent<MeshRenderer>().sharedMaterials[materialIndex]
                            .color = colors[random];
            }*/
        }

        public void Change_Color(int colorID)
        {
            GetComponent<MeshRenderer>().sharedMaterials[materialIndex]
                .color = colors[colorID];

            PlayerPrefs.SetInt("CarColor" + carID.ToString(), colorID);
        }
    }
}