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
    public class Start_Counter : MonoBehaviour
    {
        public float fadeSpeed = 0.5f;

        public GameObject[] counters;

        int currentCount;
        bool startCounter;

        AudioSource counterAudio;
        [HideInInspector] public float timeScale;

        public void StartCounter()
        {
            StartCoroutine(StartRaceCounter());
        }

        IEnumerator StartRaceCounter()
        {
            startCounter = true;
            
            if (GetComponent<AudioSource>())
                counterAudio = GetComponent<AudioSource>();

            for (int b = 0; b < counters.Length; b++)
                counters[b].SetActive(false);

            for (int a = 0; a< counters.Length; a++)
            {
                currentCount = a;

                for (int c = 0; c < counters.Length; c++)
                    counters[c].SetActive(false);

                counters[a].SetActive(true);

                counterAudio.Play();

                yield return new WaitForSeconds(timeScale);
            }

            for (int d = 0; d < counters.Length; d++)
                counters[d].SetActive(false);

            // Enable local position display on top of the cars
            foreach (Car_Position carPos in FindObjectsOfType<Car_Position>())
            {
                // Show or hide car position on the top of the car
                carPos.GetComponent<Car_Position>().displayPosition =
                    FindFirstObjectByType<Race_Manager>().showLocalPosition;
            }

            yield return new WaitForSeconds(FindObjectOfType<EasyCarAudio>().startSkidDuration);

            foreach (EasyCarAudio carAudio in FindObjectsOfType<EasyCarAudio>())
            {
                carAudio.raceIsStarted = true;
            }

            // Disable counter completely
            gameObject.SetActive(false);

        }

        void Update()
        {
            if (!startCounter)
                return;

            if (counters[currentCount].GetComponent<RectTransform>().localScale.x > 0)
                counters[currentCount].GetComponent<RectTransform>().localScale -= new Vector3(Time.deltaTime * fadeSpeed,
                    Time.deltaTime * fadeSpeed, Time.deltaTime * fadeSpeed); ;
        }
    }
}