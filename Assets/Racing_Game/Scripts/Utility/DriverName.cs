using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALIyerEdon
{
    public class DriverName : MonoBehaviour
    {
        public InputField driverName;
        public string garageName = "Garage";
        public GameObject loading;
        public float delay = 3f;
        void Awake()
        {
            // PlayerPrefs.Init();



            if (PlayerPrefs.GetInt("FirstRun") == 3)
            {
                StartCoroutine(Start_Game_Delay());
            }
        }

        IEnumerator Start_Game_Delay()
        {
            if (PlayerPrefs.GetInt("FirstRun") != 3)
                PlayerPrefs.SetString("Player_Name", driverName.text);

            loading.SetActive(true);

            yield return new WaitForSeconds(delay);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(garageName);
        }

        public void Start_Game()
        {
            StartCoroutine(Start_Game_Delay());
        }
    }
}