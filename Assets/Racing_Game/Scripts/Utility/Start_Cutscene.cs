using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALIyerEdon {
    
    public class Start_Cutscene : MonoBehaviour
    {
        public GameObject banner;
        public GameObject skipButton;
        public Text trackName;
        public Text detailInfo;

        public float skipDelay = 7f;
        public float moveSpeed = 10;

        [SerializeField]
        public Cutscene_Camera[] cutsceneCamera;

        int currentCamera;
        Camera mainCamera;
        
        // Reset cutscene camera position at the end (reset cutscene)
        Vector3[] originalPositions;
        
        IEnumerator Start()
        {

            originalPositions = new Vector3[cutsceneCamera.Length];

            if (skipButton)
                skipButton.SetActive(false);

            // Disable all cameras at start
            for (int c = 0; c < cutsceneCamera.Length; c++)
            {
                originalPositions[c] = cutsceneCamera[c].camera.transform.localPosition;
                cutsceneCamera[c].camera.SetActive(false);
            }

            // Start camera animation
            StartCoroutine(Play_Animation());

            yield return new WaitForEndOfFrame();

            GameObject.FindGameObjectWithTag("Player").GetComponent
            <EasyCarAudio>().engineSource.volume = 0f;

            mainCamera = FindFirstObjectByType<SmoothFollow2>().GetComponentInChildren<Camera>();
            mainCamera.enabled = false;

            // Display banner
            if (banner)
                banner.SetActive(true);

            if (trackName)
                trackName.text = FindFirstObjectByType<Race_Manager>().trackName;

            if (detailInfo)
                detailInfo.text = "Driver Name: " + GameObject.FindGameObjectWithTag("Player").GetComponent<Car_Position>().RacerName;

            yield return new WaitForSeconds(skipDelay);

            if(skipButton)
                skipButton.SetActive(true);

        }

        IEnumerator Play_Animation()
        {

            // Select the next camer in time delay
            for (int a = 0; a < cutsceneCamera.Length; a++)
            {
                for (int c = 0; c < cutsceneCamera.Length; c++)
                {
                    cutsceneCamera[c].camera.SetActive(false);
                }

                cutsceneCamera[a].camera.SetActive(true);

                cutsceneCamera[a].camera.GetComponent<Camera>().fieldOfView = cutsceneCamera[a].fieldOfView;

                currentCamera = a;

                yield return new WaitForSeconds(cutsceneCamera[a].duration);
            }

            FindFirstObjectByType<Race_Manager>().Show_StartUI();

            if (banner)
                banner.SetActive(false);

            if (skipButton)
                skipButton.SetActive(false);

            // Reset cutscene
            for (int c = 0; c < cutsceneCamera.Length; c++)
                cutsceneCamera[c].camera.transform.localPosition = originalPositions[c];

            StartCoroutine(Play_Animation());
        }
            
        void Update()
        {
            if(cutsceneCamera[currentCamera].direction == Move_Direction.Forward)
                cutsceneCamera[currentCamera].camera.transform.Translate(Vector3.forward * (Time.deltaTime * cutsceneCamera[currentCamera].intensity));
            if (cutsceneCamera[currentCamera].direction == Move_Direction.Side)
                cutsceneCamera[currentCamera].camera.transform.Translate(Vector3.left * (Time.deltaTime * cutsceneCamera[currentCamera].intensity));
            if (cutsceneCamera[currentCamera].direction == Move_Direction.Up)
                cutsceneCamera[currentCamera].camera.transform.Translate(Vector3.up * (Time.deltaTime * cutsceneCamera[currentCamera].intensity));
        }

        public void Start_Race()
        {

            StopCoroutine(Play_Animation());

            // Disable all cameras at start
            for (int c = 0; c < cutsceneCamera.Length; c++)
            {
                cutsceneCamera[c].camera.SetActive(false);
            }

            mainCamera.enabled = true;

            GameObject.Destroy(gameObject);
        }

        public void Skip_Cutscene()
        {
            FindFirstObjectByType<Race_Manager>().Show_StartUI();

            banner.SetActive(false);
            
            skipButton.SetActive(false);
        }
    }

    public enum Move_Direction
    {
        Forward, Up, Side
    }

    [System.Serializable]
    public class Cutscene_Camera
    {
        public GameObject camera;
        public Move_Direction direction;
        public float duration;
        public float fieldOfView = 45f;
        public float intensity = 1f;
    }
}