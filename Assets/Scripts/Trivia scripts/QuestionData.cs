using UnityEngine;

[CreateAssetMenu(
    fileName ="New Question",
    menuName = "Trivia/Question"
    )]

public class QuestionData : ScriptableObject
{
    public string questionText;

    public string[] answers;

    public int correctAnswerIndex;
}
