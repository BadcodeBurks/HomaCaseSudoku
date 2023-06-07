using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Burk.Core;
using Sudoku;
using TMPro;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public enum GameState
    {
        MainMenu,
        InGame,
    }

    #region Managers

    public LevelManager levelManager;
    public GridManager gridManager;    
    
    #endregion

    public SudokuSession currentSession;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelHeader;
    
    private GameState _gameState;
    private float _timerStart;
    private bool _timerRunning;

    private void Start()
    {
        Input.backButtonLeavesApp = true;
        
        levelManager = LevelManager.I;
        gridManager = GridManager.I;
        ThemeManager.I.Init();
        SetGameState(GameState.InGame);
    }
    
    public void SetGameState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                //gridManager.CloseGrid()
                //UIManager.Instance.ShowGameUI(false);
                //UIManager.Instance.ShowMenuUI(true);
                break;
            case GameState.InGame:
                gridManager.OpenGrid();
                StartSudokuSession();
                break;
        }
        _gameState = state;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (_timerRunning)
        {
            float time = Time.time - _timerStart;
            timerText.text = $"<b>Time : {(int)time / 60}:{(int)time % 60}";
        }
    }

    #region Timer

    public void TimerStart()
    {
        _timerStart = Time.time;
        _timerRunning = true;
    }
    public void TimerStop()
    {
        _timerRunning = false;
    }
    public void TimerReset()
    {
        timerText.color = ThemeManager.I.GetTextColor(Enums.IndexType.Default);
        timerText.text = "<b>Time : 0:0";
        _timerRunning = false;
    }

    #endregion

    #region Level Name

    public void SetLevelName()
    {
        levelHeader.color = ThemeManager.I.GetTextColor(Enums.IndexType.Unchangeable);
        string levelName = levelManager.GetSelectedLevel().name;
        StartCoroutine(SetNameLetterByLetter(levelName));
    }

    public void ClearLevelName()
    {
        StartCoroutine(RemoveNameLetterByLetter());
    }
    
    
    private IEnumerator RemoveNameLetterByLetter()
    {
        for (int i = timerText.text.Length -1; i >= 3; i--)
        {
            levelHeader.text = levelHeader.text.Remove(i);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator SetNameLetterByLetter(string levelName)
    {
        levelHeader.text = "<b>";
        for (int i = 0; i < levelName.Length; i++)
        {
            levelHeader.text += levelName[i];
            yield return new WaitForSeconds(0.1f);
        }
    }

    #endregion

    public void StartSudokuSession()
    {
        TimerReset();
        currentSession = new SudokuSession(levelManager.GetSelectedLevel());
        gridManager.OnGridReady += currentSession.StartSession;
        currentSession.OnLevelCompleted += ()=>
        {
            ClearLevelName();
            TimerStop();
            gridManager.CloseGrid();
            StartCoroutine(StartNextLevel(2f));
        };
    }

    public IEnumerator StartNextLevel(float time)
    {
        LevelManager.I.GetToNextLevel();
        yield return new WaitForSeconds(time);
        SetGameState(GameState.InGame);
    }
    
    
}
