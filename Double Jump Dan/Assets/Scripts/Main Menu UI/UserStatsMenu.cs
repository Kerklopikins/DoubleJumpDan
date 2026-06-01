using UnityEngine;
using UnityEngine.UI;
using System;

public class UserStatsMenu : MonoBehaviour
{
    [SerializeField] GameObject statsGameObject;
    [SerializeField] Text statsTitleText;
    [SerializeField] Text enemiesCounterText;
    [SerializeField] Text deathsCounterText;
    [SerializeField] Text gemsCounterText;
    [SerializeField] Text playtimeCounterText;
    [SerializeField] Text jumpsCounterText;

    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        if(statsGameObject.activeInHierarchy)
            playtimeCounterText.text = GetTotalPlaytimeString();
    }

    public void RefreshUserStats()
    {
        statsTitleText.text = gameManager.currentUser.userName + "'s Statistics";
        enemiesCounterText.text = gameManager.currentUser.totalEnemiesKilled.ToString();
        deathsCounterText.text = gameManager.currentUser.totalDeaths.ToString();
        gemsCounterText.text = gameManager.currentUser.totalGemsCollected.ToString();
        jumpsCounterText.text = gameManager.currentUser.totalJumps.ToString();
        
        playtimeCounterText.text = GetTotalPlaytimeString();
    }

    string GetTotalPlaytimeString()
    {
        TimeSpan t = TimeSpan.FromSeconds(gameManager.currentUser.totalPlaytime);

        string formatted = "";

        if(t.Days > 0)
            formatted += t.Days + "d ";
        
        if(t.Hours > 0)
            formatted += t.Hours + "h ";

        if(t.Minutes > 0)
            formatted += t.Minutes + "m ";
        
        formatted += t.Seconds + "s ";

        return formatted;
    }
}