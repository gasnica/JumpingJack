using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class jjLevelMgr : MonoBehaviour {

    public int numLives = 6;
    public int score = 0;
    public int highScore = 0;

    private bool timedNextLevel = false;

    void Awake()
    {
        jjMain.levelMgr = this;
    }

	// Use this for initialization
	void Start ()
    {
        levelTemplates = new GameObject[(int)Level.Type.NumTypes];
        for (int i = 0; i < (int)Level.Type.NumTypes; i++)
            levelTemplates[i] = Resources.Load(string.Format("Prefabs/{0}", ((Level.Type)i).ToString())) as GameObject;

        for (int i = 0; i < 20; i++)
        {
            levels.Add(new Level(i != 0 ? Level.Type.NextLevel : Level.Type.WelcomeSplash, i));
            levels.Add(new Level(Level.Type.Level, i));
        }

        StartNextLevel();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!timedNextLevel) // don't conflict with timed invoke of StartNextLevel()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                StartNextLevel();
            }

            if (levelIdx < 0 || levels[levelIdx].type != Level.Type.Level)
            {
                // some splash or intermission screen
                if (Input.GetKeyDown(KeyCode.Return)) StartNextLevel();
            }
        }
    }

    void StartNextLevel()
    {
        timedNextLevel = false;
        Destroy(currentLevel);
        levelIdx++;
        if (levelIdx == levels.Count)
        {
            levelIdx = 0;
        }

        Level level = levels[levelIdx];
        currentLevel = GameObject.Instantiate(levelTemplates[(int)level.type]);
        switch(level.type)
        {
            case Level.Type.Level: currentLevel.GetComponent<jjLevel>().initialNumHazards = level.numHazards; break;
            case Level.Type.WelcomeSplash: timedNextLevel = true; Invoke("StartNextLevel", 1); break;
        }
    }

    void GameOver()
    {
        Destroy(currentLevel);
        levelIdx = 0;
        if (score > highScore)
        {
            highScore = score;
            currentLevel = GameObject.Instantiate(levelTemplates[(int)Level.Type.GameOverNewHigh]);
        }
        else
        {
            currentLevel = GameObject.Instantiate(levelTemplates[(int)Level.Type.GameOver]);
        }
        numLives = 6;
        score = 0;
    }

    private int levelIdx = -1;
    private GameObject currentLevel;
    private GameObject[] levelTemplates;

    private List<Level> levels = new List<Level>();



    class Level
    {
        public Level(Type type, int numHazards) { this.type = type;  this.numHazards = numHazards; }
        public enum Type { WelcomeSplash, Level, NextLevel, GameOver, GameOverNewHigh, NumTypes };
        public Type type;
        public int numHazards;
    }


}
