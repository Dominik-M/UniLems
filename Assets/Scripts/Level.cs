using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ExtractionMode { ColorMatch, Threshold, AllNonBlack }

[CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
public class Level : ScriptableObject
{
    // Color Codings
    public static readonly Color32 ColorEmpty = Color.black;
    public static readonly Color32 ColorDirt = Color.yellow;
    public static readonly Color32 ColorSteel = Color.purple;
    public static readonly Color32 ColorFire = Color.red;
    public static readonly Color32 ColorWater = Color.blue;
    public static readonly Color32 ColorOneWayLeft = Color.darkGreen;
    public static readonly Color32 ColorOneWayRight = Color.lightGreen;

    [Header("Parameters")]
    [SerializeField] private bool locked;
    [SerializeField] private int totalGuys;
    [SerializeField] private float spawnDelay;
    [SerializeField] private int minGuysSavedToWin;
    [SerializeField] private int[] skillUsages = new int[(int)Skill.COUNT];
    [SerializeField] private GameObject levelPrefab;

    [Header("Music Soundclip")]
    [SerializeField] private AudioClip bgm;

    [Header("Textures")]
    [SerializeField] private Texture2D visualTexture;
    [SerializeField] private Texture2D logicMask;

    [Header("Dimensions")]
    [SerializeField] private int width = 640;
    [SerializeField] private int height = 400;

    [Header("Texture Conversion Settings")]
    [SerializeField] private ExtractionMode mode = ExtractionMode.AllNonBlack;
    [SerializeField] private float threshold = 0.1f; // Für Helligkeits-Check

    // Getter
    public int TotalGuys => totalGuys;
    public float SpawnDelay => spawnDelay;
    public int MinGuysSavedToWin => minGuysSavedToWin;
    public Texture2D VisualTexture { get => visualTexture; set => visualTexture = value; }
    public Texture2D LogicMask => logicMask;
    public int Width => width;
    public int Height => height;
    public int[] SkillUsages => skillUsages;
    public AudioClip Music => bgm;
    public GameObject Prefab { get => levelPrefab; set => levelPrefab = value; }

    public bool IsUnlocked { get; set; }
    public int Index { get; set; }

    public void GenerateMask(string savepath)
    {
#if UNITY_EDITOR
        if (!locked)
        {
            Texture2D visual = VisualTexture;

            // WICHTIG: Textur muss Read/Write enabled sein
            MakeTextureReadable(visual);

            width = visual.width;
            height = visual.height;

            logicMask = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32[] visualPixels = visual.GetPixels32();
            Color32[] logicPixels = new Color32[visualPixels.Length];

            for (int i = 0; i < visualPixels.Length; i++)
            {
                Color32 v = visualPixels[i];
                logicPixels[i] = ToLogicColor(v);
            }

            logicMask.SetPixels32(logicPixels);
            logicMask.Apply();

            // Als PNG speichern
            byte[] bytes = logicMask.EncodeToPNG();
            string fileName = name + "_LogicMask.png";
            string fullPath = savepath + fileName;
            File.WriteAllBytes(fullPath, bytes);

            // Asset Datenbank aktualisieren, um das neue PNG zu registrieren
            AssetDatabase.ImportAsset(fullPath);

            // Das neue Asset dem ScriptableObject zuweisen
            logicMask = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            EditorUtility.SetDirty(this);
            Debug.Log($"Logic Mask für {name} erstellt und verknüpft.");
        }
        else
        {
            Debug.Log($"{name} ist schreibgeschützt.");
        }
#endif
    }

    private Color32 ToLogicColor(Color32 v)
    {
        Color32 l = ColorEmpty;
        switch (mode)
        {
            case ExtractionMode.AllNonBlack:
                // Alles was nicht (fast) schwarz ist und nicht transparent
                if (v.r > 5 || v.g > 5 || v.b > 5)
                    l = ColorDirt;
                break;
            case ExtractionMode.Threshold:
                float brightness = (v.r + v.g + v.b) / (3f * 255f);
                if (brightness > threshold)
                    l = ColorDirt;
                break;
            case ExtractionMode.ColorMatch:
                // TODO Farbpalette benutzen
                break;
        }
        return l;
    }

    private bool CompareColor(Color32 a, Color32 b)
    {
        // Kleiner Schwellenwert (Threshold), falls die vgmaps leichte Farbvariationen haben
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }
#if UNITY_EDITOR
    private void MakeTextureReadable(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        // In Unity 6 nutzen wir AssetImporter.GetAtPath
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        if (ti != null && (!ti.isReadable || ti.textureCompression != TextureImporterCompression.Uncompressed))
        {
            ti.isReadable = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed; // Wichtig für Pixel-Perfect Logic!
            ti.SaveAndReimport(); // Sauberes Re-Importieren in Unity 6
        }
    }
#endif
}