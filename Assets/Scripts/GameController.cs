using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] public GameObject pauseUI;
    public static int levelToLoad;
    private void Awake()
    {
        int savedScore = PlayerPrefs.GetInt("Score", 0);
        QuizController.score = savedScore;
        scoreText.text = "Score: " + QuizController.score;
    }

    public void Start()
    {
        levelToLoad = PlayerPrefs.GetInt("levelToLoad", 1);
    }

    public void PlayBtn()
    {
        SceneManager.LoadScene(levelToLoad);
    }
    public void Back()
    {
        Time.timeScale = 1f;
        FindObjectOfType<WordSelectionController>().enabled = true;
        SceneManager.LoadScene("Menu");
    }

    public void Pause()
    {
        pauseUI.SetActive(true);
        FindObjectOfType<WordSelectionController>().enabled = false;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        FindObjectOfType<WordSelectionController>().enabled = true;
        pauseUI.SetActive(false);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PlayTrial()
    {
        SceneManager.LoadScene("Trial");
    }
}