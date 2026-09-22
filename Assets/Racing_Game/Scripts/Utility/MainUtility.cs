//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using ALIyerEdon;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace ALIyerEdon
{
	public class MainUtility : MonoBehaviour
	{
		public int targetFPS = 60;

		// Instantiate car at start
		public CarSelect carSelect;

		public AudioSource clickSound;

		public GameObject Loading, exitMenu;

		public int startingScore = 1400;

		public Text totalCoinsText;

        [Header("Camera Settings")]
        public Transform sportCamera;
        public Transform truckCamera;
        public Transform F1Camera;
        public Transform offroadCamera;

        [HideInInspector] public GameObject mainCamera;

        void Awake()
		{
			AudioListener.volume = 1f;

            Time.timeScale = 1f;

			PlayerPrefs.SetInt("Target FPS", targetFPS);

			Application.targetFrameRate = targetFPS;

            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            if (Gamepad.current != null)
                Gamepad.current.SetMotorSpeeds(0, 0);

            // Is game first run?   3 => true    0 => false
            if (PlayerPrefs.GetInt("FirstRun") != 3)
			{
				PlayerPrefs.SetInt("OriginalX", Screen.width);
				PlayerPrefs.SetInt("OriginalY", Screen.height);

				// Default difficulty level => 0 = simulation , 1 = Arcade
				PlayerPrefs.SetInt("Difficulty Level", 0);

				// Set default control type to the arrow keys
				//Arrow keys = 0 , Joystick = 1 , acceleration = 2
				PlayerPrefs.SetInt("ControlType", 0);
				
				// Default quality level is Low
				PlayerPrefs.SetInt("QualityLevel", 1);

				PlayerPrefs.SetFloat("accelSensibility", 100f);

				PlayerPrefs.SetFloat("SteeringWheelSens", 250f);

				// Set music ambient sound in settings false
				PlayerPrefs.SetFloat("Music", 0.7f);

				// Enable right position ui info display
				PlayerPrefs.SetInt("ShowPositionUI", 3);

				// Open the first 2 sport cars
				PlayerPrefs.SetInt("Car_Sport0", 3);
				PlayerPrefs.SetInt("Car1_Sport", 3);
				// Open the first  truck
				PlayerPrefs.SetInt("Car_Truck0", 3);
				// Open the first  offroad car
				PlayerPrefs.SetInt("Car_Offroad0", 3);
				// Open the first  F1 car
				PlayerPrefs.SetInt("Car_F10", 3);

				// Set default suspension value for each cars to the 0
				PlayerPrefs.SetFloat("Car0Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car1Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car2Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car3Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car4Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car5Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car6Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car7Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car8Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car9Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car10Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car11Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car12Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car13Suspension", -0.1f);
				PlayerPrefs.SetFloat("Car14Suspension", -0.1f);

				// Open the first level (level0)
				PlayerPrefs.SetInt("Level0", 3);
				PlayerPrefs.SetInt("Level7", 3);

				// Set none  awards for all levels
				PlayerPrefs.SetInt("Award_Level_0", 3);
				PlayerPrefs.SetInt("Award_Level_1", 3);
				PlayerPrefs.SetInt("Award_Level_2", 3);
				PlayerPrefs.SetInt("Award_Level_3", 3);
				PlayerPrefs.SetInt("Award_Level_4", 3);
				PlayerPrefs.SetInt("Award_Level_5", 3);
				PlayerPrefs.SetInt("Award_Level_6", 3);
				PlayerPrefs.SetInt("Award_Level_7", 3);
				PlayerPrefs.SetInt("Award_Level_8", 3);
				PlayerPrefs.SetInt("Award_Level_9", 3);
				PlayerPrefs.SetInt("Award_Level_10", 3);

				// Player first time starting the game score
				PlayerPrefs.SetInt("TotalScores", startingScore);

				PlayerPrefs.SetInt("Dynamic Camera", 1);

				PlayerPrefs.SetInt("Display_FPS", 0);
				PlayerPrefs.SetInt("targetFPS", 2);

                // Disable the first running the game settings
                PlayerPrefs.SetInt("FirstRun", 3);
			}

			if (totalCoinsText)
				totalCoinsText.text = PlayerPrefs.GetInt("TotalScores").ToString();
        }

        void Update()
        {
            #region Exit
            // Exit with back button
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonEast.wasPressedThisFrame)
                {
                    exitMenu.SetActive(!exitMenu.activeSelf);
                }
            }
            else
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    {
                        exitMenu.SetActive(!exitMenu.activeSelf);
                    }
                }
            }
            #endregion

            #region Delete_SavedData
            if (Keyboard.current != null)
            {
                if (Keyboard.current.hKey.wasPressedThisFrame)
                {
#if UNITY_EDITOR
                    PlayerPrefs.DeleteAll();
                    Debug.Log("PlayerPrefs.DeleteAll ();");
#endif
                }
            }
            #endregion

            #region Add_Coins
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
#if UNITY_EDITOR
                    PlayerPrefs.SetInt("TotalScores", PlayerPrefs.GetInt("TotalScores") + 30000);
                    Debug.Log("Added 30.000 coins");
#endif
                }
            }
            #endregion

        }

        public void Exit()
		{
			Application.Quit();
		}

		public void SetTrue(GameObject target)
		{
			target.SetActive(true);
		}

		public void SetFalse(GameObject target)
		{
			target.SetActive(false);
		}

		public void ToggleObject(GameObject target)
		{
			target.SetActive(!target.activeSelf);
		}

		public void LoadLevel(string name)
		{

			Loading.SetActive(true);
			SceneManager.LoadSceneAsync(name);
		}

		public void OpenURL(string val)
		{
			Application.OpenURL(val);
		}

		public void Click_Sound()
		{
			if (clickSound)
				clickSound.PlayOneShot(clickSound.clip);
		}

        public void Set_CameraPosition(int GarageCameraState)
        {
            if(GarageCameraState == 0)
                mainCamera.transform.position = sportCamera.position;
            if(GarageCameraState == 1)
                mainCamera.transform.position = truckCamera.position;
            if(GarageCameraState == 2)
                mainCamera.transform.position = F1Camera.position;
            if(GarageCameraState == 3)
                mainCamera.transform.position = offroadCamera.position;
        }

		public void GameMode_Select(int id)
		{
            PlayerPrefs.SetInt("GameMode",id);
        }
    }
}