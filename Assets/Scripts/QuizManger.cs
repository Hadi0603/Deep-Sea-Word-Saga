using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class QuizController : MonoBehaviour
{
    public static QuizController instance;
    [SerializeField] public GameObject levelWin;
    [SerializeField] private GameObject lostUI;      // UI to show when time runs out
    [SerializeField] private Text timerText;              // UI text for the countdown timer
    [SerializeField] private int gameTime = 60; 
    [SerializeField] private QuizData questionData;
    [SerializeField] private WordsData[] answerWordArray;
    [SerializeField] private WordsData[] optionWordArray;
    [SerializeField] private Text wrongWordText;
    [SerializeField] private Text correctWordText;
    [SerializeField] private GameObject questionImage;
    [SerializeField] private float transparency = 0f;
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private char[] charArray = new char[6];

    [SerializeField] private GameObject pauseBtn;
    private int currentAnswerIndex = 0;
    private bool correctAnswer = true;
    private List<int> selectedWordIndex;
    private int currentQuestionIndex = 0;
    private GameStatus gameStatus = GameStatus.Playing;
    private string answerWord;
    private string answerWord1;
    private int number;
    public static int score = 0;
    private int wordsRemaining = 2;
    private bool firstWordCompleted = false;
    private int remainingTime;
    private bool levelLost = false;
    private int levelWon = 0;
    private bool isAnswerWordCompleted = false;
    private bool isAnswerWord1Completed = false;
    private Coroutine levelTimerCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        selectedWordIndex = new List<int>();
    }

    private void Start()
    {
        remainingTime = gameTime;
        timerText.gameObject.SetActive(true);
        UpdateTimerUI();
        levelTimerCoroutine = StartCoroutine(LevelTimer());
        levelWin.SetActive(false);
        pauseBtn.SetActive(true);
        correctWordText.gameObject.SetActive(false);
        wrongWordText.gameObject.SetActive(false);
        SetQuestion();
        FindObjectOfType<WordSelectionController>().enabled = true;
    }
    private IEnumerator LevelTimer()
    {
        
        while (remainingTime > 0 && !levelLost&&levelWon==0)
        {
            yield return new WaitForSeconds(1f);
            remainingTime--;
            UpdateTimerUI();

            if (remainingTime <= 0)
            {
                ShowLevelLost();
            }
        }
    }
    private void UpdateTimerUI()
    {
        timerText.text = $"00:{remainingTime:D2}";
        timerText.color = remainingTime <= 10 ? Color.red : Color.black;
    }
    private IEnumerator FadeOut(SpriteRenderer spriteRenderer, float duration)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer is null. Cannot fade out.");
            yield break;
        }

        // Get the initial color
        Color startColor = spriteRenderer.color;

        // Target color with alpha set to 0
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Interpolate the color over time
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the final color is fully transparent
        
        spriteRenderer.color = endColor;
        Debug.Log($"{spriteRenderer.gameObject.name} has faded out.");
    }

    private void ChangeTransparency()
    {
        // Get the child GameObject's SpriteRenderer
        Transform childTransform = questionImage.transform.GetChild(0); // Get the first child

        if (childTransform != null)
        {
            SpriteRenderer childSpriteRenderer = childTransform.GetComponent<SpriteRenderer>();
            if (childSpriteRenderer != null)
            {
                // Gradually decrease transparency
                StartCoroutine(FadeOut(childSpriteRenderer, 1f)); // Fade out over 1 second
            }
            else
            {
                Debug.LogError("No SpriteRenderer found on the child GameObject.");
            }
        }
        else
        {
            Debug.LogError("No child found under the questionImage GameObject.");
        }
    }

    private IEnumerator SlideOut(GameObject target, Vector3 direction, float duration)
    {
        yield return new WaitForSeconds(duration);
        // Get the initial position of the GameObject
        Vector3 startPosition = target.transform.position;

        // Calculate the target position
        Vector3 endPosition = startPosition + direction;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Interpolate position over time
            target.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches the final position
        target.transform.position = endPosition;

        Debug.Log($"{target.name} has slid out!");
    }

    private void ShowLevelLost()
    {
        WrongAnswer();
        pauseBtn.SetActive(false);
        levelLost = true;
        lostUI.SetActive(true);
        FindObjectOfType<WordSelectionController>().enabled = false;
    }
    private void SetQuestion()
    {
        currentAnswerIndex = 0;
        selectedWordIndex.Clear();
        correctWordText.gameObject.SetActive(false);
        if (currentQuestionIndex >= questionData.questions.Count)
        {
            levelWin.SetActive(true);
            timerText.gameObject.SetActive(false);
            FindObjectOfType<WordSelectionController>().enabled = false;
            return;
        }

        answerWord = questionData.questions[currentQuestionIndex].answer;
    
        ResetQuestion();

        for (int i = 0; i < answerWord.Length; i++)
        {
            charArray[i] = char.ToUpper(answerWord[i]);
        }
        for (int i = answerWord.Length; i < optionWordArray.Length; i++)
        {
            charArray[i] = (char)UnityEngine.Random.Range(65, 91);
        }
        charArray = ShuffleListData.ShuffleListItems<char>(charArray.ToList()).ToArray();

        for (int i = 0; i < optionWordArray.Length; i++)
        {
            optionWordArray[i].SetChar(charArray[i]);
        }

        gameStatus = GameStatus.Playing;
    }


