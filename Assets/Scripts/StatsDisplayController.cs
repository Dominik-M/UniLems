using UnityEngine;
using UnityEngine.UI;

public class StatsDisplayController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text totalValue;
    [SerializeField] private Text savedValue;
    [SerializeField] private Text goalValue;

    void Update()
    {
        totalValue.text = Core.Instance.GetSpawnedCount().ToString();
        savedValue.text = Core.Instance.GetSavedCount().ToString();
        Level level = Core.Instance.GetCurrentLevel();
        if (level != null)
        {
            goalValue.text = level.MinGuysSavedToWin.ToString();
        }
    }
}
