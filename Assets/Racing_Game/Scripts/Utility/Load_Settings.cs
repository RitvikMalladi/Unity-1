//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________


using LightingBox.Effects;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ALIyerEdon
{
	public class Load_Settings : MonoBehaviour
	{

		public bool inGarage;
       // public bool enableNightLights;
		public float reflectionIntensityLow = 1f;
        public Color fogColor = Color.white;
        public GameObject[] localFog;

        float reflectionIntensityDefault;
        AudioSource music;

        IEnumerator Start()
		{
			reflectionIntensityDefault = FindFirstObjectByType<ReflectionProbe>().intensity;

            music = GetComponentInChildren<AudioSource>();

            Update_MusicVolume(PlayerPrefs.GetFloat("Music"));

            Set_Grass();

            // Don't apply quality settings for when is in the first runing mode
            if (PlayerPrefs.GetInt("FirstLoadSettings") != 1)
                PlayerPrefs.SetInt("FirstLoadSettings", 1); 
            else
                Set_QualityLevel(PlayerPrefs.GetInt("QualityLevel"));
            
            Update_TerrainQuality();
            Set_LocalPosition();
            Set_DisplayFPS();
            Set_SSR();

            if (PlayerPrefs.GetInt("MotionBlur") == 0)
				Set_MotionBlur(false);
			if (PlayerPrefs.GetInt("MotionBlur") == 1)
				Set_MotionBlur(true);

			if (PlayerPrefs.GetInt("DepthOfField") == 0)
				Set_DepthOfField(false);
			if (PlayerPrefs.GetInt("DepthOfField") == 1)
				Set_DepthOfField(true);

            yield return new WaitForEndOfFrame();

            // GameObject.FindGameObjectWithTag("Player").GetComponent<EasyCarController>().Toggle_FrontLights(enableNightLights);

            #region Fog Smoke
            if (localFog.Length != 0)
            {
                for (int a = 0; a < localFog.Length; a++)
                {
                    localFog[a].GetComponent<ParticleSystem>().startColor = fogColor;
                }
            }
            // Update wheel smoke effects
            EasyCarAudio[] carAudio = FindObjectsOfType<EasyCarAudio>();

            for (int a = 0; a < carAudio.Length; a++)
            {
                for (int b = 0; b < carAudio[a].wheelSmokes.Length; b++)
                {
                    carAudio[a].wheelSmokes[b].GetComponent<ParticleSystem>().startColor = fogColor;
                }

                carAudio[a].offroadSmoke.GetComponent<ParticleSystem>().startColor = fogColor;
            }
            #endregion

        }

        public void Update_MusicVolume(float volume)
		{
			music.volume = volume;
		}

		public void Update_TerrainQuality()
		{
			if (FindFirstObjectByType<Terrain>())
			{
				if (PlayerPrefs.GetInt("TerrainQuality") == 0)
					FindFirstObjectByType<Terrain>().heightmapPixelError = 200;
				if (PlayerPrefs.GetInt("TerrainQuality") == 1)
					FindFirstObjectByType<Terrain>().heightmapPixelError = 30;
				if (PlayerPrefs.GetInt("TerrainQuality") == 2)
					FindFirstObjectByType<Terrain>().heightmapPixelError = 1;
			}
        }

        public void Set_QualityLevel(int level)
        {

            QualitySettings.SetQualityLevel(level);

            // UniversalRenderPipelineAsset data = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            // Set render scale to the 1 because we want to control it manually
            /*  if (data)
                  data.renderScale = 1f;*/

            FindFirstObjectByType<ReflectionProbe>().intensity = reflectionIntensityLow;

            if (level == 0) // Very Low
            {
                foreach (Camera cam in FindObjectsOfType<Camera>())
                {
                    if (cam.name != "Minimap_Camera")
                    {
                        cam.GetComponent<UniversalAdditionalCameraData>()
                        .antialiasing = AntialiasingMode.None;
                    }
                }
                if (FindFirstObjectByType<Terrain>())
                {
                    FindFirstObjectByType<Terrain>().detailObjectDensity = 0;
                    FindFirstObjectByType<Terrain>().detailObjectDistance = 0;
                    FindFirstObjectByType<Terrain>().heightmapPixelError = 200f;
                }
            }
            if (level == 1) // Low
            {
                foreach (Camera cam in FindObjectsOfType<Camera>())
                {
                    if (cam.name != "Minimap_Camera")
                    {
                        cam.GetComponent<UniversalAdditionalCameraData>()
                        .antialiasing = AntialiasingMode.None;
                    }
                }
                if (FindFirstObjectByType<Terrain>())
                {
                    FindFirstObjectByType<Terrain>().detailObjectDistance = 100f;
                    FindFirstObjectByType<Terrain>().heightmapPixelError = 170f;
                }
            }
            if (level == 2) // Medium
            {
                foreach (Camera cam in FindObjectsOfType<Camera>())
                {
                    if (cam.name != "Minimap_Camera")
                    {
                        cam.GetComponent<UniversalAdditionalCameraData>()
                        .antialiasing = AntialiasingMode.TemporalAntiAliasing;
                        cam.GetComponent<UniversalAdditionalCameraData>()
                                .taaSettings.quality = TemporalAAQuality.High;
                 }
                }
                if (FindFirstObjectByType<Terrain>())
                {
                    FindFirstObjectByType<Terrain>().detailObjectDistance = 150f;
                    FindFirstObjectByType<Terrain>().heightmapPixelError = 150f;
                }
            }
            if (level == 3) // High
            {
                foreach (Camera cam in FindObjectsOfType<Camera>())
                {
                    if (cam.name != "Minimap_Camera")
                    {
                        cam.GetComponent<UniversalAdditionalCameraData>()
                        .antialiasing = AntialiasingMode.TemporalAntiAliasing;
                        cam.GetComponent<UniversalAdditionalCameraData>()
                                .taaSettings.quality = TemporalAAQuality.VeryHigh;
                    }
                }
                if (FindFirstObjectByType<Terrain>())
                {
                    FindFirstObjectByType<Terrain>().detailObjectDistance = 300f;
                    FindFirstObjectByType<Terrain>().heightmapPixelError = 100f;
                }
            }
            if (level == 4) // Ultra
            {
                foreach (Camera cam in FindObjectsOfType<Camera>())
                {
                    if (cam.name != "Minimap_Camera")
                    {
                        cam.GetComponent<UniversalAdditionalCameraData>()
                            .antialiasing = AntialiasingMode.TemporalAntiAliasing;
                        cam.GetComponent<UniversalAdditionalCameraData>()
                                .taaSettings.quality = TemporalAAQuality.VeryHigh;
                    }
                }
                if (FindFirstObjectByType<Terrain>())
                {
                    FindFirstObjectByType<Terrain>().detailObjectDistance = 300f;
                    FindFirstObjectByType<Terrain>().heightmapPixelError = 30f;
                }
            }
            if (level == 5) // Max - higher resolution only (90% of the native resolution)
            {
                foreach (Camera cam in FindObjectsOfType<Camera>())
                {
                    if (cam.name != "Minimap_Camera")
                    {
                        cam.GetComponent<UniversalAdditionalCameraData>()
                            .antialiasing = AntialiasingMode.TemporalAntiAliasing;
                        cam.GetComponent<UniversalAdditionalCameraData>()
                                .taaSettings.quality = TemporalAAQuality.VeryHigh;
                    }
                }
                if (FindFirstObjectByType<Terrain>())
                {
                    FindFirstObjectByType<Terrain>().detailObjectDistance = 300f;
                    FindFirstObjectByType<Terrain>().heightmapPixelError = 30f;
                }
            }

           // GameObject.Find("Minimap_Camera").GetComponent<Camera>().renderingPath = RenderingPath.Forward;
        }

        public void Set_SSR()
        {
            ScreenSpaceReflection ssr;

            Volume volume = FindFirstObjectByType<Volume>();

            volume.profile.TryGet<ScreenSpaceReflection>(out ssr);
            
            if (PlayerPrefs.GetInt("SSR") == 1)
                ssr.active = true;
            else
                ssr.active = false;
        }

        public void Set_MotionBlur(bool enabled)
		{
			MotionBlur mb;

			Volume volume = FindFirstObjectByType<Volume>();

			volume.profile.TryGet<MotionBlur>(out mb);

			mb.active = enabled;
		}

        public void Set_Grass()
        {
            if (FindFirstObjectByType<Terrain>())
            {

                FindFirstObjectByType<Terrain>().detailObjectDistance = 100f;

                if (PlayerPrefs.GetInt("Grass") == 0)
                {
                    FindFirstObjectByType<Terrain>().detailObjectDensity = 0;
                    FindFirstObjectByType<Terrain>().detailObjectDistance = 0;
                }
                if (PlayerPrefs.GetInt("Grass") == 1)
                    FindFirstObjectByType<Terrain>().detailObjectDensity = 0.3f;
                if (PlayerPrefs.GetInt("Grass") == 2)
                    FindFirstObjectByType<Terrain>().detailObjectDensity = 0.5f;
                if (PlayerPrefs.GetInt("Grass") == 3)
                    FindFirstObjectByType<Terrain>().detailObjectDensity = 1f;
            }
        }

        public void Set_DepthOfField(bool enabled)
		{
			DepthOfField dof;

			Volume volume = FindFirstObjectByType<Volume>();

			volume.profile.TryGet<DepthOfField>(out dof);

			dof.active = enabled;
		}

		public void Set_DisplayFPS()
		{
            if (FindFirstObjectByType<FPSCounter>())
                FindFirstObjectByType<FPSCounter>().Update_Settings();
        }

		public void Set_LocalPosition()
		{
            foreach (Car_Position pos in FindObjectsByType<Car_Position>(FindObjectsSortMode.None))
            {
                if (PlayerPrefs.GetInt("ShowLocalPosition") == 1 && !inGarage)
                {
                    pos.displayPosition = true;
                    FindFirstObjectByType<Race_Manager>().showLocalPosition = true;
                }
                else
                {
                    pos.displayPosition = false;
                    FindFirstObjectByType<Race_Manager>().showLocalPosition = false;
                }
            }
        }
    }
}