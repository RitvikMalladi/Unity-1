//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using ALIyerEdon;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALIyerEdon
{
    public class Race_Manager : MonoBehaviour
    {
        private class Racer_Position
        {
            public int ID;
            public string Name;
            public float Position;
        }

        [Header("Options ____________________________________________________" +
            "____________________________________________________")]
        [Space(5)]
        public int levelID = 0;
        public string trackName = "Level 1";
        public bool startCutscene = false;

        [HideInInspector] public bool showLocalPosition = false;

        [Header("Race Start ____________________________________________________" +
            "____________________________________________________")]
        [Space(5)]
        public float timeScale = 1f;
        int counterNumbers = 3;
        public int totalLaps = 3;
        [HideInInspector] public GameObject startCounter;

        [Header("Minimap Icons ____________________________________________________" +
    "____________________________________________________")]
        public GameObject playerArrow;
        public GameObject racerArrow;
        public float yOffset = 10f;
        public float scale = 10f;

        [Header("User Interface ____________________________________________________" +
            "____________________________________________________")]
        [Space(5)]
        public GameObject startUI;
        public GameObject raceUI;
        public GameObject raceFinishUI;
        public GameObject positionUI;
        public GameOver gameoverMenu;
        public GameObject mobileControls;

        [Header("Player Info ____________________________________________________" +
            "____________________________________________________")]
        [Space(5)]
        public UnityEngine.UI.Text playerInfo;
        public UnityEngine.UI.Text lapInfo;
        public UnityEngine.UI.Text[] racerInfo;


        // Racers info class    
        List<Racer_Position> positions = new List<Racer_Position>();
        List<Racer_Position> sortedPositions = new List<Racer_Position>();

        [Header("Racing Elements ____________________________________________________" +
            "____________________________________________________")]
        [Space(5)]
        // Name of the each racer in order
        [HideInInspector] public string[] racerNames;

        // Player cars to spawn at the spawn points
        public GameObject[] playerPrefabs;
        GameObject playerPrefab;

        // Racer cars to spawn at the spawn points
        public GameObject[] racerPrefabs;

        [HideInInspector] public GameObject[] totalRacerPrefabs;

        // Spawn point for each racer in order
        public Transform[] spawnPositions;

        Car_Position[] carPositions;

        Car_Position playerPosition;

        [HideInInspector] public bool raceStarted;
        [HideInInspector] public EasyCarAudio[] racerAudios;

        bool dontGetKey = false;
        string playerName = "Player";
        bool canStart;


        IEnumerator Start()
        {
            AudioListener.volume = 1f;

            Time.timeScale = timeScale;
                       
            if (Gamepad.current != null)
                Gamepad.current.SetMotorSpeeds(0, 0);

            if (PlayerPrefs.GetInt("Target FPS") > 25)
            {
                Application.targetFrameRate =
                    PlayerPrefs.GetInt("Target FPS");
            }

            if (startUI)
                startUI.SetActive(false);
            if (raceUI)
                raceUI.SetActive(false);
            if (mobileControls)
                mobileControls.SetActive(false);


            FindFirstObjectByType<Start_Counter>().timeScale = timeScale;

            totalRacerPrefabs = new GameObject[racerPrefabs.Length + 1];

            // First racer is the player's prefab
            if (PlayerPrefs.GetInt("GameMode") == 0) // Sport
                totalRacerPrefabs[0] = playerPrefabs[PlayerPrefs.GetInt("CarID_Sport")];
            if (PlayerPrefs.GetInt("GameMode") == 1) // Truck
                totalRacerPrefabs[0] = playerPrefabs[PlayerPrefs.GetInt("CarID_Truck")];
            if (PlayerPrefs.GetInt("GameMode") == 2) // F1
                totalRacerPrefabs[0] = playerPrefabs[PlayerPrefs.GetInt("CarID_F1")];
            if (PlayerPrefs.GetInt("GameMode") == 3) // Offroad
                totalRacerPrefabs[0] = playerPrefabs[PlayerPrefs.GetInt("CarID_Offroad")];

            // Add racer prefabs to the total racer array
            for (int ttt = 1; ttt < totalRacerPrefabs.Length; ttt++)
            {
                totalRacerPrefabs[ttt] = racerPrefabs[ttt - 1];
            }

            // Assign racers id
            for (int ddd = 1; ddd < totalRacerPrefabs.Length; ddd++)
            {
                totalRacerPrefabs[ddd].GetComponent<Car_Position>().RacerID = ddd;
            }

            // Initial info
            carPositions = new Car_Position[totalRacerPrefabs.Length];
            racerNames = new string[totalRacerPrefabs.Length];

            // Instantiate racers and prefabs
            for (int i = 0; i < totalRacerPrefabs.Length; i++)
            {
                GameObject racer = Instantiate(totalRacerPrefabs[i], spawnPositions[i].position,
                     spawnPositions[i].rotation) as GameObject;

                // Instantiate minimap arrow icon for player
                if (i == 0)
                {
                    GameObject minimapArrow = Instantiate(playerArrow, new Vector3(racer.transform.position.x, racer.transform.position.y + yOffset, racer.transform.position.z),
                       Quaternion.identity) as GameObject;

                    minimapArrow.transform.parent = racer.transform;

                    minimapArrow.transform.localScale = new Vector3(scale, scale, scale);

                    minimapArrow.transform.localRotation = new Quaternion(1f, 0, 0, 1f);

                    minimapArrow.name = "Player Minimap Arrow";
                }
                else // Instantiate minimap arrow icon for racers                
                {
                    GameObject minimapArrow = Instantiate(racerArrow, new Vector3(racer.transform.position.x, racer.transform.position.y + yOffset, racer.transform.position.z),
                       Quaternion.identity) as GameObject;

                    minimapArrow.transform.parent = racer.transform;

                    minimapArrow.transform.localScale = new Vector3(scale, scale, scale);

                    minimapArrow.transform.localRotation = new Quaternion(1f, 0, 0, 1f);

                    minimapArrow.name = "Racer Minimap Arrow";
                }

                // Show or hide car position on the top of the car
                racer.GetComponent<Car_Position>().displayPosition = false;

                racer.GetComponent<Car_AI>().raceStarted = false;

                racer.GetComponent<Car_Position>().RacerID = i;

                carPositions[i] = racer.GetComponent<Car_Position>();

                racerNames[i] = totalRacerPrefabs[i].GetComponent<Car_Position>().RacerName;

                // Add the racers position class to the list
                Racer_Position newRacePos = new Racer_Position() { Name = racerNames[i], Position = 0 };
                positions.Add(newRacePos);
                sortedPositions.Add(newRacePos);

            }

            playerName = GameObject.FindGameObjectWithTag("Player").GetComponent
                <Car_Position>().RacerName;
            //_________________________________

            // Find car position component of the player car to update UI text info (position + lap)
            playerPosition = GameObject.FindGameObjectWithTag("Player").
                GetComponent<Car_Position>();

            startCounter = FindFirstObjectByType<Start_Counter>().gameObject;

            GameObject.FindGameObjectWithTag("Player")
                .GetComponent<EasyCarController>().Clutch = true;

            FindFirstObjectByType<InputSystem>().canControl = false;

            if (!startCutscene)
                Show_StartUI();

            yield return new WaitForEndOfFrame();

            // Disable local position display on top of the cars before race start
            foreach (Car_Position carPos in FindObjectsOfType<Car_Position>())
            {
                carPos.GetComponent<Car_Position>().displayPosition = false;
            }

            racerAudios = FindObjectsOfType<EasyCarAudio>();

            for (int i = 0; i < racerAudios.Length; i++)
            {
                racerAudios[i].engineVolume = 0;
                racerAudios[i].engineSource.volume = 0;
            }
        }
        public void Show_StartUI()
        {
            StartCoroutine(StartUI_Delay());
        }
        IEnumerator StartUI_Delay()
        {
            if (startCutscene)
                yield return new WaitForSeconds(0.001f);
            else
                yield return new WaitForSeconds(1f * timeScale);

            startUI.SetActive(true);
            canStart = true;

            Update_Positions_Display();
        }
        public void Update_Positions_Display()
        {
            for (int a = 0; a < FindObjectOfType<Start_Finish_UI>().positions.Length; a++)
            {
                try
                {
                    FindObjectOfType<Start_Finish_UI>().driversName[a].text =
                       sortedPositions[a].Name.ToString();
                }
                catch { }
            }

            startUI.GetComponent<Start_Finish_UI>().totalScores.text =
                "Total Coins : " +
                PlayerPrefs.GetInt("TotalScores").ToString();
        }
        public void StartRace_Button()
        {
            if (!dontGetKey)
            {
                raceStarted = true;

                foreach (EasyCarAudio carAudio in FindObjectsOfType<EasyCarAudio>())
                {
                    carAudio.engineVolume = carAudio.engineStartVolume;
                    carAudio.engineSource.volume = carAudio.engineStartVolume;
                }

                if (FindFirstObjectByType<Start_Cutscene>())
                    FindFirstObjectByType<Start_Cutscene>().Start_Race();

                StartRace();
                dontGetKey = true;
            }
        }
        public void StartRace()
        {
            StartCoroutine(StartRaceDelay());
        }
        IEnumerator StartRaceDelay()
        {
            raceStarted = true;

            for (int i = 0; i < racerAudios.Length; i++)
            {
                racerAudios[i].engineVolume = racerAudios[i].engineStartVolume;
                racerAudios[i].engineSource.volume = racerAudios[i].engineStartVolume;
            }

            FindObjectOfType<InputSystem>().canControl = true;

            if (startUI)
                startUI.SetActive(false);
            if (raceUI)
                raceUI.SetActive(true);

            if (GetComponentInChildren<InputSystem>().controlType == InputType.Mobile)
                FindObjectOfType<Race_Manager>().mobileControls.SetActive(true);
            else
                FindObjectOfType<Race_Manager>().mobileControls.SetActive(false);

            // Enable or disable right side position ui
            if (PlayerPrefs.GetInt("ShowPositionUI") == 1)
                positionUI.SetActive(true);
            else
                positionUI.SetActive(false);

            if (mobileControls)
            {
                if (FindObjectOfType<InputSystem>().controlType == InputType.Mobile)
                    mobileControls.SetActive(true);

            }

            yield return new WaitForSeconds(1);

            FindObjectOfType<Start_Counter>().StartCounter();

            yield return new WaitForSeconds((counterNumbers) * timeScale);

            raceStarted = true;

            foreach (Car_AI carAI in FindObjectsOfType<Car_AI>())
            {
                carAI.raceStarted = true;
                carAI.gameObject.GetComponent<EasyCarController>()
                    .Clutch = false;
            }

            GameObject.FindGameObjectWithTag("Player")
                .GetComponent<EasyCarController>().Clutch = false;

            GameObject.FindGameObjectWithTag("Player")
                                .GetComponent<EasyCarAudio>().stopRandom = true;

            if (GameObject.FindGameObjectWithTag("Player")
                .GetComponent<EasyCarController>().throttleInput > 0.6f)
            {
                GameObject.FindGameObjectWithTag("Player")
                                .GetComponent<EasyCarAudio>().Play_StartSkid_Sound();
            }

            foreach (GameObject racerCars in GameObject.FindGameObjectsWithTag("Racer"))
                racerCars.GetComponent<EasyCarAudio>().Play_StartSkid_Sound();

            // User can display the pause menu after race start
            FindObjectOfType<Pause_Menu>().raceIsStarted = true;
            FindObjectOfType<Nitro_Feature>().raceIsStarted = true;

            foreach (Racer_Nitro rn in GameObject.FindObjectsOfType<Racer_Nitro>())
                rn.raceIsStarted = true;

            yield return new WaitForSeconds(
                GameObject.FindGameObjectWithTag("Player")
                .GetComponent<EasyCarController>().startDuration);

            GameObject.FindGameObjectWithTag("Player")
                .GetComponent<EasyCarController>().shaking = false;
            yield return new WaitForSeconds(1f);

            // Racers can check reverse mode after 2 seconds from the race start 
            foreach (Car_AI carAI in FindObjectsOfType<Car_AI>())
                 carAI.canReverseCheck = true;

         }
         public void Finish_Race()
         {
             GameObject.FindGameObjectWithTag("Player").GetComponent<Car_AI>().enabled = true;
             FindObjectOfType<InputSystem>().canControl = false;

             raceFinishUI.SetActive(true);

             FindObjectOfType<Start_Finish_UI>().finishRaceMenu.SetActive(true);
             FindObjectOfType<Start_Finish_UI>().startButton.SetActive(false);
             FindObjectOfType<Start_Finish_UI>().raceUI.SetActive(false);

             mobileControls.SetActive(false);
             Update_Positions_Display();

             // Update award icons (gold , bronze silver) at race finish menu
             if (sortedPositions[0].Name == playerName)
                 FindObjectOfType<Start_Finish_UI>().Update_Award(0, levelID);
             else if (sortedPositions[1].Name == playerName)
                 FindObjectOfType<Start_Finish_UI>().Update_Award(1, levelID);
             else if (sortedPositions[2].Name == playerName)
                 FindObjectOfType<Start_Finish_UI>().Update_Award(2, levelID);
             else
                 FindObjectOfType<Start_Finish_UI>().Update_Award(3, levelID);

             startUI.GetComponent<Start_Finish_UI>().totalScores.text =
                 "Total Coins : " +
                 PlayerPrefs.GetInt("TotalScores").ToString();

         }

        void Update()
         {



             // Update ui info (player position + current lap   )
             if (playerInfo)
                 playerInfo.text = "Pos : " + (playerPosition.currentPosition + 1).ToString()
                 + " / " + carPositions.Length.ToString();
             else
                 Debug.Log("Please add -Position Info- text object in the -Race Manager- component");

             if (playerPosition.currentLap > 0)
             {
                 if (lapInfo)
                     lapInfo.text = "Lap : " + playerPosition.currentLap.ToString()
                      + " / " + totalLaps.ToString();
                 else
                     Debug.Log("Please add -Lap Info- text object in the -Race Manager- component");
             }
             else
             {
                 if (lapInfo)
                     lapInfo.text = "Lap : 1" + " / " + totalLaps.ToString();
                 else
                     Debug.Log("Please add -Lap Info- text object in the -Race Manager- component");
             }
             //_________________________________

             // Positions info
             for (int pos = 0; pos < racerInfo.Length; pos++)
             {
                 try
                 {
                     if (racerInfo[pos])
                         racerInfo[pos].text = "   " + (pos + 1).ToString() + "   |   " + sortedPositions[pos].Name.ToString();
                 }
                 catch { }
             }
         }

         // List and sort car positions based on the istance form the checkpoints
         public void Update_Position(int racerID, string totalPoints)
         {
             // List and sort racer positions based on the distance from the checkpoint
             positions[racerID].Position = float.Parse(totalPoints);
             sortedPositions = positions.OrderBy(number => number.Position).ToList();

             sortedPositions.Reverse();
             //_________________________________

             for (int b = 0; b < sortedPositions.Count; b++)
             {
                 if (playerPosition.RacerName == sortedPositions[b].Name)
                 {
                     playerPosition.currentPosition = b;
                 }
             }

             // Enable current position icon (on the top of the car) for each racer
             for (int a = 0; a < carPositions.Length; a++)
             {
                 for (int c = 0; c < carPositions.Length; c++)
                 {
                     if (carPositions[a].RacerName == sortedPositions[c].Name)
                     {
                         carPositions[a].Update_Position(c);
                     }
                 }/*
                 if (carPositions[a].RacerName == sortedPositions[0].Name)
             {
                 carPositions[a].Update_Position(0);
              }

             if (carPositions[a].RacerName == sortedPositions[1].Name)
             { 
                 carPositions[a].Update_Position(1);
              }
             if (carPositions[a].RacerName == sortedPositions[2].Name)
             { 
                 carPositions[a].Update_Position(2);
             }
             if (carPositions[a].RacerName == sortedPositions[3].Name)
             {
                 carPositions[a].Update_Position(3);
             }
             if (carPositions[a].RacerName == sortedPositions[4].Name)
             {
                 carPositions[a].Update_Position(4);
             }*/
        }

        //_________________________________

    }

    }
}