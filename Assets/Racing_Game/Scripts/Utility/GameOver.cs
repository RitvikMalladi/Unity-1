using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALIyerEdon
{
    public class GameOver : MonoBehaviour
    {
        public static GameOver instance;
        public GameObject gameOverMenu,watchAD,resume;
        public GameObject mobileControls;

        public Slider healthBar;
        public Image fillArea;
        public Text Loading; 
        
        public GameObject[] raceUI;


        private void Start()
        {
            if (instance == null)
            {
                instance = this;
            }


            /*if (RewardedVids.Instance != null && RewardedVids.Instance._rewardedAd.CanShowAd())
            {
                Loading.text = "Watch ad to continue race";
                watchAD.gameObject.SetActive(true);
                resume.gameObject.SetActive(false);

            }
            else
            {
                Loading.text = "Ad is not available";
                watchAD.gameObject.SetActive(false);
                resume.gameObject.SetActive(true);
                watchAD.SetActive(false);
            }*/
        }
        public void Game_Over_Now()
        {
            FindFirstObjectByType<Pause_Menu>().canPause = false;

            gameOverMenu.SetActive(true);

            foreach(GameObject aaa in raceUI)
                    aaa.SetActive(false);

            mobileControls.SetActive(false);
            Time.timeScale = 0;
            AudioListener.volume = 0;
        }

        public void Watch_Ad()
        {
           /* RewardedVids.Instance.ShowRewardedADGameOver();
            watchAD.SetActive(false);
            resume.SetActive(true);
            FindFirstObjectByType<Car_Health>().Refill_Health();*/
        }

        public void Resume()
        {
            watchAD.SetActive(true);
            FindFirstObjectByType<Car_Health>().Refill_Health();

            FindFirstObjectByType<Pause_Menu>().canPause = true;
            AudioListener.volume = 1f;
            Time.timeScale = FindObjectOfType<Race_Manager>().timeScale;
            gameOverMenu.SetActive(false);

            foreach (GameObject aaa in raceUI)
                aaa.SetActive(true);

            mobileControls.SetActive(true);
        }

        public void Exit()
        {
            AudioListener.volume = 0;
            Time.timeScale = FindObjectOfType<Race_Manager>().timeScale;
            Loading.text = "Loading...";
            UnityEngine.SceneManagement.SceneManager.LoadScene("Garage");
        }

        public void Retry()
        {
            AudioListener.volume = 0;
            Time.timeScale = FindObjectOfType<Race_Manager>().timeScale;
            Loading.text = "Loading...";
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        }
    }
}