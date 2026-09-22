//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using ALIyerEdon;

namespace ALIyerEdon
{
    public class CameraSwitch : MonoBehaviour
    {
        [SerializeField]
        public CameraView[] cameraView;

        // Hold curent active camera id
        int currentCamera = 0;
        SmoothFollow2 smoothFollow;

        void Start()
        {
            if (FindFirstObjectByType<SmoothFollow2>())
            {
                smoothFollow = FindFirstObjectByType<SmoothFollow2>();

                smoothFollow.smooth = cameraView[currentCamera].Smooth;
                smoothFollow.distance = cameraView[currentCamera].Distance;
                smoothFollow.height = cameraView[currentCamera].Height;
                smoothFollow.Angle = cameraView[currentCamera].Angle;
            }
        }

#if UNITY_EDITOR
        void Update()
        {
            if (cameraView[currentCamera].captureCurrentView)
            {
                cameraView[currentCamera].captureCurrentView = false;

                cameraView[currentCamera].Smooth =
                    FindFirstObjectByType<SmoothFollow2>().smooth;
                cameraView[currentCamera].Distance =
                   FindFirstObjectByType<SmoothFollow2>().distance;
                cameraView[currentCamera].Height =
                   FindFirstObjectByType<SmoothFollow2>().height;
                cameraView[currentCamera].Angle =
                   FindFirstObjectByType<SmoothFollow2>().Angle;
            }

        }
#endif
        // Switch to next camera based total camera counts
        public void NextCamera()
        {
            if (currentCamera < cameraView.Length - 1)
                currentCamera++;
            else
                currentCamera = 0;

            smoothFollow.smooth = cameraView[currentCamera].Smooth;
            smoothFollow.distance = cameraView[currentCamera].Distance;
            smoothFollow.height = cameraView[currentCamera].Height;
            smoothFollow.Angle = cameraView[currentCamera].Angle;

            if (cameraView[currentCamera].DashboardCamera)
            {
                smoothFollow.SwitchTarget(true);
                if(GetComponent<EasyCarController>().interior)
                    GetComponent<EasyCarController>().interior.SetActive(true);
                if(GetComponent<EasyCarController>().carMesh)
                    GetComponent<EasyCarController>().carMesh.SetActive(false);
            }
            else
            {
                smoothFollow.SwitchTarget(false);

                if (GetComponent<EasyCarController>().interior)
                    GetComponent<EasyCarController>().interior.SetActive(false);
               
                if (GetComponent<EasyCarController>().carMesh)
                    GetComponent<EasyCarController>().carMesh.SetActive(true);
            }
        }

        [System.Serializable]
        public class CameraView
        {
            public float Smooth;
            public float Distance;
            public float Height;
            public float Angle;
            public bool captureCurrentView;
            public bool DashboardCamera;
        }
    }
}