//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALIyerEdon;

namespace ALIyerEdon
{
    public class SettingsMenu : MonoBehaviour
    {
        // UI items in settings menu window
        public Slider AccelSensibility;
        public Text AccelSensibilityInfo;
        public Slider musicVolume;
        public Text musicVolumeInfo;
        public Dropdown controlType;

        public Dropdown QualityLevel;
        public Dropdown Grass;
        public Dropdown SSR;
        public Dropdown TerrainQuality;
        public Dropdown MotionBlur;
        public Dropdown DepthOfField;
        public Dropdown DisplayFPS;
        public Dropdown localPosition;

        // Start is called before the first frame update
        void Start()
        {
            // Load initilial settings            
            AccelSensibility.value = PlayerPrefs.GetFloat("accelSensibility");
            AccelSensibilityInfo.text = AccelSensibility.value.ToString();

            controlType.value = PlayerPrefs.GetInt("ControlType");

            QualityLevel.value = PlayerPrefs.GetInt("QualityLevel");
            TerrainQuality.value = PlayerPrefs.GetInt("TerrainQuality");
            MotionBlur.value = PlayerPrefs.GetInt("MotionBlur");
            DepthOfField.value = PlayerPrefs.GetInt("DepthOfField");
            DisplayFPS.value = PlayerPrefs.GetInt("DisplayFPS");
            localPosition.value = PlayerPrefs.GetInt("ShowLocalPosition");
            SSR.value = PlayerPrefs.GetInt("SSR");
            Grass.value = PlayerPrefs.GetInt("Grass");

            musicVolume.value = PlayerPrefs.GetFloat("Music");

        }
        // Accelerometer  Sensibility
        public void Accelometer_Sensibility()
        {
            PlayerPrefs.SetFloat("accelSensibility", AccelSensibility.value);
            AccelSensibilityInfo.text = AccelSensibility.value.ToString();
        }
        public void Music_Volume()
        {
            PlayerPrefs.SetFloat("Music", musicVolume.value);
            musicVolumeInfo.text = musicVolume.value.ToString();
            if (FindFirstObjectByType<Load_Settings>())
                FindFirstObjectByType<Load_Settings>().Update_MusicVolume(musicVolume.value);
        }
        // Control type : accelerometer , steering wheel , arrow keys
        public void Set_ControlType()
        {
            if (controlType.value == 0)
                PlayerPrefs.SetInt("ControlType", 0);
            if (controlType.value == 1)
                PlayerPrefs.SetInt("ControlType", 1);
            if (controlType.value == 2)
                PlayerPrefs.SetInt("ControlType", 2);
        }

        public void Set_TerrainQuality()
        {
            PlayerPrefs.SetInt("TerrainQuality", TerrainQuality.value);

            if (FindFirstObjectByType<Load_Settings>())
                FindFirstObjectByType<Load_Settings>().Update_TerrainQuality();
         }

        // Screen resolution quality : best for performance

        public void Select_QualityLevel()
        {
            PlayerPrefs.SetInt("QualityLevel", QualityLevel.value);

            if (FindFirstObjectByType<Load_Settings>())
            {
                FindFirstObjectByType<Load_Settings>().
                Set_QualityLevel(PlayerPrefs.GetInt("QualityLevel"));
            }
        }

        public void Set_MotionBlur()
        {
            PlayerPrefs.SetInt("MotionBlur", MotionBlur.value);

            if (PlayerPrefs.GetInt("MotionBlur") == 0)
                FindFirstObjectByType<Load_Settings>().Set_MotionBlur(false);
            if (PlayerPrefs.GetInt("MotionBlur") == 1)
                FindFirstObjectByType<Load_Settings>().Set_MotionBlur(true);
        }

        public void Set_DepthOfField()
        {
            PlayerPrefs.SetInt("DepthOfField", DepthOfField.value);

            if (PlayerPrefs.GetInt("DepthOfField") == 0)
                FindFirstObjectByType<Load_Settings>().Set_DepthOfField(false);
            if (PlayerPrefs.GetInt("DepthOfField") == 1)
                FindFirstObjectByType<Load_Settings>().Set_DepthOfField(true);
        }

        public void Set_DisplayFPS()
        {
            PlayerPrefs.SetInt("DisplayFPS", DisplayFPS.value);
            if (FindFirstObjectByType<Load_Settings>())
                FindFirstObjectByType<Load_Settings>().Set_DisplayFPS();
        }

        public void Set_LocalPosition()
        {
            PlayerPrefs.SetInt("ShowLocalPosition", localPosition.value);
            if(FindFirstObjectByType<Load_Settings>())
                FindFirstObjectByType<Load_Settings>().Set_LocalPosition();
        }

        public void Set_SSR()
        {
            PlayerPrefs.SetInt("SSR", SSR.value);
            FindFirstObjectByType<Load_Settings>().Set_SSR();
        }

        public void Set_GrassQuality()
        {
            PlayerPrefs.SetInt("Grass", Grass.value);
            if (FindFirstObjectByType<Load_Settings>())
                FindFirstObjectByType<Load_Settings>().Set_Grass();
        }
    }
}