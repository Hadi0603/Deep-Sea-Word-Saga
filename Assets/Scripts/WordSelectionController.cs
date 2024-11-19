using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WordSelectionController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private List<WordsData> selectedWords = new List<WordsData>();
    private LineRenderer lineRenderer;
    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.sortingOrder = 2;
        lineRenderer.sortingLayerName = "UI";
        lineRenderer.useWorldSpace = true;

        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        selectedWords.Clear();
        lineRenderer.positionCount = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        foreach (WordsData word in selectedWords)
        {
            word.DeselectChar();
        }
        selectedWords.Clear();
        lineRenderer.positionCount = 0;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && graphicRaycaster != null && eventSystem != null)
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            // Raycast to detect UI elements
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);

            bool foundSelectable = false;

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.CompareTag("Option"))
                {
                    WordsData wordsData = result.gameObject.GetComponent<WordsData>();
                    if (wordsData != null && !wordsData.isSelected)
                    {
                        selectedWords.Add(wordsData);
                        wordsData.SelectChar();
                        lineRenderer.positionCount = selectedWords.Count + 1; // +1 for the cursor
                        lineRenderer.SetPosition(selectedWords.Count - 1, wordsData.transform.position);
                        foundSelectable = true;
                    }
                }
            }
            if (foundSelectable || selectedWords.Count > 0)
            {
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane)));
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("hi");
            QuizController.instance.ResetQuestion();
            for (int i = 0; i <10; i++)
            {
                QuizController.instance.ResetLastWord();
            }
            
        }
    }


    public void ResetSelections()
    {
        foreach (WordsData word in selectedWords)
        {
            word.DeselectChar();
        }
        selectedWords.Clear();
        lineRenderer.positionCount = 0;
    }
}