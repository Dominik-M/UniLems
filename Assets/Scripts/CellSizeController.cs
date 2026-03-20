using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class CellSizeController : MonoBehaviour
{
    private RectTransform rt;
    [Header("Dimension portions per child element")]
    [SerializeField] private Vector2[] portions = new Vector2[] { new Vector2(0.7f, 0.5f), new Vector2(0.3f, 0.5f) };
    [Header("Anchor Paddings")]
    [SerializeField] private float left = 0.01f;
    [SerializeField] private float right = 0.01f;
    [SerializeField] private float top = 0.01f;
    [SerializeField] private float bottom = 0.01f;

    void Awake()
    {
        // Komponenten automatisch holen
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        // Initial Update
        UpdateLayout();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateLayout();
    }

    // Wird aufgerufen, wenn sich die Größe ändert (auch bei Bildschirm-Rotation)
    void UpdateLayout()
    {
        // Sicherstellen, dass die Komponenten geladen sind (wichtig für den Editor-Modus)
        if (rt == null) return;

        float width = rt.rect.width;
        float height = rt.rect.height;

        // Kleiner Check, um Division durch 0 zu vermeiden
        if (width <= 0 || height <= 0) return;


        bool portrait = Screen.width <= Screen.height;
        int idx = 0;
        float offset = 0;
        foreach (RectTransform child in rt)
        {
            if (idx >= portions.Length) { Debug.LogWarning("More children than dimensions defined"); break; }
            if (portrait)
            {
                // Portrait: 2 Zeilen
                child.anchorMin = new Vector2(left, offset + bottom);
                offset += portions[idx].y;
                child.anchorMax = new Vector2(1 - right, offset - top);
            }
            else
            {
                // Landscape: 2 Spalten
                child.anchorMin = new Vector2(offset + left, bottom);
                offset += portions[idx].x;
                child.anchorMax = new Vector2(offset - right, 1 - top);
            }
            idx++;
        }
    }
}