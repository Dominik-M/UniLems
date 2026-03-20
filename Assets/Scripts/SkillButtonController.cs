using UnityEngine;
using UnityEngine.UI;

public class SkillButtonController : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Skill skill;
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Text amount;

    void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    void Update()
    {
        amount.text = Core.Instance.GetSkillUsages(skill).ToString();
    }

    void OnClick()
    {
        Debug.Log("SkillButton Clicked: " + skill);
        Core.Instance.SelectedSkill = skill;
    }
}
