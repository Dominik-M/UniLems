#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class LevelProcessor : EditorWindow
{
    private const string SAVE_PATH = "Assets/Resources/Textures/LogicMasks/";
    private const string VGMAPS_PATH = "Assets/Resources/Textures/visual/";
    private const string LEVEL_ASSET_SAVE_PATH = "Assets/Resources/Level/";
    private const string LEVEL_PREFABS_SAVE_PATH = "Assets/Prefabs/Level/";
    private const string DEFAULT_LEVEL_PREFAB_PATH = "Assets/Prefabs/Level/Level 0.prefab";

    [MenuItem("Tools/Generate Logic Masks")]
    public static void GenerateMasks()
    {
        // 0. Alle Color codes loggen 
        Debug.Log("All Colors:\r\n" +
            $"ColorEmpty = {Level.ColorEmpty}\r\n" +
            $"ColorDirt = {Level.ColorDirt}\r\n" +
            $"ColorSteel = {Level.ColorSteel}\r\n" +
            $"ColorFire = {Level.ColorFire}\r\n" +
            $"ColorWater = {Level.ColorWater}\r\n" +
            $"ColorOneWayLeft = {Level.ColorOneWayLeft}\r\n" +
            $"ColorOneWayRight = {Level.ColorOneWayRight}\r\n"
            );

        // 1. Sicherstellen, dass der Zielordner existiert
        if (!Directory.Exists(SAVE_PATH))
            Directory.CreateDirectory(SAVE_PATH);

        // 2. Alle Level-ScriptableObjects in Resources finden
        Level[] allLevels = Resources.LoadAll<Level>("Level");

        if (allLevels.Length == 0)
        {
            Debug.LogWarning("Keine Level ScriptableObjects in 'Resources/Level' gefunden!");
            return;
        }

        foreach (Level level in allLevels)
        {
            if (level.VisualTexture == null)
            {
                Debug.LogWarning($"Level {level.name} hat keine Visual Texture zugewiesen.");
                continue;
            }

            level.GenerateMask(SAVE_PATH);
        }

        AssetDatabase.Refresh();
        Debug.Log("Logic Mask Generierung abgeschlossen.");
    }
    [MenuItem("Tools/Generate Level Assets")]
    public static void GenerateLevelAssets()
    {
        // 1. Suche alle PNGs
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { VGMAPS_PATH });
        int skippedCount = 0;
        int createdCount = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string guid = guids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D mapTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            string assetName = "Level " + i;

            // Pfade für die neuen Assets definieren
            string newPrefabPath = $"{LEVEL_PREFABS_SAVE_PATH}/{assetName}.prefab";
            string soPath = $"{LEVEL_ASSET_SAVE_PATH}/{assetName}.asset";

            // --- PRÜFUNG: Existiert eines der Assets bereits? ---
            bool prefabExists = AssetDatabase.LoadMainAssetAtPath(newPrefabPath) != null;
            bool soExists = AssetDatabase.LoadMainAssetAtPath(soPath) != null;

            if (prefabExists || soExists)
            {
                Debug.LogWarning($"Übersprungen: Assets für '{assetName}' existieren bereits.");
                skippedCount++;
                continue; // Springe zum nächsten Asset in der Schleife
            }

            // --- 3. & 4. Prefab Kopie erstellen ---
            if (AssetDatabase.CopyAsset(DEFAULT_LEVEL_PREFAB_PATH, newPrefabPath))
            {
                // Das neue Prefab-Asset als GameObject laden
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath);

                if (prefabAsset != null)
                {
                    // Den SpriteRenderer auf dem Root oder in Kindern finden
                    SpriteRenderer renderer = prefabAsset.GetComponentInChildren<SpriteRenderer>();

                    if (renderer != null)
                    {
                        // Die Textur als Sprite laden
                        // WICHTIG: Die vgmap muss im Editor als 'Sprite' importiert sein!
                        Sprite mapSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                        if (mapSprite != null)
                        {
                            // Den Sprite zuweisen
                            renderer.sprite = mapSprite;

                            // Änderungen am Prefab-Asset dauerhaft speichern
                            EditorUtility.SetDirty(prefabAsset);
                            PrefabUtility.SavePrefabAsset(prefabAsset);

                            Debug.Log($"Sprite für {assetName} erfolgreich gesetzt.");
                        }
                        else
                        {
                            Debug.LogError($"Konnte Sprite für {path} nicht laden. Ist der Texture Type auf 'Sprite (2D and UI)' gestellt?");
                        }
                    }
                }
            }

            // --- 5. Scriptable Object erstellen ---
            // 'LevelData' muss deine SO-Klasse sein
            Level newLevelSO = ScriptableObject.CreateInstance<Level>();
            newLevelSO.VisualTexture = mapTex;
            newLevelSO.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath);

            AssetDatabase.CreateAsset(newLevelSO, soPath);
            createdCount++;
        }

        // 6. Abschluss
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Generierung abgeschlossen. Erstellt: {createdCount}, Übersprungen: {skippedCount}.");
    }

}
#endif