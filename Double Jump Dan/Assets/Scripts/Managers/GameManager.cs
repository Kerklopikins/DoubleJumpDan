using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float cursorAcceleration; /////SAVE 
    public float cursorDeceleration; ////SAVE
    //Game Data
    [HideInInspector] public User currentUser;
    [HideInInspector] public List<User> users = new List<User>();
    [HideInInspector] public float sfxVolume = 1;
    [HideInInspector] public float musicVolume = 1;
	[HideInInspector] public int screenResolution = -1;
    [HideInInspector] public int frameRate = -1;
    [HideInInspector] public bool vSync = true;
    [HideInInspector] public bool cameraShake = true;
    [HideInInspector] public bool showPerformanceData = false;
    [HideInInspector] public float aimSensitivity = 0.5f; 
    [HideInInspector] public float cursorSensitivity = 0.5f; 
    [HideInInspector] public bool controllerVibration = true;
    [HideInInspector] public bool swapJoysticks = false;
    [HideInInspector] public bool useDPad = false;
    [HideInInspector] public bool lockAiming = false;
    [HideInInspector] public string inputBindings = "";
    [HideInInspector] public bool postProcessing = true;
    [HideInInspector] public bool distortionEffects = true;
    [HideInInspector] public bool weatherEffects = true;
    string folderPath;
    public static bool died;
    bool inMainMenu;
    MainMenuManager mainMenuManager;

    void Awake()
    {
        Instance = this;
        folderPath = Application.persistentDataPath;

        if(SceneManager.GetActiveScene().name == "Main Menu")
        {
            inMainMenu = true;
            mainMenuManager = GameObject.FindWithTag("Main Menu").GetComponent<MainMenuManager>();
        }
        
        if(!File.Exists(folderPath + "/GameData.json"))
        {
            currentUser = new User();
            SaveData();
        }
        else
        {
            LoadData();
            LoadUserData();
        }        
    }

    public bool InMainMenu()
    {
        return inMainMenu;
    }
    
    void Update()
    {
        if(users.Count > 0)
            currentUser.totalPlaytime += Time.unscaledDeltaTime;
    }
    
    #region Encryption
    public static string Encrypt(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32)); // 256-bit key

        using(Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.GenerateIV();

            using(var encryptor = aes.CreateEncryptor())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(encrypted);
            }
        }
    }

    public static string Decrypt(string cipherText, string key)
    {
        var parts = cipherText.Split(':');

        byte[] iv = Convert.FromBase64String(parts[0]);
        byte[] data = Convert.FromBase64String(parts[1]);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32));

        using(Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;

            using(var decryptor = aes.CreateDecryptor())
            {
                byte[] decrypted = decryptor.TransformFinalBlock(data, 0, data.Length);
                return Encoding.UTF8.GetString(decrypted);
            }
        }
    }

    #endregion

    #region GameData
    public void SaveData()
    {
        GameData gameData = new GameData();

        //Game Data
        int index = users.FindIndex(users => users.userID == currentUser.userID);

        if(index >= 0)
            users[index] = currentUser;
        
        gameData.currentUser = currentUser;
        gameData.users = users;
        gameData.sfxVolume = sfxVolume;
        gameData.musicVolume = musicVolume;
		gameData.screenResolution = screenResolution;
        gameData.frameRate = frameRate;
        gameData.vSync = vSync;
        gameData.cameraShake = cameraShake;
        gameData.showPerformanceData = showPerformanceData;
        gameData.aimSensitivity = aimSensitivity;
        gameData.cursorSensitivity = cursorSensitivity;
        gameData.controllerVibration = controllerVibration;
        gameData.swapJoysticks = swapJoysticks;
        gameData.useDPad = useDPad;
        gameData.lockAiming = lockAiming;
        gameData.inputBindings = inputBindings;
        gameData.postProcessing = postProcessing;
        gameData.distortionEffects = distortionEffects;
        gameData.weatherEffects = weatherEffects;

        string json = JsonUtility.ToJson(gameData);
        string encrypted = Encrypt(json, "5a82be8ec0fdafa41013f6ac33b109");
        File.WriteAllText(folderPath + "/GameData.json", encrypted);
    }

    public void LoadData()
    {
		if(File.Exists(folderPath + "/GameData.json"))
        {
            string encrypted = File.ReadAllText(folderPath + "/GameData.json");
            string json = Decrypt(encrypted, "5a82be8ec0fdafa41013f6ac33b109");
            GameData gameData = JsonUtility.FromJson<GameData>(json);

            //Game Data
            users = gameData.users;

            int index = users.FindIndex(users => users.userID == gameData.currentUser.userID);

            if(index >= 0)
                currentUser = users[index];

            sfxVolume = gameData.sfxVolume;
            musicVolume = gameData.musicVolume;
			screenResolution = gameData.screenResolution;
            frameRate = gameData.frameRate;
            vSync = gameData.vSync;
            cameraShake = gameData.cameraShake;
            showPerformanceData = gameData.showPerformanceData;
            aimSensitivity = gameData.aimSensitivity;
            cursorSensitivity = gameData.cursorSensitivity;
            controllerVibration = gameData.controllerVibration;
            swapJoysticks = gameData.swapJoysticks;
            useDPad = gameData.useDPad;
            lockAiming = gameData.lockAiming;
            inputBindings = gameData.inputBindings;
            postProcessing = gameData.postProcessing;
            distortionEffects = gameData.distortionEffects;
            weatherEffects = gameData.weatherEffects;
        }
    }
    #endregion

    #region UserData
    public void SaveUserData()
    {
        User user = new User();

        //User Data Here
        user.userID = currentUser.userID;
        user.userName = currentUser.userName;
        user.userColorIndex = currentUser.userColorIndex;
        user.gems = currentUser.gems;
        user.ownedHats = currentUser.ownedHats;
        user.ownedGuns = currentUser.ownedGuns;
		user.ownedSkins = currentUser.ownedSkins;
        user.ownedUpgrades = currentUser.ownedUpgrades;
        user.hatID = currentUser.hatID;
        user.gunID = currentUser.gunID;
		user.skinID = currentUser.skinID;
        user.equippedUpgrades = currentUser.equippedUpgrades;
        user.customSkinData = currentUser.customSkinData;
		user.levelsCompleted = currentUser.levelsCompleted;
        user.hash = (user.gems * 17 + 9).ToString();
        user.totalEnemiesKilled = currentUser.totalEnemiesKilled;
        user.totalDeaths = currentUser.totalDeaths;
        user.totalGemsCollected = currentUser.totalGemsCollected;
        user.totalPlaytime = currentUser.totalPlaytime;
        user.totalJumps = currentUser.totalJumps;

        string json = JsonUtility.ToJson(user);
        string encrypted = Encrypt(json, "5a82be8ec0fdafa41013f6ac33b109");

        File.WriteAllText(folderPath + "/UserData" + currentUser.userID + ".json", encrypted);
        File.WriteAllText(folderPath + "/UserData" + currentUser.userID + ".bak", encrypted);
    }

    public void LoadUserData()
    {
		if(File.Exists(folderPath + "/UserData" + currentUser.userID + ".json"))
        {            
            string encrypted = File.ReadAllText(folderPath + "/UserData" + currentUser.userID + ".json");
            string json = Decrypt(encrypted, "5a82be8ec0fdafa41013f6ac33b109");
            User user = JsonUtility.FromJson<User>(json);

            if(user.hash == (user.gems * 17 + 9).ToString())
            {
                currentUser.userID = user.userID;
                currentUser.userName = user.userName;
                currentUser.userColorIndex = user.userColorIndex;
                currentUser.gems = user.gems;
                currentUser.ownedHats = user.ownedHats;
                currentUser.ownedGuns = user.ownedGuns;
                currentUser.ownedSkins = user.ownedSkins;
                currentUser.ownedUpgrades = user.ownedUpgrades;
                currentUser.hatID = user.hatID;
                currentUser.gunID = user.gunID;
                currentUser.skinID = user.skinID;
                currentUser.equippedUpgrades = user.equippedUpgrades;
                currentUser.customSkinData = user.customSkinData;
                currentUser.levelsCompleted = user.levelsCompleted;
                currentUser.totalEnemiesKilled = user.totalEnemiesKilled;
                currentUser.totalDeaths = user.totalDeaths;
                currentUser.totalGemsCollected = user.totalGemsCollected;
                currentUser.totalPlaytime = user.totalPlaytime;
                currentUser.totalJumps = user.totalJumps;

                return;
            }
        }

        if(File.Exists(folderPath + "/UserData" + currentUser.userID + ".bak"))
        {
            string encryptedBak = File.ReadAllText(folderPath + "/UserData" + currentUser.userID + ".bak");
            string jsonBak = Decrypt(encryptedBak, "5a82be8ec0fdafa41013f6ac33b109");
            User userBak = JsonUtility.FromJson<User>(jsonBak);

            if(userBak.hash == (userBak.gems * 17 + 9).ToString())
            {
                if(inMainMenu)
                    mainMenuManager.TamperedUserFile("User file tampered with, loading backup...");

                currentUser.userID = userBak.userID;
                currentUser.userName = userBak.userName;
                currentUser.userColorIndex = userBak.userColorIndex;
                currentUser.gems = userBak.gems;
                currentUser.ownedHats = userBak.ownedHats;
                currentUser.ownedGuns = userBak.ownedGuns;
                currentUser.ownedSkins = userBak.ownedSkins;
                currentUser.ownedUpgrades = userBak.ownedUpgrades;
                currentUser.hatID = userBak.hatID;
                currentUser.gunID = userBak.gunID;
                currentUser.skinID = userBak.skinID;
                currentUser.equippedUpgrades = userBak.equippedUpgrades;
                currentUser.customSkinData = userBak.customSkinData;
                currentUser.levelsCompleted = userBak.levelsCompleted;
                currentUser.totalEnemiesKilled = userBak.totalEnemiesKilled;
                currentUser.totalDeaths = userBak.totalDeaths;
                currentUser.totalGemsCollected = userBak.totalGemsCollected;
                currentUser.totalPlaytime = userBak.totalPlaytime;
                currentUser.totalJumps = userBak.totalJumps;
                
                SaveUserData();
                return;
            }
            else
            {
                if(inMainMenu)
                    mainMenuManager.TamperedUserFile("All user files tampered with, creating new file...");
                
                LoadDefaultUserData();
                SaveUserData();
            }              
        }
    }

    void LoadDefaultUserData()
    {
        currentUser.gems = 0;
        currentUser.ownedHats.Clear();
        currentUser.ownedGuns.Clear();
		currentUser.ownedSkins.Clear();
        currentUser.ownedUpgrades.Clear();
		currentUser.levelsCompleted = 1;
        currentUser.totalEnemiesKilled = 0;
        currentUser.totalDeaths = 0;
        currentUser.totalGemsCollected = 0;
        currentUser.totalPlaytime = 0;
        currentUser.totalJumps = 0;
        currentUser.customSkinData.Clear();

        currentUser.ownedHats.Add(1111);
        currentUser.hatID = 1111;
        currentUser.ownedGuns.Add(1111);
        currentUser.gunID = 1111;
        currentUser.ownedSkins.Add(1111);
        currentUser.skinID = 1111;
        currentUser.equippedUpgrades.Clear();
        currentUser.customSkinData = new List<float> { 0, 0, 0.85f, 0, 0, 0.5f, 0 };
    }

    public void DeleteUserData(int user)
    {
		if(File.Exists(folderPath + "/UserData" + user + ".json"))
			File.Delete(folderPath + "/UserData" + user + ".json");

        if(File.Exists(folderPath + "/UserData" + user + ".bak"))
			File.Delete(folderPath + "/UserData" + user + ".bak");
    }

    #endregion

    public void ResetGame()
    {
        for(int i = 0; i < users.Count; i++)
        {
			if(File.Exists(folderPath + "/UserData" + users[i].userID + ".json"))
				File.Delete(folderPath + "/UserData" + users[i].userID + ".json");
            
            if(File.Exists(folderPath + "/UserData" + users[i].userID + ".bak"))
				File.Delete(folderPath + "/UserData" + users[i].userID + ".bak");
        }

		if(File.Exists(folderPath + "/GameData.json"))
			File.Delete(folderPath + "/GameData.json");
    }

    void OnApplicationQuit()
    {        
        if(users.Count > 0)
        {
            SaveData();
            SaveUserData();
        }
    }
}

