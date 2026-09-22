//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using ALIyerEdon;

namespace ALIyerEdon
{
	public class MainMenu : MonoBehaviour
	{

		// Use this for initialization
		public GameObject Loading;

		public void LoadLevel(string name)
		{
			Loading.SetActive(true);
			UnityEngine.SceneManagement.SceneManager.LoadScene(name);

		}
		public void Exit()
		{
			Application.Quit();
		}
	}
}