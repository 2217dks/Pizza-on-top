using UnityEngine;
using PizzaOnTop.Player;
using PizzaOnTop.Managers;

[RequireComponent(typeof(Collider2D))]
public class TutorialTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private CanvasGroup popup;

    private bool open;
    private PlayerController2D player;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        player = FindAnyObjectByType<PlayerController2D>();

        if (popup != null)
            popup.gameObject.SetActive(false);
    }

    public string GetInteractionPrompt()
    {
        return "Read";
    }

    public void Interact()
    {
        player = FindAnyObjectByType<PlayerController2D>();
        Debug.Log("Tutorial opened");

        open = !open;

        popup.gameObject.SetActive(open);

        if (TimerManager.Instance != null)
        {
            if (open)
                TimerManager.Instance.PauseTimer();
            else
                TimerManager.Instance.ResumeTimer();
        }

        if (player != null)
        {
            player.SetVelocity(Vector2.zero);
            player.enabled = !open;
        }
    }
}