public void SelectedOption(WordsData wordsData)
{
    if (gameStatus == GameStatus.Next || currentAnswerIndex >= answerWord.Length) return;

    selectedWordIndex.Add(wordsData.transform.GetSiblingIndex());
    answerWordArray[currentAnswerIndex].SetChar(wordsData.charValue);
    currentAnswerIndex++;

    if (currentAnswerIndex >= answerWord.Length)
    {
        correctAnswer = true;
        
        for (int i = 0; i < answerWord.Length; i++)
        {
            if (char.ToUpper(answerWord[i]) != char.ToUpper(answerWordArray[i].charValue))
            {
                correctAnswer = false;
                break;
            }
        }

        if (correctAnswer)
        {
            score += 50;
            PlayerPrefs.SetInt("Score", score);
            PlayerPrefs.SetInt("levelToLoad",++GameController.levelToLoad);
            PlayerPrefs.Save();
            correctWordText.gameObject.SetActive(true);
            pauseBtn.SetActive(false);
            if (levelTimerCoroutine != null)
            {
                StopCoroutine(levelTimerCoroutine);
                levelTimerCoroutine = null; // Clear reference to avoid reuse
            }
            ChangeTransparency();
            StartCoroutine(SlideOut(questionImage, Vector3.right * 5f, 1f));
            gameStatus = GameStatus.Next;

            currentQuestionIndex++;
            Invoke("SetQuestion", 2f);  // Move to the next word after completion
        }
        else
        {
            WrongAnswer();
        }
    }
}



    

    public void ResetQuestion()
    {
        FindObjectOfType<WordSelectionController>().ResetSelections();
        for (int i = 0; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(true);
            answerWordArray[i].SetChar('_');
        }
        for (int i = answerWord.Length; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < optionWordArray.Length; i++)
        {
            optionWordArray[i].gameObject.SetActive(true);
        }
    }

    public void WrongAnswer()
    {
        StartCoroutine(WrongAnswerCoroutine());
    }

    private IEnumerator WrongAnswerCoroutine()
    {
        Debug.Log("Incorrect answer.");
        wrongWordText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        FindObjectOfType<WordSelectionController>().ResetSelections();
        for(int i = 0;i < answerWordArray.Length; i++)
        {
            ResetLastWord();
        }
        yield return new WaitForSeconds(1f);
        wrongWordText.gameObject.SetActive(false);
    }

    public void ResetLastWord()
    {
        if (selectedWordIndex.Count > 0)
        {
            int index = selectedWordIndex[selectedWordIndex.Count - 1];
            optionWordArray[index].gameObject.SetActive(true);
            selectedWordIndex.RemoveAt(selectedWordIndex.Count - 1);
            currentAnswerIndex--;
            answerWordArray[currentAnswerIndex].SetChar('_');
            FindObjectOfType<WordSelectionController>().ResetSelections();
        }
    }
}

[System.Serializable]
public class QuestionData
{
    public string answer;
}

public enum GameStatus
{
    Playing,
    Next
}
