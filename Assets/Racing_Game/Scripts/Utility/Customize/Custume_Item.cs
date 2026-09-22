//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Custume_Item : MonoBehaviour
    {
		public void Select_Item(int id)
		{

			GameObject.FindGameObjectWithTag("Player").
				GetComponentInChildren<Custume_Item_Loader>().Select_Item(id);

		}
	}
}