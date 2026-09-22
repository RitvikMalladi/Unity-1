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
    public class Start_Finish_UI : MonoBehaviour
    {
        // Display race finish menu
        [Header("Menu ____________________________________________________" +
            "____________________________________________________")]
        [Space(5)]
        public GameObject finishRaceMenu;
        public GameObject raceUI;
        public GameObject startButton;
        public UnityEngine.UI.Text totalScores;

        // Award icons 
        [Header("Awards ____________________________________________________" +
            "____________________________________________________")]
        [Space(5)]
        public GameObject goldIcon;
        public GameObject bronzeIcon;
        public GameObject silverIcon;
        public GameObject noAward;

        // Give scores
        [Header("Scores ____________________________________________________" +
            "____________________________________________________")]
        [Space(5)]
        public int goldAward = 3000;
        public int bronzeAward = 2000;
        public int silverAward = 1000;

        // Update player's position
        [Header("Positions ____________________________________________________" +
            "____________________________________________________")]
        [Space(5)] public UnityEngine.UI.Text[] positions;
        public UnityEngine.UI.Text[] driversName;

        // Update award menu from "Race_Manager" component at the race finish
        public void Update_Award(int position, int level_ID)
        {
            //  Save current level award to display in the main menu (garage scene)
            if (PlayerPrefs.GetInt("Award_Level_" + level_ID.ToString()) > position)
                PlayerPrefs.SetInt("Award_Level_" + level_ID.ToString(), position);

            // Enable disable award icons (gold, bronze, silver)
            if (position == 0)
            {
                goldIcon.SetActive(true);
                bronzeIcon.SetActive(false);
                silverIcon.SetActive(false);
                noAward.SetActive(false);
                PlayerPrefs.SetInt("TotalScores", PlayerPrefs.GetInt("TotalScores") + goldAward);
            }
            if (position == 1)
            {
                goldIcon.SetActive(false);
                bronzeIcon.SetActive(true);
                silverIcon.SetActive(false);
                noAward.SetActive(false);
                PlayerPrefs.SetInt("TotalScores", PlayerPrefs.GetInt("TotalScores") + bronzeAward);
            }
            if (position == 2)
            {
                goldIcon.SetActive(false);
                bronzeIcon.SetActive(false);
                silverIcon.SetActive(true);
                noAward.SetActive(false);
                PlayerPrefs.SetInt("TotalScores", PlayerPrefs.GetInt("TotalScores") + silverAward);
            }
            if (position == 3)
            {
                goldIcon.SetActive(false);
                bronzeIcon.SetActive(false);
                silverIcon.SetActive(false);
                noAward.SetActive(true);
            }
        }
    }


}