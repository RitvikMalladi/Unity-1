using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ALIyerEdon
{
    [ExecuteInEditMode]
    public class Track_Camera_Manager : MonoBehaviour
    {
        public Transform[] cameraList;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            cameraList = GetComponentsInChildren<Transform>();

#endif
        }

        void OnDrawGizmos()
        {
            if (cameraList.Length > 0)
            {

                Gizmos.color = Color.green;

                for(int a = 1;a< cameraList.Length;a++)
                    Gizmos.DrawSphere(cameraList[a].position, 1f);

                Gizmos.color = Color.yellow;

                for (int b = 1; b < cameraList.Length - 1; b++)
                    Gizmos.DrawLine(cameraList[b].position, cameraList[b + 1].position);
            }
        }
    }
}