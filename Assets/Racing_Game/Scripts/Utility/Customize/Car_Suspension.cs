//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALIyerEdon
{
    public class Car_Suspension : MonoBehaviour
    {
		public Slider suspensionDistance;

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            Update_Slider();
        
        }
        public void Set_Suspension()
		{
			GameObject.FindObjectOfType<Suspension_Loader>()
                .Select_Suspension(suspensionDistance.value);
		}
        public void Update_Slider()
        {
            try
            {
                suspensionDistance.value =
                          PlayerPrefs.GetFloat("Car" + GameObject.FindObjectOfType<Suspension_Loader>().carID.ToString() + "Suspension");
            }
            catch { }
        }
    }
}