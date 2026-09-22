//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ALIyerEdon;

namespace ALIyerEdon
{
    public enum CarSelectMode
    {
        Sport, Truck, F1, Offroad
    }

    public class CarSelect : MonoBehaviour
	{
		// SpawnPoint
		public Transform spawnPoint;

        public CarSelectMode carSelectMode;

        // Cars prefabs array
        public GameObject[] cars;

		// Each Car value
		public int[] carPrices;

		// Lock Icon,Shop window,Buy button
		public GameObject lockIcon, buyButton, Price, noEnoughScores;
		public GameObject purchaseUI;

		// Display total scores
		public Text TotalScores;
		public Text carName;

		// Selected car ID
		public int ID;

		//TotalScore text, car value text
		public Text carPriceText;

		public LevelSelect levelSelect;
		public ColorPicker colorPicker;

		// SetActive(true) loading window before start loading level
		public GameObject Loading;

        bool isStartLoaded;

        public void Start_Load()
        {
            // Read lastest car selected ID before
            if (carSelectMode == CarSelectMode.Sport)
                ID = PlayerPrefs.GetInt("CarID_Sport");
            if (carSelectMode == CarSelectMode.Truck)
                ID = PlayerPrefs.GetInt("CarID_Truck");
            if (carSelectMode == CarSelectMode.F1)
                ID = PlayerPrefs.GetInt("CarID_F1");
            if (carSelectMode == CarSelectMode.Offroad)
                ID = PlayerPrefs.GetInt("CarID_Offroad");

            // Update total scores display
            TotalScores.text = PlayerPrefs.GetInt("TotalScores").ToString();

            // Distroy all players car before instantiate the new one
            Destroy(GameObject.FindGameObjectWithTag("Player"));

            // Instantiate last selected car by saved ID
            GameObject car = Instantiate(cars[ID], spawnPoint.position, spawnPoint.rotation);

            /*if (colorPicker != null)
            {
                colorPicker.currentCarID = ID;
                colorPicker.Load_Color();
            }*/

            // Enable or disable buy , lock and car price displays
            if (carSelectMode == CarSelectMode.Sport)
            {
                if (PlayerPrefs.GetInt("Car_Sport" + ID.ToString()) == 3)
                {
                    lockIcon.SetActive(false);
                    buyButton.SetActive(false);
                    Price.SetActive(false);
                }
                else
                {
                    lockIcon.SetActive(true);
                    buyButton.SetActive(true);
                    Price.SetActive(true);
                }
            }
            if (carSelectMode == CarSelectMode.Truck)
            {
                if (PlayerPrefs.GetInt("Car_Truck" + ID.ToString()) == 3)
                {
                    lockIcon.SetActive(false);
                    buyButton.SetActive(false);
                    Price.SetActive(false);
                }
                else
                {
                    lockIcon.SetActive(true);
                    buyButton.SetActive(true);
                    Price.SetActive(true);
                }
            }
            if (carSelectMode == CarSelectMode.F1)
            {
                if (PlayerPrefs.GetInt("Car_F1" + ID.ToString()) == 3)
                {
                    lockIcon.SetActive(false);
                    buyButton.SetActive(false);
                    Price.SetActive(false);
                }
                else
                {
                    lockIcon.SetActive(true);
                    buyButton.SetActive(true);
                    Price.SetActive(true);
                }
            }
            if (carSelectMode == CarSelectMode.Offroad)
            {
                if (PlayerPrefs.GetInt("Car_Offroad" + ID.ToString()) == 3)
                {
                    lockIcon.SetActive(false);
                    buyButton.SetActive(false);
                    Price.SetActive(false);
                }
                else
                {
                    lockIcon.SetActive(true);
                    buyButton.SetActive(true);
                    Price.SetActive(true);
                }
            }

            // Update current car value text
            carPriceText.text = carPrices[ID].ToString() + " $";

        }

