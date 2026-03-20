using UnityEngine;
using UnityEngine.UI;

public class LevelButtonController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text title;
    [SerializeField] private Image levelVG;
    [SerializeField] private GameObject lockedDisplay;

    private Level mLevel;

    public void SetLevel(Level l) { mLevel = l; UpdateDisplay(); }

    void Start()
    {
        UpdateDisplay();
        GetComponent<Button>().onClick.AddListener(HandleClick);
    }

    public void UpdateDisplay()
    {
        if (mLevel != null
            && title != null
            && levelVG != null)
        {
            title.text = mLevel.name;
            int width = mLevel.VisualTexture.width;
            int height = mLevel.VisualTexture.height;
            levelVG.sprite = Sprite.Create(mLevel.VisualTexture,
                new Rect(0, 0, width, height), new Vector2(0f, 0f), 1);
            lockedDisplay.SetActive(!mLevel.IsUnlocked);
        }
    }

    public void HandleClick()
    {
        if (mLevel != null && mLevel.IsUnlocked)
            Core.Instance.StartLevel(mLevel);
    }
}
