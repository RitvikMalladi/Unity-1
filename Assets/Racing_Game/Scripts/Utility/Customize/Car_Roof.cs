//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Car_Roof : MonoBehaviour
    {
		public void Select_Roof(int id)
		{

			GameObject.FindGameObjectWithTag("Player").
				GetComponentInChildren<Roof_Loader>().Select_Roof(id);

		}
	}
}