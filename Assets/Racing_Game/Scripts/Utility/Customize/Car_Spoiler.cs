//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Car_Spoiler : MonoBehaviour
    {
		public void Select_Spoiler(int id)
		{

			GameObject.FindGameObjectWithTag("Player").
				GetComponentInChildren<Spoiler_Loader>().Select_Spoiler(id);

		}
	}
}