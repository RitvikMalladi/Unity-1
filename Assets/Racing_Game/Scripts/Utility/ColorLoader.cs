//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using ALIyerEdon;

namespace ALIyerEdon
{
	public class ColorLoader : MonoBehaviour
	{
		public Material carBody;

        public int carId;

        void Start()
        {
            UpdateColor(PlayerPrefs.GetFloat("Red" + carId.ToString()),
               PlayerPrefs.GetFloat("Green" + carId.ToString()),
               PlayerPrefs.GetFloat("Blue" + carId.ToString()),
               PlayerPrefs.GetFloat("Metallic" + carId.ToString()),
               PlayerPrefs.GetFloat("Smoothness" + carId.ToString()));
        }

        public void UpdateColor(float red, float green, float blue, float metallic, float smoothness )
        {
			carBody.SetColor("_Color", new Color(red, green, blue));
			carBody.SetFloat("_Metallic", metallic);
			carBody.SetFloat("_Glossiness", smoothness);
        }
	}
}