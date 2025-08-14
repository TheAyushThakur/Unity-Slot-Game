using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LeverController : MonoBehaviour, IPointerDownHandler
{
    [Header("Animation")]
    [SerializeField] private Animator leverAnimator;
    [SerializeField] private float animationDuration = 1f;

    [Header("Events")]
    public UnityEvent onLeverClicked; // For showing panel
    public UnityEvent onPullComplete; // For starting spin

    private bool isAnimating = false;

    private void Awake()
    {
        if (leverAnimator == null)
            leverAnimator = GetComponent<Animator>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isAnimating)
        {
            onLeverClicked.Invoke();
        }
    }

    public void PullLever()
    {
        if (!isAnimating)
        {
            isAnimating = true;
            leverAnimator.Play("LeverPull", 0, 0f);
            Invoke(nameof(OnAnimationComplete), animationDuration);
        }
    }

    private void OnAnimationComplete()
    {
        isAnimating = false;
        onPullComplete.Invoke();
    }
}
