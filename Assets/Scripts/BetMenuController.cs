using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BetMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button[] betButtons;

    [Header("Dependencies")]
    [SerializeField] private LeverController leverController;
    [SerializeField] private SlotMachineController slotMachine;
    [SerializeField] private Text creditText;

    private void Start()
    {
        if (slotMachine == null)
        {
            slotMachine = FindObjectOfType<SlotMachineController>();
        }

        menuPanel.SetActive(false);

        if (leverController != null)
        {
            leverController.onLeverClicked.AddListener(ShowMenu);
            leverController.onPullComplete.AddListener(OnLeverPullComplete);
        }

        for (int i = 0; i < betButtons.Length; i++)
        {
            int index = i;
            betButtons[i].onClick.AddListener(() => OnBetSelected(index));
        }
    }

    public void ShowMenu()
    {
        menuPanel.SetActive(true);
        UpdateCreditDisplay();
    }

    public void HideMenu()
    {
        menuPanel.SetActive(false);
    }

    private void OnBetSelected(int betIndex)
    {
        slotMachine.SetCurrentBet(betIndex);
        HideMenu();
        leverController.PullLever(); 
    }

    private void OnLeverPullComplete()
    {
        if (slotMachine != null)
        {
            slotMachine.Spin();
        }
    }

    public void UpdateCreditDisplay()
    {
        creditText.text = "   Credits: " + slotMachine.CurrentCredits;
    }

    public void exit()
    {
        SceneManager.LoadScene("MainScene");
    }
}
