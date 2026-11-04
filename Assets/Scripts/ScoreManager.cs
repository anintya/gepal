
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score;
    public TMP_Text scoreText;

    private void Awake()
    {
        Instance = this;
        UpdateScoreUI();
    }

    public void AddPoint()
    {
        score += 10;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Skor: " + score;
    }
}