[System.Serializable]
public class GameData
{
    public User currentUser;
    public List<User> users = new List<User>();
    public float sfxVolume;
    public float musicVolume;
	public int screenResolution;
    public int frameRate;
    public bool vSync;
    public bool cameraShake;
    public bool showPerformanceData;
    public float aimSensitivity; 
    public float cursorSensitivity; 
    public bool controllerVibration;
    public bool swapJoysticks;
    public bool useDPad;
    public bool lockAiming;
    public string inputBindings;
    public bool postProcessing;
    public bool distortionEffects;
    public bool weatherEffects;
}

[System.Serializable]
public class User
{
    public int userID;
    public string userName;
    public int userColorIndex;
    public int gems;
    public List<int> ownedHats = new List<int> { 1111 };
    public List<int> ownedGuns = new List<int> { 1111 };
	public List<int> ownedSkins = new List<int> { 1111 };
    public List<int> ownedUpgrades = new List<int>();
    public int hatID = 1111;
    public int gunID = 1111;
	public int skinID = 1111;
    public List<int> equippedUpgrades = new List<int>();
    public List<float> customSkinData = new List<float> { 0, 0, 0.85f, 0, 0, 0.5f, 0 };
	public int levelsCompleted = 1;
    public string hash;
    public int totalEnemiesKilled;
    public int totalDeaths;
    public int totalGemsCollected;
    public double totalPlaytime;
    public int totalJumps;
}