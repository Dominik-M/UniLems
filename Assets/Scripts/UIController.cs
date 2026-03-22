using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public enum UIState { INIT, MAINMENU, LEVELMENU, SETTINGS, GAME_RUNNING, GAME_PAUSED, COUNT }

    public const int STATE_INIT = (int)UIState.INIT;
    public const int STATE_COUNT = (int)UIState.COUNT;

    [Header("UI References")]
    [SerializeField] private Text message;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject mainButtons;
    [SerializeField] private GameObject bottomMenu;
    [SerializeField] private Text mainButtonLeftLabel;
    [SerializeField] private Text mainButtonRightLabel;

    private UIState state = UIState.INIT;
    private bool victory = false;

    public void NavigateToMenu(int menuId)
    {
        if (menuId > STATE_INIT && menuId < STATE_COUNT)
        {
            UIState uistate = (UIState)menuId;
            SetState(uistate);
        }
    }

    void SetState(UIState nextState)
    {
        if (nextState != state)
        {
            state = nextState;
            mainButtonRightLabel.text = victory ? "Next Level" : "Retry";
            mainMenu.SetActive(state == UIState.MAINMENU);
            levelMenu.SetActive(state == UIState.LEVELMENU);
            settingsMenu.SetActive(state == UIState.SETTINGS);
            mainButtons.SetActive(state == UIState.GAME_PAUSED);
            bottomMenu.SetActive(state == UIState.GAME_RUNNING || state == UIState.GAME_PAUSED);
            message.gameObject.SetActive(state == UIState.GAME_PAUSED);
        }
    }

    void OnEnable()
    {
        message.text = "";
        Core.Instance.OnGameOver += HandleGameOver;
        Core.Instance.OnPauseResume += HandlePauseResume;
        Core.Instance.OnNewLevelStarted += HandleLevelStart;
    }

    void OnDisable()
    {
        Core.Instance.OnGameOver -= HandleGameOver;
        Core.Instance.OnPauseResume -= HandlePauseResume;
        Core.Instance.OnNewLevelStarted -= HandleLevelStart;
    }

    void Start()
    {
        SetState(UIState.MAINMENU);
    }
    private void HandleLevelStart(Level l)
    {
        SetState(UIState.GAME_RUNNING);
    }

    private void HandleGameOver(bool win)
    {
        message.text = win ? "You Win!" : "Game Over";
        victory = win;
        SetState(UIState.GAME_PAUSED);
    }

    private void HandlePauseResume(bool running)
    {
        if (!Core.Instance.IsGameOver)
        {
            message.text = running ? "" : "Paused";
            SetState(running ? UIState.GAME_RUNNING : UIState.GAME_PAUSED);
        }
    }

    public void HandlePauseButtonClicked()
    {
        Debug.Log("Pause button pressed");
        Core.Instance.Running = !Core.Instance.Running;
        Core.Instance.AM.PlaySound(SoundEffect.CLICK);
    }

    public void HandleLeftMainButtonClicked()
    {
        Debug.Log("Go to main menu");
        Core.Instance.ClearLevel();
        SetState(UIState.MAINMENU);
        Core.Instance.AM.PlaySound(SoundEffect.CLICK);
    }

    public void HandleRightMainButtonClicked()
    {
        if (victory)
        {
            Debug.Log("Start Next Level");
            Level nextLevel = Core.Instance.GetNextLevel();
            if (nextLevel != null)
                Core.Instance.StartLevel(nextLevel);
        }
        else
        {
            Debug.Log("Restart Level");
            Core.Instance.StartLevel(Core.Instance.GetCurrentLevel());
        }
        Core.Instance.AM.PlaySound(SoundEffect.CLICK);
        victory = false;
    }

    public void HandleContinueClicked()
    {
        Core.Instance.AM.PlaySound(SoundEffect.CLICK);
        Level nextLevel = Core.Instance.GetNextLevel();
        if (nextLevel != null && nextLevel.IsUnlocked)
        {
            Core.Instance.StartLevel(nextLevel);
            return;
        }
        // Next level not yet unlocked, start last (current) level or it's a new start
        nextLevel = Core.Instance.GetCurrentLevel();
        if (nextLevel != null && nextLevel.IsUnlocked)
        {
            Core.Instance.StartLevel(nextLevel);
        }
        else
        {
            Core.Instance.StartLevel(0);
        }
    }
}
