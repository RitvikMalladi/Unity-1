//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Select_CarColor : MonoBehaviour
    {
        public void Select_Color(int colorID)
        {
            if (FindObjectOfType<Car_Color>())
                FindObjectOfType<Car_Color>().Change_Color(colorID);
        }
    }
}