        bool nextClick, prevClick;
		// Public function for NextCar select button in menu
		public void NextCar()
		{
			if (!prevClick)
			{

				nextClick = true;

				if (ID < cars.Length - 1)
					ID++;

                if (carSelectMode == CarSelectMode.Sport)
                    PlayerPrefs.SetInt("CarID_Sport", ID);
                if (carSelectMode == CarSelectMode.Truck)
                    PlayerPrefs.SetInt("CarID_Truck", ID);
                if (carSelectMode == CarSelectMode.F1)
                    PlayerPrefs.SetInt("CarID_F1", ID);
                if (carSelectMode == CarSelectMode.Offroad)
                    PlayerPrefs.SetInt("CarID_Offroad", ID);

                // Distroy all players car before instantiate the new one
                Destroy(GameObject.FindGameObjectWithTag("Player"));

                if (carSelectMode == CarSelectMode.Sport)
                    ID = PlayerPrefs.GetInt("CarID_Sport");
                if (carSelectMode == CarSelectMode.Truck)
                    ID = PlayerPrefs.GetInt("CarID_Truck");
                if (carSelectMode == CarSelectMode.F1)
                    ID = PlayerPrefs.GetInt("CarID_F1");
                if (carSelectMode == CarSelectMode.Offroad)
                    ID = PlayerPrefs.GetInt("CarID_Offroad");

                // Instantiate last selected car by saved ID
                GameObject car = Instantiate(cars[ID], spawnPoint.position, spawnPoint.rotation);

                /*if (colorPicker != null)
                {
                    colorPicker.currentCarID = ID;
                    colorPicker.Load_Color();
                }*/

                if (carSelectMode == CarSelectMode.Sport)
                {
                    if (PlayerPrefs.GetInt("Car_Sport" + ID.ToString()) == 3)
                    {
                        lockIcon.SetActive(false);
                        buyButton.SetActive(false);
                        Price.SetActive(false);
                    }
                    else
                    {
                        lockIcon.SetActive(true);
                        buyButton.SetActive(true);
                        Price.SetActive(true);
                    }
                }
                if (carSelectMode == CarSelectMode.Truck)
                {
                    if (PlayerPrefs.GetInt("Car_Truck" + ID.ToString()) == 3)
                    {
                        lockIcon.SetActive(false);
                        buyButton.SetActive(false);
                        Price.SetActive(false);
                    }
                    else
                    {
                        lockIcon.SetActive(true);
                        buyButton.SetActive(true);
                        Price.SetActive(true);
                    }
                }
                if (carSelectMode == CarSelectMode.F1)
                {
                    if (PlayerPrefs.GetInt("Car_F1" + ID.ToString()) == 3)
                    {
                        lockIcon.SetActive(false);
                        buyButton.SetActive(false);
                        Price.SetActive(false);
                    }
                    else
                    {
                        lockIcon.SetActive(true);
                        buyButton.SetActive(true);
                        Price.SetActive(true);
                    }
                }
                if (carSelectMode == CarSelectMode.Offroad)
                {
                    if (PlayerPrefs.GetInt("Car_Offroad" + ID.ToString()) == 3)
                    {
                        lockIcon.SetActive(false);
                        buyButton.SetActive(false);
                        Price.SetActive(false);
                    }
                    else
                    {
                        lockIcon.SetActive(true);
                        buyButton.SetActive(true);
                        Price.SetActive(true);
                    }
                }

                carPriceText.text = carPrices[ID].ToString() + " $";

				nextClick = false;
			}
		}
		// Public function for PrevCar select button in menu
		public void PrevCar()
		{
			if (!nextClick)
			{

				prevClick = true;

				if (ID > 0)
					ID--;

				if (carSelectMode == CarSelectMode.Sport)
                    PlayerPrefs.SetInt("CarID_Sport", ID);
                if (carSelectMode == CarSelectMode.Truck)
                    PlayerPrefs.SetInt("CarID_Truck", ID);
                if (carSelectMode == CarSelectMode.F1)
                    PlayerPrefs.SetInt("CarID_F1", ID);
                if (carSelectMode == CarSelectMode.Offroad)
                    PlayerPrefs.SetInt("CarID_Offroad", ID);

				Destroy(GameObject.FindGameObjectWithTag("Player"));

                if (carSelectMode == CarSelectMode.Sport)
                    ID = PlayerPrefs.GetInt("CarID_Sport");
                if (carSelectMode == CarSelectMode.Truck)
                    ID = PlayerPrefs.GetInt("CarID_Truck");
                if (carSelectMode == CarSelectMode.F1)
                    ID = PlayerPrefs.GetInt("CarID_F1");
                if (carSelectMode == CarSelectMode.Offroad)
                    ID = PlayerPrefs.GetInt("CarID_Offroad");

                // Instantiate last selected car by saved ID
                GameObject car =  Instantiate(cars[ID], spawnPoint.position, spawnPoint.rotation);

               /* if (colorPicker != null)
                {
                    colorPicker.currentCarID = ID;
                    colorPicker.Load_Color();
                }*/

                if (carSelectMode == CarSelectMode.Sport)
                {
                    if (PlayerPrefs.GetInt("Car_Sport" + ID.ToString()) == 3)
                    {
                        lockIcon.SetActive(false);
                        buyButton.SetActive(false);
                        Price.SetActive(false);
                    }
                    else
                    {
                        lockIcon.SetActive(true);
                        buyButton.SetActive(true);
                        Price.SetActive(true);
                    }
                }
                if (carSelectMode == CarSelectMode.Truck)
                {
                    if (PlayerPrefs.GetInt("Car_Truck" + ID.ToString()) == 3)
                    {
                        lockIcon.SetActive(false);
                        buyButton.SetActive(false);
                        Price.SetActive(false);
                    }
                    else
                    {
                        lockIcon.SetActive(true);
                        buyButton.SetActive(true);
                        Price.SetActive(true);
                    }
                }
                if (carSelectMode == CarSelectMode.F1)
                {
                    if (PlayerPrefs.GetInt("Car_F1" + ID.ToString()) == 3)
                    {
                        lockIcon.SetActive(false);
                        buyButton.SetActive(false);
                        Price.SetActive(false);
                    }
                    else
                    {
                        lockIcon.SetActive(true);
                        buyButton.SetActive(true);
                        Price.SetActive(true);
                    }
                }
                if (carSelectMode == CarSelectMode.Offroad)
                {
                    if (PlayerPrefs.GetInt("Car_Offroad" + ID.ToString()) == 3)
                    {
                        lockIcon.SetActive(false);
                        buyButton.SetActive(false);
                        Price.SetActive(false);
                    }
                    else
                    {
                        lockIcon.SetActive(true);
                        buyButton.SetActive(true);
                        Price.SetActive(true);
                    }
                }

                carPriceText.text = carPrices[ID].ToString() + " $";

				prevClick = false;
			}
		}

