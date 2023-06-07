using System;
using System.Collections;
using System.Collections.Generic;
using Burk.Core;
using Sudoku;
using UnityEngine;

public class LevelManager : MonoBehaviourSingleton<LevelManager>
{
    [SerializeField]private List<Level> levels;
    
    public int selectedLevelIndex;

    private void Start()
    {
        selectedLevelIndex = 0;
    }

    public Level GetSelectedLevel()
    {
        return levels[selectedLevelIndex];
    }

    public void GetToNextLevel()
    {
        selectedLevelIndex++;
        if(selectedLevelIndex >= levels.Count)
            selectedLevelIndex = 0;
    }
}
