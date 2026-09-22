//______________________________________________
//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;

namespace ALIyerEdon
{

    public class GameMode_Loader : MonoBehaviour
    {

        public GameObject Sport_Manager;
        public GameObject Truck_Manager;
        public GameObject F1_Manager;

        void Awake()
        {
            if (PlayerPrefs.GetInt("GameMode") == 0) // Sport
            {
                Sport_Manager.SetActive(true);
                Truck_Manager.SetActive(false);
                F1_Manager.SetActive(false);
            }
            if (PlayerPrefs.GetInt("GameMode") == 1) // Truck
            {
                Sport_Manager.SetActive(false);
                Truck_Manager.SetActive(true);
                F1_Manager.SetActive(false);
            }
            if (PlayerPrefs.GetInt("GameMode") == 2) // F1
            {
                Sport_Manager.SetActive(false);
                Truck_Manager.SetActive(false);
                F1_Manager.SetActive(true);
            }
        }
    }

}
