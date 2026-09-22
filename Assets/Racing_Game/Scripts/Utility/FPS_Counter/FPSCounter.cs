using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALIyerEdon
{
    public class FPSCounter : MonoBehaviour
    {
        public float SmoothSpeed = 1f;
        float fps, smoothFps;
        public Text text;
        public GameObject panel;

        void Start()
        {
            Update_Settings();
        }

        public void Update_Settings()
        {
            if (PlayerPrefs.GetInt("DisplayFPS") == 1)
            {
                panel.SetActive(true);
                GetComponent<FPSCounter>().enabled = true;
            }
            else
            {
                panel.SetActive(false);
                GetComponent<FPSCounter>().enabled = false;
            }
        }

        void Update()
        {
            fps = 1f / Time.unscaledDeltaTime;
            if (Time.timeSinceLevelLoad < 0.1f) smoothFps = fps;
            smoothFps += (fps - smoothFps) * Mathf.Clamp(Time.unscaledDeltaTime * SmoothSpeed, 0, 1);
            text.text = ((int)smoothFps).ToString() + " fps";
        }
    }
}