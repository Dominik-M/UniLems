using UnityEngine;
using UnityEngine.UI;

public class LevelButtonBuilder : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int rows;
    [SerializeField] private int cols;
    [Header("References")]
    [SerializeField] private GameObject prefab;

    private Rect mRect;
    private GridLayoutGroup grid;

    void Start()
    {
        mRect = GetComponent<RectTransform>().rect;
        grid = GetComponent<GridLayoutGroup>();
        mRect.width -= grid.padding.left;
        mRect.width -= grid.padding.right;
        mRect.height -= grid.padding.top;
        mRect.height -= grid.padding.bottom;
        BuildList();
    }
    public void BuildList()
    {
        if (cols != 0 && rows != 0)
        {
            float w = mRect.width / cols - grid.spacing.x;
            float h = mRect.height / rows - grid.spacing.y;
            grid.cellSize = new Vector2(w, h);
        }
        foreach (Level level in Core.Instance.GetAllLevels())
        {
            GameObject instance = Instantiate(prefab, transform, false);
            LevelButtonController btn = instance.GetComponent<LevelButtonController>();
            btn.SetLevel(level);
        }
    }
}
