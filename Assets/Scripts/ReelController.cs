using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class ReelController : MonoBehaviour
{
    [Header("Symbols")]
    [SerializeField] private Sprite[] possibleSymbols;
    public Sprite[] PossibleSymbols => possibleSymbols;

    [Header("References")]
    [SerializeField] private Transform stripe;

    [Header("Spin timing/feel")]
    [SerializeField] private float startSpinSpeed = 8f;
    [SerializeField] private float slowDownRate = 6f;
    [SerializeField] private int minFullCycles = 3;
    [SerializeField] private int maxFullCycles = 5;

    [Header("Layout")]
    [SerializeField] private float symbolHeight = 0f;
    [SerializeField] private int visibleSlots = 3;

    [Header("UI Panels")]
    public GameObject insufficientCreditsPanel;
    public GameObject insufficientBetCreditsPanel;

    private List<Transform> slots;
    private List<SpriteRenderer> slotRenderers;
    private List<int> slotIndices;
    private int virtualNextSymbolIndex;
    private int centerSlotIndex;
    private float totalHeight;

    private int shiftsNeeded;
    private int shiftsDone;
    private float currentSpeed;
    private bool spinning = false;
    private bool slowingDown = false;

    public bool IsSpinning => spinning;
    public Sprite CurrentSymbol { get; private set; }

    private void Awake()
    {
        if (stripe == null) stripe = transform;

        slots = new List<Transform>();
        slotRenderers = new List<SpriteRenderer>();
        slotIndices = new List<int>();

        for (int i = 0; i < stripe.childCount; i++)
        {
            var child = stripe.GetChild(i);
            slots.Add(child);
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr == null)
                Debug.LogError($"ReelController: child '{child.name}' has no SpriteRenderer!");
            slotRenderers.Add(sr);

            int idx = 0;
            if (sr != null && sr.sprite != null)
                idx = FindSymbolIndex(sr.sprite);
            slotIndices.Add(idx);
        }

        if (slotRenderers.Count == 0)
        {
            Debug.LogError("ReelController: No slot children found under stripe.");
            return;
        }

        if (symbolHeight <= 0f)
        {
            var first = slotRenderers[0];
            symbolHeight = first.bounds.size.y;
            if (symbolHeight <= 0f)
                Debug.LogError("ReelController: Could not detect symbolHeight automatically. Set it in inspector.");
        }

        totalHeight = symbolHeight * slots.Count;
        centerSlotIndex = Mathf.Clamp(visibleSlots / 2, 0, slots.Count - 1);
        virtualNextSymbolIndex = (slotIndices[slotIndices.Count - 1] + 1) % possibleSymbols.Length;
        CurrentSymbol = slotRenderers[centerSlotIndex].sprite;
    }

    private int FindSymbolIndex(Sprite s)
    {
        if (possibleSymbols == null || possibleSymbols.Length == 0) return 0;
        for (int i = 0; i < possibleSymbols.Length; i++)
            if (possibleSymbols[i] == s) return i;
        return 0;
    }

    public void StartSpin(int targetSymbolIndex)
    {
        if (possibleSymbols == null || possibleSymbols.Length == 0)
        {
            Debug.LogError("ReelController: no possibleSymbols assigned!");
            return;
        }

        if (spinning) return;

        int currentCenterSymbolIndex = slotIndices[centerSlotIndex];
        int delta = (targetSymbolIndex - currentCenterSymbolIndex + possibleSymbols.Length) % possibleSymbols.Length;

        int cycles = Random.Range(minFullCycles, maxFullCycles + 1);
        shiftsNeeded = cycles * possibleSymbols.Length + delta;
        shiftsDone = 0;

        currentSpeed = startSpinSpeed;
        spinning = true;
        slowingDown = false;
    }

    private void Update()
    {
        if (!spinning) return;

        // If reels are in slow-down phase then
       // decelerate every frame
        if (slowingDown)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, slowDownRate * Time.deltaTime);
            if (currentSpeed <= 0.01f)
            {
                spinning = false;
                slowingDown = false;
                currentSpeed = startSpinSpeed;
                CurrentSymbol = slotRenderers[centerSlotIndex].sprite;
                return;
            }
        }

        // Moves the row down
        stripe.localPosition += Vector3.down * currentSpeed * Time.deltaTime;

        if (stripe.localPosition.y <= -symbolHeight)
        {
            stripe.localPosition += Vector3.up * symbolHeight;

            slotIndices.RemoveAt(0);
            slotIndices.Add(virtualNextSymbolIndex);
            virtualNextSymbolIndex = (virtualNextSymbolIndex + 1) % possibleSymbols.Length;

            for (int i = 0; i < slotRenderers.Count; i++)
                slotRenderers[i].sprite = possibleSymbols[slotIndices[i]];

            shiftsDone++;

            if (shiftsDone >= shiftsNeeded && !slowingDown)
                slowingDown = true;
        }
    }
}
