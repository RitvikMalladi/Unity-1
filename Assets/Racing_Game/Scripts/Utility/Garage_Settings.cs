//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALIyerEdon;

namespace ALIyerEdon
{
    public class Garage_Settings : MonoBehaviour
    {
        public float fieldOfView = 47f;

        public GameObject[] disableObjects;

        bool inGarage;
        // Start is called before the first frame update
        void Awake()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Garage"))
            {
                if (FindFirstObjectByType<CarSelect>())
                    FindFirstObjectByType<CarSelect>().carName.text = GetComponent<Car_Position>().RacerName;

                inGarage = true;
                // Disable particle effects in the garage scene
                foreach (GameObject g in disableObjects)
                    g.SetActive(false);

                GetComponent<Car_Position>().enabled = false;

                GetComponent<Car_Health>().enabled = false;

                GetComponent<Car_AI>().enabled = false;
                
                GetComponent<EasyCarAudio>().Stop_Effects();
                GetComponent<EasyCarAudio>().enabled = false;

                if (GetComponent<Random_Car_Speed>())
                    GetComponent<Random_Car_Speed>().enabled = false;

                GetComponent<EasyCarController>().Clutch = true;
            }
        }

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            if(inGarage)
                GetComponent<EasyCarController>().defaultFOV = fieldOfView;
        }
    }
}