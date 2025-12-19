using UnityEngine;
using TMPro;

public class TriviaGameManager : MonoBehaviour
{
    public HealthManager healthManager;

    public QuestionData[] questions;
    private int currentIndex = 0;
    public QuestionData currentQuestion;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts;
    public TextMeshProUGUI resultAnswerText;

    public float nextDelay = 0.8f;
    private bool canAnswer = true;

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
        resultAnswerText.text = "";
    }

    public void CheckAnswer(int index)
    {
        if(index == currentQuestion.correctAnswerIndex)
        {
            resultAnswerText.text = "HELAL OLSUN";
            resultAnswerText.color = Color.green;
        }
        else
        {
            resultAnswerText.text = "NEÐÐÐÐ DÝYON OLUM";
            resultAnswerText.color = Color.red;

            healthManager.LoseHealth();

            if (healthManager.IsDead())
            {
                resultAnswerText.text = "Kaybettinke";
                canAnswer = false;
                return;
            }
        }
    }

    void NextQuestion()
    {
        currentIndex++;
        if(currentIndex >= questions.Length)
        {
            resultAnswerText.text = "Kazandýn loooo";
            Debug.Log("Soru bitti laminyo");
            return;
        }

        SetQuestions(currentIndex);
        
    }

    public void SelectButton(int index)
    {
        if(!canAnswer)return;
        canAnswer = false;

        CheckAnswer(index);

        Invoke(nameof(GoNext), nextDelay);

    }

    void GoNext()
    {
        ClearBoard();
        Invoke(nameof(LoadNextQuestion), 1f);
    }

    void LoadNextQuestion()
    {
        NextQuestion();
        canAnswer= true;
    }

    void ClearBoard()
    {
        questionText.text = "";

        for (int i = 0; i < answerTexts.Length; i++)
        {
            answerTexts[i].text = "";
        }
    }


}
