using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreInfo
{
    public string Name { get; set; }
    public int Score { get; set; }

    public ScoreInfo(string name, int score)
    {
        Name = name;
        Score = score;
    }
}

public static class StaticScores {

    public static List<ScoreInfo> scoreList = new List<ScoreInfo>();

    /// <summary>
    /// Saves the scores into playerprefs
    /// Only saves top 10 scores deletes the rest
    /// </summary>
    /// <param name="name">Player name</param>
    /// <param name="score">Score to submit for possible highscore</param>
    public static void SaveScore(string name, int score)
    {
        scoreList.Add(new ScoreInfo(name, score));
        scoreList = scoreList.OrderByDescending(x => x.Score).ToList();
        if (scoreList.Count > 10)
        {
            scoreList.RemoveRange(10, scoreList.Count-10);
        }
        string highString = "";
        foreach (var item in scoreList)
        {
            highString = highString + item.Name + "," + item.Score + ";";
        }
        PlayerPrefs.SetString("HighscoreList", highString);
        //Debug.Log(highString.Split(';')[0].Split(',')[1]);
    }

    /// <summary>
    /// Loads the scores from playerprefs
    /// </summary>
    public static void LoadScores()
    {
        string highString = "";
        if (PlayerPrefs.HasKey("HighscoreList"))
        {
            highString = PlayerPrefs.GetString("HighscoreList");

            foreach (var item in highString.Remove(highString.Length-1).Split(';'))
            {
                var scoreInfo = item.Split(',');
                scoreList.Add(new ScoreInfo(scoreInfo[0], Int32.Parse(scoreInfo[1])));
            }
        }

    }
}