		public void Buy_CurrentCar()
		{
			purchaseUI.SetActive(true);
			noEnoughScores.SetActive(false);
		}

		// Buy current selected car
		public void BuyCar()
		{

			// Check player have enough money
			if (carPrices[ID] <= PlayerPrefs.GetInt("TotalScores"))
			{
                if(carSelectMode == CarSelectMode.Sport)
				    PlayerPrefs.SetInt("Car_Sport" + ID.ToString(), 3);
                if(carSelectMode == CarSelectMode.Truck)
				    PlayerPrefs.SetInt("Car_Truck" + ID.ToString(), 3);
                if(carSelectMode == CarSelectMode.F1)
				    PlayerPrefs.SetInt("Car_F1" + ID.ToString(), 3);
                if(carSelectMode == CarSelectMode.Offroad)
				    PlayerPrefs.SetInt("Car_Offroad" + ID.ToString(), 3);
                
				// Reduce current car price from the total scores
				PlayerPrefs.SetInt("TotalScores",
					PlayerPrefs.GetInt("TotalScores") - carPrices[ID]);


				// Disable lock icon for current car
				lockIcon.SetActive(false);

				// Disable Buy button for current car
				buyButton.SetActive(false);

				// Disable car price text
				Price.SetActive(false);

				purchaseUI.SetActive(false);

				// Update total scores display
				TotalScores.text = PlayerPrefs.GetInt("TotalScores").ToString();
			}
			else
			{
				// Show the shop offer window
				noEnoughScores.SetActive(true);
				purchaseUI.SetActive(false);
			}
		}

