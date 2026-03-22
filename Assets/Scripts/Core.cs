using UnityEngine;
using System.Collections.Generic;
using System;

public class Core : MonoBehaviour
{
    // Gameplay Konstanten
    public const int STAIR_WIDTH = 3;
    public const int STAIR_HEIGHT = 2;
    public const int MIN_COLLISION_OFFSET = 1; // 1 Pixel Minimum für Boden/Wand-Checks
    public const int DIG_RANGE = 2;
    public const int MAX_CLIMB_HEIGHT = STAIR_HEIGHT * 2;
    public const float EXPLOSION_RADIUS = 2f;
    public const int DEFAULT_SKILL_USES = 12;

    [Header("References")]
    [SerializeField] private Transform levelParent;
    [SerializeField] private GameObject guyPrefab;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Level[] allLevels;

    private static Core instance;
    public static Core Instance => instance;

    public event Action<bool> OnGameOver;
    public event Action<bool> OnPauseResume;
    public event Action<Level> OnNewLevelStarted;
    public bool IsGameOver { get => gameover; }
    public AudioManager AM { get => audioManager; }
    public Skill SelectedSkill { get; set; }

    private TerrainManager terrainManager = new();
    private Level currentLevel;
    private int currentLevelIdx;
    private float timeToNextSpawn;
    private List<GuyBehavior> spawnedGuys = new();
    private readonly int[] usagesRemaining = new int[(int)Skill.COUNT];
    private int savedCount;
    private Vector2[] bufferedSpawnPositions;

    public Level[] GetAllLevels() { return allLevels; }

    private bool running = false, gameover = false;
    public bool Running
    {
        get => running; set
        {
            if (!gameover && running != value)
            {
                running = value;
                if (!running)
                {
                    Debug.Log("Game Paused");
                }
                else
                {
                    Debug.Log("Game Resumed");
                }
                OnPauseResume?.Invoke(running);
            }
        }
    }

    void Awake()
    {
        instance = this;
        LoadResources();
        //StartLevel(0);
    }

    public void StartLevel(int idx)
    {
        if (idx >= 0 && allLevels != null && idx < allLevels.Length)
            StartLevel(allLevels[idx]);
        else
            Debug.LogError("Invalid level index: " + idx);
    }

    public Level GetCurrentLevel() { return currentLevel; }
    public Level GetNextLevel()
    {
        int idx = currentLevelIdx + 1;
        if (idx < allLevels.Length && idx >= 0)
            return allLevels[idx];
        return null;
    }

    public void ClearLevel()
    {
        running = false;
        gameover = true;
        spawnedGuys.Clear();
        foreach (Transform child in levelParent)
        {
            Destroy(child.gameObject);
        }
    }
    public void StartLevel(Level level)
    {
        Debug.Log("Start Level: " + level);
        // Clear old level
        ClearLevel();

        currentLevel = level;
        currentLevelIdx = level.Index;

        // Create Level
        GameObject levelObject = SpawnObject(level.Prefab, 0, 0);
        FindSpawns();

        //  Initialize Terrain Manager
        terrainManager.LoadLevel(levelObject, currentLevel);

        // Reset game parameters
        savedCount = 0;
        for (int i = 0; i < usagesRemaining.Length; i++)
            usagesRemaining[i] = level.SkillUsages[i];
        timeToNextSpawn = level.SpawnDelay;
        gameover = false;// Always set gameover first
        Running = true;
        // Notify listeners
        OnNewLevelStarted?.Invoke(currentLevel);
    }

    private void FindSpawns()
    {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("Respawn");
        bufferedSpawnPositions = new Vector2[spawns.Length];
        for (int i = 0; i < spawns.Length; i++)
        {
            bufferedSpawnPositions[i] = new Vector2(spawns[i].transform.localPosition.x, spawns[i].transform.localPosition.y);
            spawns[i].tag = "Untagged";// remove tag to not find it again
        }
    }

    void Update()
    {
        // Main loop
        if (Running && !gameover)
        {
            float dT = Mathf.Clamp(Time.deltaTime, 0.001f, 0.05f);
            if (timeToNextSpawn <= 0)
            {
                SpawnGuy();
                timeToNextSpawn = currentLevel.SpawnDelay;
            }
            else
            {
                timeToNextSpawn -= dT;
            }
            bool allDead = true;
            foreach (GuyBehavior guy in spawnedGuys)
            {
                if (!guy.IsDead)
                {
                    allDead = false;
                    guy.Act(dT);
                    guy.Move(dT);
                }
            }
            if (allDead && spawnedGuys.Count == currentLevel.TotalGuys)
            {
                gameover = true;
                Invoke(nameof(GameOver), 2f);
            }
        }
    }

    void LateUpdate()
    {
        if (Running)
        {
            terrainManager.RenderUpdate();
        }
    }

    private void GameOver()
    {
        gameover = true;
        bool win = GetSavedCount() >= currentLevel.MinGuysSavedToWin;
        if (win)
        {
            // Unlock next level
            Level nextLevel = GetNextLevel();
            if (nextLevel != null) nextLevel.IsUnlocked = true;
        }
        OnGameOver?.Invoke(win);
    }

    public int GetSkillUsages(Skill s)
    {
        return usagesRemaining[(int)s];
    }
    public void ReduceSkillUsages(Skill s)
    {
        if (s != Skill.NONE && s != Skill.COUNT)
            usagesRemaining[(int)s]--;
    }

    public void GuyClicked(GuyBehavior guy)
    {
        Debug.Log("Clicked at Guy: " + guy);
        AM.PlaySound(SoundEffect.CLICK);
        if (SelectedSkill == Skill.NONE
            || GetSkillUsages(SelectedSkill) <= 0)
            return;
        if (guy.GiveSkill(SelectedSkill))
        {
            ReduceSkillUsages(SelectedSkill);
        }
    }

    public void KillAll()
    {
        foreach (GuyBehavior guy in spawnedGuys)
        {
            guy.GiveSkill(Skill.EXPLODER);
        }
    }

    void SpawnGuy()
    {
        foreach (Vector2 spawnPos in bufferedSpawnPositions)
        {
            if (spawnedGuys.Count < currentLevel.TotalGuys)
            {
                GameObject l = Instantiate(guyPrefab, levelParent);
                float x = spawnPos.x;
                float y = spawnPos.y;
                l.transform.localPosition = new Vector3(x, y, 0);
                GuyBehavior guy = l.GetComponent<GuyBehavior>();
                if (guy == null) guy = l.AddComponent<GuyBehavior>();
                guy.SetTerrain(terrainManager);
                spawnedGuys.Add(guy);
            }
        }
    }
    GameObject SpawnObject(GameObject prefab, float spawnX, float spawnY)
    {
        GameObject l = Instantiate(prefab, levelParent);
        float x = spawnX;
        float y = spawnY;
        l.transform.localPosition = new Vector3(x, y, 0);
        return l;
    }

    public int GetSpawnedCount()
    {
        return spawnedGuys.Count;
    }

    public int GetSavedCount()
    {
        return savedCount;
    }
    public void AddSavedCount()
    {
        savedCount++;
    }
    void LoadResources()
    {
        // Load Level Objects
        //allLevels = Resources.LoadAll<Level>("Level");
        // Init levels
        for (int i = 0; i < allLevels.Length; i++)
        {
            allLevels[i].Index = i;
            allLevels[i].IsUnlocked = i == 0;
            allLevels[i].IsUnlocked = true;// Only for debugging
        }
        Debug.Log("Initialized " + allLevels.Length + " Levels");
    }
}