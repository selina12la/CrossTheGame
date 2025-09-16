using UnityEngine;
using System;

public class GameSession : MonoBehaviour
{
    public static GameSession I { get; private set; }

    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnScoreChanged;

    [SerializeField] int startLives = 3;
    public int Lives { get; private set; }
    public int Score { get; private set; }

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        Lives = startLives;
        Score = 0;
    }

    public static void ResetSession(int lives)
    {
        if (I == null) return;
        I.Lives = lives;
        I.Score = 0;
        OnLivesChanged?.Invoke(I.Lives);
        OnScoreChanged?.Invoke(I.Score);
    }

    public static void AddScore(int amount)
    {
        if (I == null) return;
        I.Score += amount;
        OnScoreChanged?.Invoke(I.Score);
    }

    public static void LoseLife(int amount = 1)
    {
        if (I == null) return;
        I.Lives = Mathf.Max(0, I.Lives - amount);
        OnLivesChanged?.Invoke(I.Lives);
    }
}