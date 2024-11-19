using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordsData : MonoBehaviour
{
    [SerializeField]
    private Text charText;
    [HideInInspector]
    public char charValue;
    private Button buttonObj;
    public bool isSelected = false;

    private void Awake()
    {
        buttonObj = GetComponent<Button>();
    }

    public void SetChar(char value)
    {
        charText.text = value+"";
        charValue = value;
    }

    public void SelectChar()
    {
        if (!isSelected)
        {
            // Visual indication of selection, e.g., changing color
            charText.color = Color.green;
            isSelected = true;
            QuizController.instance.SelectedOption(this);

        }
    }

    public void DeselectChar()
    {
        charText.color = Color.black;
        isSelected = false;
    }
}
