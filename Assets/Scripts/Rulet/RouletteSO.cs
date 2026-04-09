using UnityEngine;

public enum StartTurn
{
    Player,
    Enemy
}

[CreateAssetMenu(menuName = "Roulette/SO", fileName = "RouletteSO")]
public class RouletteSO : ScriptableObject
{
    [Header("Core Settings")]
    [Min(1)] public int chamberCount = 6;
    [Min(0)] public int bulletCount = 2;

    [Header("Lives")]
    [Min(1)] public int maxLives = 3;

    [Header("Turn")]
    public StartTurn startTurn = StartTurn.Player;
}
