using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;

public class SlotMachineController : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField] private ReelController[] reels;
    [SerializeField] private int[] betAmounts = { 10, 50, 100 };

    [Header("Payout Multipliers")]
    [SerializeField] private int barMultiplier = 5;
    [SerializeField] private int sevenMultiplier = 10;
    [SerializeField] private int defaultMultiplier = 2;

    [Header("UI References")]
    [SerializeField] private BetMenuController betMenuController;
    public GameObject insufficientCreditsPanel;
    public GameObject insufficientBetCreditsPanel;

    private int currentCredits = 100;
    private int currentBetIndex = 0;
    private bool isSpinning = false;

    public int CurrentCredits => currentCredits;

    public void SetCurrentBet(int betIndex)
    {
        currentBetIndex = betIndex;
    }

    public void Spin()
    {
        if (reels == null || reels.Length == 0)
             return;

        if (isSpinning)
            return;

        if (currentCredits <= 0)
        {
            StartCoroutine(ShowPanelForSeconds(insufficientCreditsPanel, 3f));
            return;
        }

        
        if (currentCredits < betAmounts[currentBetIndex])
        {
            StartCoroutine(ShowPanelForSeconds(insufficientBetCreditsPanel, 3f));
            betMenuController.ShowMenu(); 
            return;
        }

        currentCredits -= betAmounts[currentBetIndex];
        betMenuController.UpdateCreditDisplay();
        StartCoroutine(SpinReels());
    }

    private IEnumerator SpinReels()
    {
        isSpinning = true;
        int[] results = new int[reels.Length];

        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] != null && reels[i].PossibleSymbols != null && reels[i].PossibleSymbols.Length > 0)
            {
                results[i] = Random.Range(0, reels[i].PossibleSymbols.Length);
            }
        }

        // Start spinning each reel
        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] != null)
            {
                reels[i].StartSpin(results[i]);
                yield return new WaitForSeconds(0.2f);
            }
        }

        yield return new WaitUntil(() => reels.All(r => !r.IsSpinning));

        CheckWin();
        isSpinning = false;

        if (currentCredits >= betAmounts.Min())
        {
            betMenuController.ShowMenu();
        }

        
    }

    private void CheckWin()
    {
        bool isWin = true;
        Sprite firstSymbol = reels[0].CurrentSymbol;

        for (int i = 1; i < reels.Length; i++)
        {
            if (reels[i].CurrentSymbol != firstSymbol)
            {
                isWin = false;
                break;
            }
        }

        if (isWin)
        {
            int winAmount = betAmounts[currentBetIndex] * GetPayoutMultiplier(firstSymbol);
            currentCredits += winAmount;
            betMenuController.UpdateCreditDisplay();
        }
    }

    private int GetPayoutMultiplier(Sprite symbol)
    {
        if (symbol.name.Contains("BAR")) return barMultiplier;
        if (symbol.name.Contains("7")) return sevenMultiplier;
        return defaultMultiplier;
    }

    private IEnumerator ShowPanelForSeconds(GameObject panel, float seconds)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(seconds);
        panel.SetActive(false);
    }
}
