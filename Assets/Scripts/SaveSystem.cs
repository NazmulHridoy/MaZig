using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardState
{
    public int cardValue;
    public bool isFlipped;
    public bool isMatched;
    public Vector3 position;
}

[Serializable]
public class GameData
{
    public int score;
    public int moves;
    public float gameTime;
    public int matchedPairs;
    public int totalPairs;
    public int rows;
    public int columns;
    public List<CardState> cardStates;
    public string saveDate;
}

public class SaveSystem : MonoBehaviour
{
    private const string SAVE_KEY = "MatchCardGameSave";

    public void SaveGame(GameData gameData)
    {
        try
        {
            gameData.saveDate = DateTime.Now.ToString();
            string json = JsonUtility.ToJson(gameData, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("Game saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save game: " + e.Message);
        }
    }

    public GameData LoadGame()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                GameData gameData = JsonUtility.FromJson<GameData>(json);
                Debug.Log("Game loaded successfully");
                return gameData;
            }
            else
            {
                Debug.Log("No save data found");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load game: " + e.Message);
            return null;
        }
    }

    public void DeleteSave()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Debug.Log("Save data deleted");
        }
    }

    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
}
