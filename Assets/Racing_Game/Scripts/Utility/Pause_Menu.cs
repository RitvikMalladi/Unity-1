//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace ALIyerEdon
{
    public class Pause_Menu : MonoBehaviour
    {

        public GameObject pauseMenu;
        public GameObject raceUI;
        public GameObject mobileControls;

        public Text Loading;

        public string GarageScene = "Garage";

        [HideInInspector] public bool raceIsStarted = false;
        [HideInInspector] public bool canPause = true;

        public void Pause()
        {
            if (raceIsStarted)
            {
                if (canPause)
                {
                    if (Gamepad.current != null)
                        Gamepad.current.SetMotorSpeeds(0, 0);

                    AudioListener.volume = 0;
                    Time.timeScale = 0;
                    pauseMenu.SetActive(true);
                    raceUI.SetActive(false);
                    mobileControls.SetActive(false);
                }
            }
        }

        public void Resume()
        {
            AudioListener.volume = 1f;
            Time.timeScale = FindObjectOfType<Race_Manager>().timeScale;
            pauseMenu.SetActive(false);
            raceUI.SetActive(true);

            if (FindFirstObjectByType<InputSystem>())
            {
                if (FindFirstObjectByType<InputSystem>().controlType == InputType.Mobile)
                    mobileControls.SetActive(true);
            }        
        }

        public void Restart()
        {
            AudioListener.volume = 0;
            Time.timeScale = FindObjectOfType<Race_Manager>().timeScale;
            Loading.text = "Loading...";
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        public void Exit()
        {
            AudioListener.volume = 0;
            Time.timeScale = FindObjectOfType<Race_Manager>().timeScale;
            Loading.text = "Loading...";
            UnityEngine.SceneManagement.SceneManager.LoadScene(GarageScene);
        }

        public void Enable_Object(GameObject target)
        {
            target.SetActive(true);
        }

        public void Disable_Object(GameObject target)
        {
            target.SetActive(false);
        }
    }
}