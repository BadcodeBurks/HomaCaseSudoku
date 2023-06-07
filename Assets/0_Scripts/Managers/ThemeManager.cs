using System;
using System.Collections;
using Burk.Core;
using Sudoku;
using UnityEngine;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviourSingleton<ThemeManager>
{
    [Serializable]
    public class TextMaterialPair
    {
        public Enums.IndexType type;
        public Material material;
    }

    [SerializeField] private Image sudokuBGImage;
    [SerializeField] private Image upSeparatorImage;
    [SerializeField] private Image downSeparatorImage;
    [SerializeField] private Image leftSeparatorImage;
    [SerializeField] private Image rightSeparatorImage;
    [SerializeField] private TextMaterialPair[] textMaterials;
    
    public Color[] numberColors;
    
    public Color bgColor;
    public Color sudokuBgColor;
    public Color separatorColor;
    public Color cellHighlightColor;
    public Color cellSameNumberHighlightColor;
    public Color areaHighlightColor;
    public ColorBlock buttonColors;

    public void Init()
    {
        Color clearSudokuBGColor = sudokuBgColor;
        clearSudokuBGColor.a = 0;
        sudokuBGImage.color = clearSudokuBGColor;
        upSeparatorImage.color = separatorColor;
        downSeparatorImage.color = separatorColor;
        leftSeparatorImage.color = separatorColor;
        rightSeparatorImage.color = separatorColor;
        Camera.main.backgroundColor = bgColor;
    }

    public Color GetTextColor(Enums.IndexType type)
    {
        return numberColors[(int) type];
    }

    public void SudokuPanelSwitch(bool on)
    {
        Color clearSudokuBGColor = sudokuBgColor;
        clearSudokuBGColor.a = 0;
        StartCoroutine(SudokuBGColorFade(on? sudokuBgColor : clearSudokuBGColor, 0.5f));
    }
    
    private IEnumerator SudokuBGColorFade(Color color, float duration)
    {
        float t = 0;
        Color startColor = sudokuBGImage.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            sudokuBGImage.color = Color.Lerp(startColor, color, t / duration);
            yield return null;
        }
    }
}
