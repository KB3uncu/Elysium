using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectButton : MonoBehaviour
{
    [SerializeField] private TriviaGameManager triviaGameManager;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) { 
                
              GameObject clicked = hit.collider.gameObject;

                if (clicked.CompareTag("AnswerA")) triviaGameManager.SelectButton(0);
                else if (clicked.CompareTag("AnswerB")) triviaGameManager.SelectButton(1);
                else if (clicked.CompareTag("AnswerC")) triviaGameManager.SelectButton(2);
                else if (clicked.CompareTag("AnswerD")) triviaGameManager.SelectButton(3);

            }
        }
           
        
    }
}
