using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    [SerializeField] Transform levelStart;
    public Transform levelFinish;

    [Header("Finish Level")]
    [SerializeField] float maxDetectionHeight;

    [Header("Object Zones")]
    [SerializeField] float checkFrequency;
    [SerializeField] float extraZoneBuffer;
    [SerializeField] List<ObjectZone> objectZones = new List<ObjectZone>();

    [Header("Random Object Placement")]
    [SerializeField] List<SpawnType> spawnTypes = new List<SpawnType>();

    [Header("Gem Sprites")]
    public Sprite[] gemSprites;

    public event Action OnLevelFinished;
    public Player player { get; private set; }
    public BoxCollider2D levelBounds { get; private set; }
    public Transform levelObjects { get; private set; }
    public LocalWorldManager localWorldManager { get; private set; }
    public Camera mainCamera { get; private set; }
    public int gems { get; set; }
    public Vector2 currentSpawnPoint { get; private set; }
    public SpriteRenderer centralizedGem { get; set; }
    bool playerEntered;
    float xDistance = 1.5f;
    GameManager gameManager;
    int currentScene;
    bool finishedLevel;
    int objectSpawnProbability;
    int randomObjectIndex;
    float _checkTimer;
    public bool doubleGems { get; private set; }
    
    void Awake()
    {
        Instance = this;

        if(levelStart != null)
            currentSpawnPoint = levelStart.position;
        else
            currentSpawnPoint = player.transform.position;

        localWorldManager = GetComponent<LocalWorldManager>();
        levelBounds = GameObject.FindWithTag("Bounds").GetComponent<BoxCollider2D>();
        levelObjects = GameObject.FindWithTag("Level Objects").transform;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        centralizedGem = transform.Find("Centralized Gem").GetComponent<SpriteRenderer>();  
        mainCamera = Camera.main;    
            
        Respawn();
    }

    void Start()
    {
        gameManager = GameManager.Instance;

        if(SceneManager.GetActiveScene().buildIndex > 2)
        {
            string levelName = SceneManager.GetActiveScene().name;
            currentScene = int.Parse(levelName.Replace("Level ", ""));
        }

        if(spawnTypes.Count > 0)
        {
            foreach(var spawnType in spawnTypes)
            {
                for(int i = 0; i < spawnType.objectSpawnPoints.Count; i++)
                {
                    if(spawnType.objectsParent == null)
                    {
                        GameObject objectHolder = new GameObject(spawnType.name);
                        spawnType.objectsParent = objectHolder.transform;
                        spawnType.objectsParent.parent = GameObject.FindWithTag("Level Objects").transform;
                    }
                        
                    randomObjectIndex = UnityEngine.Random.Range(0, spawnType.objects.Count);
                    objectSpawnProbability = UnityEngine.Random.Range(0, 100);

                    if(objectSpawnProbability <= spawnType.objects[randomObjectIndex].placementProbability)
                        Instantiate(spawnType.objects[randomObjectIndex].prefab, (Vector2)spawnType.objectSpawnPoints[i].position + spawnType.objects[randomObjectIndex].spawnOffset, Quaternion.identity, spawnType.objectsParent);
                }
            }
        }

        player.OnPlayerRespawn += Refresh;
        player.OnPlayerTeleported += Refresh;

        CheckForUpgrades();
    }

    void CheckForUpgrades()
    {
        //The Giving Gem
        if(gameManager.currentUser.equippedUpgrades.Contains(8902))
            doubleGems = true;
    }

    void Update()
    {
        if(objectZones.Count > 0)
        {
            if(_checkTimer > 0)
                _checkTimer -= Time.deltaTime;
            else
                Refresh();
        }
        
        float playerXPositionAbs = Mathf.Abs(levelFinish.position.x - player.transform.position.x);

        if(playerXPositionAbs < xDistance && player.transform.position.y > levelFinish.position.y - 1 && !playerEntered)
        {
            if(player.transform.position.y > levelFinish.position.y - 1 + maxDetectionHeight)
                return;

            finishedLevel = true;

            if(localWorldManager.world != LocalWorldManager.World.Tutorial)
            {
                if(currentScene >= gameManager.currentUser.levelsCompleted)
                    gameManager.currentUser.levelsCompleted = currentScene + 1;
            }

            OnLevelFinished?.Invoke();
            GameHUD.Instance.FinishLevel();
            gameManager.SaveUserData();
            
            playerEntered = true;
        }
    }

    void Refresh()
    {
        foreach(var objectZone in objectZones)
        {
            if(CameraInZone(objectZone.position, objectZone.size))
            {
                if(objectZone.objectsToToggle[0].activeInHierarchy == false)
                    for(int i = 0; i < objectZone.objectsToToggle.Count; i++)
                        objectZone.objectsToToggle[i].SetActive(true);
            }
            else
            {
                if(objectZone.objectsToToggle[0].activeInHierarchy == true)
                    for(int i = 0; i < objectZone.objectsToToggle.Count; i++)
                        objectZone.objectsToToggle[i].SetActive(false);
            }
        }

        _checkTimer = checkFrequency;
    }

    bool CameraInZone(Vector2 position, Vector2 zoneSize)
    {
        if(Mathf.Abs(mainCamera.transform.position.x - position.x) <= (CameraWidth() / 2 + zoneSize.x / 2 + extraZoneBuffer) && Mathf.Abs(mainCamera.transform.position.y - position.y) <= (CameraHeight() / 2 + zoneSize.y / 2 + extraZoneBuffer))
            return true;
        else
            return false;
    }

    float CameraHeight()
    {
        return mainCamera.orthographicSize * 2;
    }

    float CameraWidth()
    {
        return CameraHeight() * mainCamera.aspect;
    }
    
    //TEST THIS AND TEST IF SCREENSHOTS TEXT IS WORKING RIGHT
    public void AddGems(int gemsToGive)
    {
        if(doubleGems)
            gems += gemsToGive * 2;
        else
            gems += gemsToGive;

        gameManager.currentUser.gems += gemsToGive;
        gameManager.currentUser.totalGemsCollected += gemsToGive;
        StatsHUD.Instance.UpdateGemsCounter(gems);
    }

    public bool FinishedLevel()
    {
        return finishedLevel;
    }
    
    public void Respawn()
    {
        player.transform.position = new Vector2(currentSpawnPoint.x, currentSpawnPoint.y - 0.125f);
    }

    public void UpdateSpawnPoint(Vector2 position)
    {
        currentSpawnPoint = position;
    }
    
	Vector2 SnapVector(Vector2 v)
	{
		return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
	}

    #if UNITY_EDITOR
    void OnValidate()
	{
		if(objectZones.Count == 0)
			return;
			
		for(int i = 0; i < objectZones.Count; i++)
		{
			objectZones[i].position = SnapVector(objectZones[i].position);
			objectZones[i].size = SnapVector(objectZones[i].size);	
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(levelStart.position, new Vector3(2, 2, 1));
        Gizmos.DrawCube(levelFinish.position, new Vector3(2, 2, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector3(levelFinish.position.x, levelFinish.position.y - 1, 0), Vector3.up + new Vector3(0, maxDetectionHeight - 1, 0));
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;

        Handles.Label(new Vector2(levelStart.position.x, levelStart.position.y + 2), "Level Start", style);
        Handles.Label(new Vector2(levelFinish.position.x, levelFinish.position.y + 2), "Level Finish", style);

        Gizmos.color = new Color(0, 1, 1, 0.5f);
        GUIStyle smallStyle = new GUIStyle();
        smallStyle.normal.textColor = Color.white;
        smallStyle.fontSize = 8;
        smallStyle.alignment = TextAnchor.MiddleCenter;
        smallStyle.fontStyle = FontStyle.Bold;

        if(spawnTypes.Count > 0)
        {
            foreach(var spawnType in spawnTypes)
            {
                if(spawnType.objectSpawnPoints.Count > 0)
                {
                    for(int i = 0; i < spawnType.objectSpawnPoints.Count; i++)
                    {
                        Handles.Label(new Vector2(spawnType.objectSpawnPoints[i].position.x, spawnType.objectSpawnPoints[i].position.y + 2), spawnType.name + " Spawn Point", smallStyle);
                        Gizmos.DrawCube(spawnType.objectSpawnPoints[i].position, new Vector3(2, 2, 1));
                    }
                }
            }
        }

        if(objectZones.Count > 0)
        {
            for(int i = 0; i < objectZones.Count; i++)
            {   
                GUIStyle largeStyle = new GUIStyle();
                largeStyle.normal.textColor = Color.white;
                largeStyle.fontSize = 20;
                largeStyle.alignment = TextAnchor.MiddleCenter;
                largeStyle.fontStyle = FontStyle.Bold;
                Handles.Label(new Vector3(objectZones[i].position.x, objectZones[i].position.y + objectZones[i].size.y / 2 + 5, 0), "Zone " + (i + 1).ToString(), largeStyle);
                
                if(mainCamera != null && CameraInZone(objectZones[i].position, objectZones[i].size))
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.cyan;
                
                //Left
                Gizmos.DrawLine(new Vector3(objectZones[i].position.x - objectZones[i].size.x / 2, objectZones[i].position.y), new Vector3(objectZones[i].position.x - objectZones[i].size.x / 2 - extraZoneBuffer, objectZones[i].position.y));
                //Right
                Gizmos.DrawLine(new Vector3(objectZones[i].position.x + objectZones[i].size.x / 2, objectZones[i].position.y), new Vector3(objectZones[i].position.x + objectZones[i].size.x / 2 + extraZoneBuffer, objectZones[i].position.y));
                //Up
                Gizmos.DrawLine(new Vector3(objectZones[i].position.x, objectZones[i].position.y + objectZones[i].size.y / 2), new Vector3(objectZones[i].position.x, objectZones[i].position.y + objectZones[i].size.y / 2 + extraZoneBuffer));
                //Down
                Gizmos.DrawLine(new Vector3(objectZones[i].position.x, objectZones[i].position.y - objectZones[i].size.y / 2), new Vector3(objectZones[i].position.x, objectZones[i].position.y - objectZones[i].size.y / 2 - extraZoneBuffer));
                
                Gizmos.DrawWireCube(objectZones[i].position, objectZones[i].size);
            }
        }
        //Gizmos.DrawSphere(new Vector3(transform.position.x - xDistance, transform.position.y, 0), 0.1f);
    }
    #endif

    [Serializable]
    public class SpawnType
    {
        public string name;
        public List<Transform> objectSpawnPoints = new List<Transform>();
        public List<LevelObject> objects = new List<LevelObject>();
        public Transform objectsParent;
    }

    [Serializable]
    public class LevelObject
    {
        public int placementProbability;
        public GameObject prefab;
        public Vector2 spawnOffset;
    }

    [Serializable]
    public class ObjectZone
    {
        public Vector2 position;
        public Vector2 size;
        public List<GameObject> objectsToToggle = new List<GameObject>();
    }
}