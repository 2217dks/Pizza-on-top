using UnityEngine;
public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }

    [SerializeField] private float floorHeight = 16f;

    public int CurrentFloor { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateFloor(float playerY)
    {
        CurrentFloor = Mathf.FloorToInt(playerY / floorHeight);
    }
}