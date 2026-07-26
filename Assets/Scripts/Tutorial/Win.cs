using UnityEngine;
using PizzaOnTop.Player;
using PizzaOnTop.Managers;
using Unity.VisualScripting;
using UnityEngine.InputSystem.iOS;

[RequireComponent(typeof(Collider2D))]
public class Win : MonoBehaviour, IInteractable
{
    [SerializeField] private CanvasGroup popup;
    private PlayerController2D player;

    [SerializeField] private GameObject winui;

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
        Debug.Log("win");

        popup.gameObject.SetActive(true);

        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.PauseTimer();
        }

        if (player != null)
        {
            player.SetVelocity(Vector2.zero);
            player.enabled = false;
            player.gameObject.GetComponent<PlayerInteract>().enabled = false;
            Invoke("WinUI",1f);
        }
    }

    private void WinUI(){
        winui.SetActive(true);
    }
}