using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectButton : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) { 
                
              GameObject clicked = hit.collider.gameObject;

                if (clicked.CompareTag("AnswerA"))
                {
                    Debug.Log("A seçtin dostum");
                }
                else if (clicked.CompareTag("AnswerB"))
                {
                    Debug.Log("B seçtin dostum");
                }
                else if (clicked.CompareTag("AnswerC"))
                {
                    Debug.Log("C seçtin dostum");
                }
                else if (clicked.CompareTag("AnswerD"))
                {
                    Debug.Log("D seçtin dostum");
                }
            }
        }
           
        
    }
}
