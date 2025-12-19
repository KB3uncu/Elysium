using UnityEngine;
using TMPro;

public class TriviaGameManager : MonoBehaviour
{
    public QuestionData[] questions;
    private int currentIndex = 0;
    public QuestionData currentQuestion;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts;
    //public TextMeshProUGUI resultAnswerText;
    void Start()
    {
        currentIndex = 0;
        SetQuestions(currentIndex);
    }

    void SetQuestions(int index)
    {
        currentQuestion = questions[index];
        LoadQuestion();
    }

    void LoadQuestion()
    {
        questionText.text = currentQuestion.questionText;

        for (int i = 0; i < answerTexts.Length; i++)
        {
            answerTexts[i].text = currentQuestion.answers[i];
        }
    }

    public void CheckAnswer(int index)
    {
        if(index == currentQuestion.correctAnswerIndex)
        {
            Debug.Log("DOÐRU GOD DEAYMMM");
        }
        else
        {
            Debug.Log("yanlýi yaptýn ENAYÝ");
        }
    }

    void NextQuestion()
    {
        currentIndex++;
        if(currentIndex >= questions.Length)
        {
            Debug.Log("Soru bitti laminyo");
            return;
        }

        SetQuestions(currentIndex);
    }

    public void SelectButton(int index)
    {
        CheckAnswer(index);
        NextQuestion();
    }

}
