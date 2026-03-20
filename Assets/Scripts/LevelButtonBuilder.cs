using UnityEngine;

public class LevelButtonBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject prefab;


    void Start()
    {
        BuildList();
    }
    public void BuildList()
    {
        foreach (Level level in Core.Instance.GetAllLevels())
        {
            GameObject instance = Instantiate(prefab, transform, false);
            LevelButtonController btn = instance.GetComponent<LevelButtonController>();
            btn.SetLevel(level);
        }
    }
}
