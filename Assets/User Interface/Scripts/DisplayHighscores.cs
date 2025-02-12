﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays all highscores in the leaderboard menu.
/// </summary>
public class DisplayHighscores : MonoBehaviour 
{
    public TMPro.TextMeshProUGUI[] rNames = null;
    public TMPro.TextMeshProUGUI[] rScores = null;
    HighScores myScores;

    void Start() //Fetches the Data at the beginning
    {

        
        for (int i = 0; i < rNames.Length;i ++)
        {
            if(rNames[i] != null)
            {
                rNames[i].text = i + 1 + ". Fetching...";
            }
        }
        myScores = GetComponent<HighScores>();
        StartCoroutine("RefreshHighscores");
    }


    public void SetScoresToMenu(PlayerScore[] highscoreList) //Assigns proper name and score for each text value
    {
        for (int i = 0; i < rNames.Length;i ++)
        {
            if (rNames[i] != null)
            {
                rNames[i].text = i + 1 + ". ";
                Debug.Log(rNames[i].text);
              
                if (highscoreList.Length > i)
                {
                    rScores[i].text = highscoreList[i].score.ToString();
                    rNames[i].text = highscoreList[i].username;
                }
            }
           
        }
    }
    IEnumerator RefreshHighscores() //Refreshes the scores every 30 seconds
    {
        while(true)
        {
            myScores.DownloadScores();
            yield return new WaitForSeconds(1f);
        }
    }
}