		// Select current car
		public void SelectCar()
		{
            if (carSelectMode == CarSelectMode.Sport)
            {
                if (PlayerPrefs.GetInt("Car_Sport" + ID.ToString()) == 3)
                {
                    // Set current selected car ID for instantiate in main level    
                    if (carSelectMode == CarSelectMode.Sport)
                        PlayerPrefs.SetInt("CarID_Sport", ID);
                    if (carSelectMode == CarSelectMode.Truck)
                        PlayerPrefs.SetInt("CarID_Truck", ID);
                    if (carSelectMode == CarSelectMode.F1)
                        PlayerPrefs.SetInt("CarID_F1", ID);
                    if (carSelectMode == CarSelectMode.Offroad)
                        PlayerPrefs.SetInt("CarID_Offroad", ID);

                    Loading.SetActive(true);

                    SceneManager.LoadSceneAsync(levelSelect.levelNames[PlayerPrefs.GetInt("LevelID")]);
                }
                else
                {
                    purchaseUI.SetActive(true);
                    noEnoughScores.SetActive(false);
                }
            }
            if (carSelectMode == CarSelectMode.Truck)
            {
                if (PlayerPrefs.GetInt("Car_Truck" + ID.ToString()) == 3)
                {
                    // Set current selected car ID for instantiate in main level    
                    if (carSelectMode == CarSelectMode.Sport)
                        PlayerPrefs.SetInt("CarID_Sport", ID);
                    if (carSelectMode == CarSelectMode.Truck)
                        PlayerPrefs.SetInt("CarID_Truck", ID);
                    if (carSelectMode == CarSelectMode.F1)
                        PlayerPrefs.SetInt("CarID_F1", ID);
                    if (carSelectMode == CarSelectMode.Offroad)
                        PlayerPrefs.SetInt("CarID_Offroad", ID);

                    Loading.SetActive(true);

                    SceneManager.LoadSceneAsync(levelSelect.levelNames[PlayerPrefs.GetInt("LevelID")]);
                }
                else
                {
                    purchaseUI.SetActive(true);
                    noEnoughScores.SetActive(false);
                }
            }
            if (carSelectMode == CarSelectMode.F1)
            {
                if (PlayerPrefs.GetInt("Car_F1" + ID.ToString()) == 3)
                {
                    // Set current selected car ID for instantiate in main level    
                    if (carSelectMode == CarSelectMode.Sport)
                        PlayerPrefs.SetInt("CarID_Sport", ID);
                    if (carSelectMode == CarSelectMode.Truck)
                        PlayerPrefs.SetInt("CarID_Truck", ID);
                    if (carSelectMode == CarSelectMode.F1)
                        PlayerPrefs.SetInt("CarID_F1", ID);
                    if (carSelectMode == CarSelectMode.Offroad)
                        PlayerPrefs.SetInt("CarID_Offroad", ID);

                    Loading.SetActive(true);

                    SceneManager.LoadSceneAsync(levelSelect.levelNames[PlayerPrefs.GetInt("LevelID")]);
                }
                else
                {
                    purchaseUI.SetActive(true);
                    noEnoughScores.SetActive(false);
                }
            }
            if (carSelectMode == CarSelectMode.Offroad)
            {
                if (PlayerPrefs.GetInt("Car_Offroad" + ID.ToString()) == 3)
                {
                    // Set current selected car ID for instantiate in main level    
                    if (carSelectMode == CarSelectMode.Sport)
                        PlayerPrefs.SetInt("CarID_Sport", ID);
                    if (carSelectMode == CarSelectMode.Truck)
                        PlayerPrefs.SetInt("CarID_Truck", ID);
                    if (carSelectMode == CarSelectMode.F1)
                        PlayerPrefs.SetInt("CarID_F1", ID);
                    if (carSelectMode == CarSelectMode.Offroad)
                        PlayerPrefs.SetInt("CarID_Offroad", ID);

                    Loading.SetActive(true);

                    SceneManager.LoadSceneAsync(levelSelect.levelNames[PlayerPrefs.GetInt("LevelID")]);
                }
                else
                {
                    purchaseUI.SetActive(true);
                    noEnoughScores.SetActive(false);
                }
            }
		}
	}	
}