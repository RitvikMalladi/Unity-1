//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using ALIyerEdon;

namespace ALIyerEdon
{
	public class Timer : MonoBehaviour
	{
		public Text timerLabel;
		float timecount;
		private float time, starttime;
		int fraction, minutes, seconds;


		void Start()
		{
			starttime = Time.time;
		}

		void Update()
		{

			timecount = Time.time - starttime;
			minutes = (int)Mathf.Floor(timecount / 60); //Divide the guiTime by sixty to get the minutes.
			seconds = (int)Mathf.Floor(timecount % 60);//Use the euclidean division for the seconds.
			fraction = (int)Mathf.Floor((timecount * 100) % 100);

			//update the label value
			timerLabel.text = string.Format("{0:00} : {1:00} : {2:00}", minutes, seconds, fraction);
		}
	}
}