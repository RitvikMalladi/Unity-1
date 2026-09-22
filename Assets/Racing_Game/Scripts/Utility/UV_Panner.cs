using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ALIyerEdon
{
    public class UV_Panner : MonoBehaviour
    {
        public float horizontalScrollSpeed = 0.25f;
        public float verticalScrollSpeed = 0.25f;

        private bool scroll = true;

        public void FixedUpdate()
        {
            if (scroll)
            {
                float verticalOffset = Time.time * verticalScrollSpeed;
                float horizontalOffset = Time.time * horizontalScrollSpeed;
                GetComponent<MeshRenderer>().sharedMaterial.mainTextureOffset = new Vector2(horizontalOffset, verticalOffset);
            }
        }

        public void DoActivateTrigger()
        {
            scroll = !scroll;
        }

    }
}
