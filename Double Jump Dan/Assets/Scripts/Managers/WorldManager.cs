using UnityEngine;
using System.Collections.Generic;
using System;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    [Header("Time of Day")]
    public Material mainMaterial;
    [SerializeField] List<Material> materials = new List<Material>();
    public SpriteRenderer sky;
    [SerializeField] GameObject sunGlow;
    [SerializeField] GameObject sunPivot;
    [SerializeField] GameObject sun;
    [SerializeField] GameObject moon;
    public SpriteRenderer stars;
    [SerializeField] Sprite skyDay;
    [SerializeField] Sprite skySunset;
    [SerializeField] Sprite skyNight;
    [SerializeField] Sprite skySunrise;
    [SerializeField] Sprite skyOvercast;

    [Header("Weather")]
    public Transform clouds;
    public Color sunsetCloudTint;
    public Color sunriseCloudTint;
    public GameObject dust;
    [SerializeField] MeshRenderer[] dustMeshes;
    public Transform snow;

    [Header("Distortion Effects")]
    [SerializeField] GameObject heatWave;
    
    public event Action OnInitialized;
    public bool Initiated { get; private set; }
    float cloudEmissionOverTime;
    Camera _camera;
    ParticleSystem cloudParticleSystem;
    ParticleSystemRenderer cloudParticleSystemRenderer;
    LocalWorldManager localWorldManager;
    ScreenEffectsManager screenEffectsManager;
    MeshRenderer heatWaveMesh;
    ParticleSystem snowParticles;
    bool inMainMenu;
    static int mainMenuTimeOfDayIndex = -1;

    void Awake()
    {
        Instance = this;
        localWorldManager = GameObject.FindWithTag("Level Managers").GetComponent<LocalWorldManager>();

        if(localWorldManager.world == LocalWorldManager.World.MainMenu)
            inMainMenu = true;
    }

    void Start()
    {
        if(!GameManager.Instance.InMainMenu())
            _camera = LevelManager.Instance.mainCamera;
        else
            _camera = Camera.main;
            
        screenEffectsManager = GetComponent<ScreenEffectsManager>();
  
        if(localWorldManager.world != LocalWorldManager.World.Tutorial)
        {
            cloudEmissionOverTime = localWorldManager.cloudEmissionOverTime;
            
            switch(localWorldManager.timeOfDay)
            {   
                case LocalWorldManager.TimeOfDay.Day:
                    SetDay();
                    break;
                case LocalWorldManager.TimeOfDay.Sunset:
                    SetSunset();
                    break;
                case LocalWorldManager.TimeOfDay.Night:
                    SetNight();
                    break;
                case LocalWorldManager.TimeOfDay.Sunrise:
                    SetSunrise();
                    break;
                case LocalWorldManager.TimeOfDay.Overcast:
                    SetOvercast();
                    break;
                case LocalWorldManager.TimeOfDay.Random:
                    SetRandomTimeOfDay();
                    break;
            }

            if(inMainMenu)
            {
                if(localWorldManager.timeOfDay == LocalWorldManager.TimeOfDay.Day)
                {
                    localWorldManager.weatherType = LocalWorldManager.WeatherType.Dusting;    
                    localWorldManager.distortionType = LocalWorldManager.DistortionType.HeatWave;
                }
                else
                {
                    localWorldManager.weatherType = LocalWorldManager.WeatherType.None;    
                    localWorldManager.distortionType = LocalWorldManager.DistortionType.None;
                }
            }

            if(snow != null)
                snowParticles = snow.GetChild(1).GetComponent<ParticleSystem>();
          
            UpdatePositionAndScale();
    
            UpdateWeatherEffects(GameManager.Instance.weatherEffects);
            UpdateDistortionEffects(GameManager.Instance.distortionEffects);
        }
        else
        {
            sky.gameObject.SetActive(false);
            sunPivot.SetActive(false);
            clouds.gameObject.SetActive(false);
            
            SetSkyAndMaterialsColor(1, 1, 1, 0);
        }
        
        Initiated = true;
        OnInitialized?.Invoke();
    }

    Vector2 BackgroundWeatherScale()
    {
        return new Vector2(CameraWidth() * 2 * 3, _camera.orthographicSize * 2 * 3);
    }

    public void UpdateWeatherEffects(bool isOn)
    {
        if(!isOn)
        {
            if(snow != null)
                snow.gameObject.SetActive(false);

            if(dust != null)  
                dust.SetActive(false);

            if(clouds != null)
                clouds.gameObject.SetActive(false);

            return;
        }

        if(cloudEmissionOverTime > 0)
        {
            clouds.gameObject.SetActive(true);

            cloudParticleSystem = clouds.GetComponent<ParticleSystem>();

            ParticleSystem.EmissionModule emission = cloudParticleSystem.emission;
            emission.rateOverTime = cloudEmissionOverTime;

            cloudParticleSystemRenderer = clouds.GetComponent<ParticleSystemRenderer>();

            cloudParticleSystem.Stop();
            cloudParticleSystem.Play();
            
            if(localWorldManager.timeOfDay == LocalWorldManager.TimeOfDay.Sunset)
                cloudParticleSystemRenderer.material.color = new Color(sunsetCloudTint.r, sunsetCloudTint.g, sunsetCloudTint.b, cloudParticleSystemRenderer.material.color.a);
            else if(localWorldManager.timeOfDay == LocalWorldManager.TimeOfDay.Sunrise)
                cloudParticleSystemRenderer.material.color = new Color(sunriseCloudTint.r, sunriseCloudTint.g, sunriseCloudTint.b, cloudParticleSystemRenderer.material.color.a);
            else
                cloudParticleSystemRenderer.material.color = new Color(mainMaterial.color.r, mainMaterial.color.g, mainMaterial.color.b, cloudParticleSystemRenderer.material.color.a);
        }
        else
        {
            clouds.gameObject.SetActive(false);
        }

        if(localWorldManager.weatherType == LocalWorldManager.WeatherType.Snowing)
        {
            screenEffectsManager.SetTintColor(localWorldManager.snowingTintColor);

            snow.gameObject.SetActive(true);
            sun.SetActive(false);
            clouds.gameObject.SetActive(false);

            snowParticles.Stop();
            snowParticles.Play();
        }
        else
        {
            if(snow != null)
                snow.gameObject.SetActive(false);
        }

        if(localWorldManager.weatherType == LocalWorldManager.WeatherType.Dusting)
            dust.gameObject.SetActive(true);
        else
            dust.gameObject.SetActive(false);
    }

    public void UpdateDistortionEffects(bool isOn)
    {
        if(!isOn)
        {
            heatWave.SetActive(false);
            return;
        }

        if(localWorldManager.distortionType == LocalWorldManager.DistortionType.HeatWave)
        {
            heatWaveMesh = heatWave.GetComponent<MeshRenderer>();
            
            heatWave.SetActive(true);
            heatWaveMesh.material.SetFloat("Distortion", localWorldManager.heatWaveDistortionIntensity);
        }
        else
        {
            heatWave.SetActive(false);
        }
    }

    public void UpdatePositionAndScale()
    {
        if(!inMainMenu)
        {
            sky.transform.localScale = new Vector2(CameraWidth() + 0.2f, _camera.orthographicSize / 17.1875f + 0.01f);
            stars.size = new Vector2(CameraWidth() * 2 + 0.5f, _camera.orthographicSize * 2 + 0.5f);
        }

        if(localWorldManager.distortionType == LocalWorldManager.DistortionType.HeatWave)
            heatWave.transform.localScale = new Vector3(CameraWidth() / 5 + 0.2f, 1, _camera.orthographicSize / 5 + 0.2f);

        if(localWorldManager.weatherType == LocalWorldManager.WeatherType.Dusting)
            dust.transform.localScale = BackgroundWeatherScale();

        if(localWorldManager.weatherType == LocalWorldManager.WeatherType.Snowing)
        {
            snow.GetChild(0).transform.localScale = BackgroundWeatherScale();

            ParticleSystem.ShapeModule shapeModule = snowParticles.shape;
            shapeModule.radius = CameraWidth() * 2 / 5;

            snow.GetChild(1).transform.localPosition = new Vector3(-CameraWidth(), _camera.orthographicSize, 20);
        }

        if(cloudEmissionOverTime > 0)
            clouds.localPosition = new Vector3(-CameraWidth(), clouds.localPosition.y, clouds.localPosition.z);
    }

    float CameraWidth()
    {
        return _camera.orthographicSize * ((float)Screen.width / Screen.height);
    }

    public void SetDay()
    {
        sunGlow.SetActive(false);

        sunPivot.transform.eulerAngles = new Vector3(0, 0, 55);
        sun.transform.eulerAngles = new Vector3(0, 0, 0);
        moon.transform.eulerAngles = new Vector3(0, 0, 0);
        
        if(stars.gameObject.activeInHierarchy)
            stars.gameObject.SetActive(false);

        if(localWorldManager.weatherType != LocalWorldManager.WeatherType.Snowing)
            screenEffectsManager.SetTintColor(localWorldManager.dayTintColor);                
        
        SetSkyAndMaterialsColor(1, 1, 1, 0);
    }

    public void SetSunset()
    {
        sunGlow.SetActive(true);

        sunPivot.transform.localEulerAngles = new Vector3(0, 0, 0);
        sun.transform.eulerAngles = new Vector3(0, 0, 0);
        moon.transform.eulerAngles = new Vector3(0, 0, 0);

        if(stars.gameObject.activeInHierarchy)
            stars.gameObject.SetActive(false);

        if(localWorldManager.weatherType != LocalWorldManager.WeatherType.Snowing)
            screenEffectsManager.SetTintColor(localWorldManager.sunsetTintColor);

        SetSkyAndMaterialsColor(0.3f, 0.3f, 0.3f, 1);

        if(inMainMenu)
            sunPivot.transform.localPosition = new Vector3(sunPivot.transform.localPosition.x, 0, sunPivot.transform.localPosition.z);
    }

    public void SetNight()
    {
        sunPivot.transform.localEulerAngles = new Vector3(0, 0, 210);
        sun.transform.eulerAngles = new Vector3(0, 0, 0);
        moon.transform.eulerAngles = new Vector3(0, 0, 0);

        stars.gameObject.SetActive(true);

        if(localWorldManager.weatherType != LocalWorldManager.WeatherType.Snowing)
            screenEffectsManager.SetTintColor(localWorldManager.nightTintColor);

        SetSkyAndMaterialsColor(0.3f, 0.3f, 0.3f, 2);
    }

    public void SetSunrise()
    {
        sunPivot.SetActive(false);

        if(stars.gameObject.activeInHierarchy)
            stars.gameObject.SetActive(false);

        if(localWorldManager.weatherType != LocalWorldManager.WeatherType.Snowing)
            screenEffectsManager.SetTintColor(localWorldManager.sunriseTintColor);

        SetSkyAndMaterialsColor(0.6f, 0.6f, 0.6f, 3);
    }
    
    public void SetOvercast()
    {
        sun.SetActive(false);

        if(stars.gameObject.activeInHierarchy)
            stars.gameObject.SetActive(false);

        if(localWorldManager.weatherType != LocalWorldManager.WeatherType.Snowing)
            screenEffectsManager.SetTintColor(localWorldManager.overcastTintColor);

        SetSkyAndMaterialsColor(0.6f, 0.6f, 0.6f, 4);
    }

    void SetRandomTimeOfDay()
    {
        int timeIndex;

        if(inMainMenu)
        {
            if(mainMenuTimeOfDayIndex == -1)
            {
                mainMenuTimeOfDayIndex = UnityEngine.Random.Range(0, 4);
                timeIndex = mainMenuTimeOfDayIndex;
            }
            else
            {
                mainMenuTimeOfDayIndex++;

                if(mainMenuTimeOfDayIndex > 3)
                    mainMenuTimeOfDayIndex = 0;

                timeIndex = mainMenuTimeOfDayIndex;
            }
        }
        else
        {
            timeIndex = UnityEngine.Random.Range(0, 4);
        }

        if(timeIndex == 0)
        {
            localWorldManager.timeOfDay = LocalWorldManager.TimeOfDay.Day;
            SetDay();
        }
        else if(timeIndex == 1)
        {
            localWorldManager.timeOfDay = LocalWorldManager.TimeOfDay.Sunset;
            SetSunset();
        }
        else if(timeIndex == 2)
        {
            localWorldManager.timeOfDay = LocalWorldManager.TimeOfDay.Night;
            SetNight();
        }
        else if(timeIndex == 3)
        {
            localWorldManager.timeOfDay = LocalWorldManager.TimeOfDay.Sunrise;
            SetSunrise();
        }
    }

    void SetSkyAndMaterialsColor(float r, float g, float b, int skyIndex)
    {
        if(localWorldManager.weatherType == LocalWorldManager.WeatherType.Snowing || localWorldManager.timeOfDay == LocalWorldManager.TimeOfDay.Overcast)
        {
            sky.color = new Color(r, g, b, mainMaterial.color.a);
            sky.sprite = skyOvercast;
        }
        else
        {
            if(skyIndex == 0)
                sky.sprite = skyDay;
            else if(skyIndex == 1)
                sky.sprite = skySunset;
            else if(skyIndex == 2)
                sky.sprite = skyNight;
            else if(skyIndex == 3)
                sky.sprite = skySunrise;
            else if(skyIndex == 4)
                sky.sprite = skyOvercast;
        }

        if(localWorldManager.weatherType == LocalWorldManager.WeatherType.Dusting)
            foreach(MeshRenderer mesh in dustMeshes)
                mesh.material.color = new Color(mesh.material.color.r, mesh.material.color.g, mesh.material.color.b, localWorldManager.dustAlpha);
            
        mainMaterial.color = new Color(r, g, b, mainMaterial.color.a);
        
        foreach(Material mat in materials)
            mat.color = mainMaterial.color;
    }

    public bool UseWeatherEffects()
    {
        if(GameManager.Instance.weatherEffects)
            return true;
        else
            return false;
    }

    void OnApplicationQuit()
    {                
        mainMaterial.color = new Color(1, 1, 1, mainMaterial.color.a);
        
        foreach(Material mat in materials)
            mat.color = mainMaterial.color;
    }
}