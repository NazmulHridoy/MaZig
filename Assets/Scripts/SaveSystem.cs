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
    public int clickCount;
    public float gameTime;
    public int matchedPairs;
    public int totalPairs;
    public int rows;
    public int columns;
    public int themeIndex;
    public List<CardState> cardStates;
    public string saveDate;
    
    // Combo system data
    public int comboCount;
    public int lastMatchMove;
}

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem instance;
    private const string SAVE_KEY = "MatchCardGameSave";
    
    public static SaveSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SaveSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SaveSystem");
                    instance = go.AddComponent<SaveSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

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
