using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class WeatherSelect : MonoBehaviour
    {
        public GameObject snowParticle;
        public GameObject rainParticle;

        public string[] snowLevels;
        public string[] rainLevels;


        void Start()
        {
            snowParticle.SetActive(false);

            rainParticle.SetActive(false);

            foreach (string s in snowLevels)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == s)
                {
                    snowParticle.SetActive(true);
                    rainParticle.SetActive(false);
                }
            }
            foreach (string r in rainLevels)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == r)
                {
                    snowParticle.SetActive(false);
                    rainParticle.SetActive(true);
                }
            }
        }
    }
}