//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using ALIyerEdon;

namespace ALIyerEdon
{
	public class ControllSelection : MonoBehaviour
	{

		public void SelectControl(int id)
		{

			PlayerPrefs.SetInt("ControlType", id);
		}
		public void SetFalse(GameObject target)
		{
			target.SetActive(false);
		}

		public void LoadLevel(string levelName)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
		}
	}
}