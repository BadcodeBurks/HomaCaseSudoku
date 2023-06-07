using System;
using System.Collections;
using Burk.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Sudoku
{
    public class GridManager : MonoBehaviourSingleton<GridManager>
    {
        public Action OnGridReady;
        public Action OnUnchangedCellsSet;

        public AnimationCurve movementCurve;

        public RectTransform gridRect;
        public RectTransform gridButtonsRect;
        public GameObject gridCellPrefab;
        public GameObject gridButtonPrefab;
        public GameObject eraserPrefab;
        public Transform cellParent;
        
        private CellController[] _cells;
        private ButtonController[] _buttons;
        
        private int _selectedCellIndex = -1;
        
        public void OpenGrid()
        {
            StartCoroutine(SpawnGridCellsAndButtons());
            InitHighlighters();
            InitSeparators();
            ThemeManager.I.SudokuPanelSwitch(true);
        }

        private IEnumerator SpawnGridCellsAndButtons()
        {
            float cellSize = gridRect.rect.width / 9;
            _cells = new CellController[81];
            for (int i = 0; i < 81; i++)
            {
                if(i%9 == 0)
                    yield return null;
                SpawnGridCell(i, cellSize);
            }
            _buttons = new ButtonController[10];
            float buttonHeight = cellSize * 1.6f;
            SpawnEraserButton();
            for (int i = 1; i < 10; i++)
            {
                SpawnButton(i, cellSize, buttonHeight);
            }
            yield return null;
            OnGridReady.Invoke();
        }

        private void SpawnGridCell(int order, float cellSize)
        {
            GameObject cell = Instantiate(gridCellPrefab, cellParent);
            cell.name = "Cell " + order;
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            CellController cellController = cell.GetComponent<CellController>();

            Vector2 relativeCenterPos = (new Vector2(order % 9, order / 9) + Vector2.one/2)/9;
            cellRect.position = (Vector3)gridRect.rect.position + gridRect.position + (Vector3)(relativeCenterPos * gridRect.rect.size);
            cellRect.sizeDelta = new Vector2(cellSize, cellSize);
            cellController.InitCell(order);
            _cells[order] = cellController;
        }

        private void SpawnButton(int number, float buttonWidth, float buttonHeight)
        {
            GameObject button = Instantiate(gridButtonPrefab, gridButtonsRect);
            button.name = "Button " + number;
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            ButtonController buttonController = button.GetComponent<ButtonController>();
            RectShowUtility rectShowUtility = button.GetComponent<RectShowUtility>();
            
            _buttons[number - 1] = buttonController;
            Vector2 relativeCenterPos = (new Vector2(number-1, 0) + Vector2.one/2)/9;
            buttonRect.position = (Vector3)gridButtonsRect.rect.position + gridButtonsRect.position + (Vector3)(relativeCenterPos * gridButtonsRect.rect.size * 1.01f);
            buttonRect.rect.Set(buttonRect.position.x, buttonRect.position.y, buttonWidth, buttonHeight);
            buttonController.InitButton(number);
            rectShowUtility.ShowIn(true, number * .1f);
        }
        
        private void SpawnEraserButton()
        {
            GameObject button = Instantiate(eraserPrefab, gridButtonsRect);
            button.name = "Eraser";
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            ButtonController buttonController = button.GetComponent<ButtonController>();
            RectShowUtility rectShowUtility = button.GetComponent<RectShowUtility>();
            _buttons[9] = buttonController;
            Vector2 relativeCenterPos = new Vector2(6, 7) / 9;
            buttonRect.position = (Vector3)gridButtonsRect.rect.position + gridButtonsRect.position + (Vector3)(relativeCenterPos * gridButtonsRect.rect.size * 1.01f);
            buttonRect.rotation = Quaternion.Euler(0, 0, 45);
            buttonController.InitButton(0);
            rectShowUtility.ShowIn(true, 1f);
        }

        public IEnumerator SetUnchangeableCells(uint[] unchangeableArray)
        {
            for(int i = 0; i < 81; i++)
            {
                if (unchangeableArray[i] != 0)
                {
                    yield return new WaitForSeconds(.04f);
                    SetCell(i, unchangeableArray[i]);
                }
            }
            OnUnchangedCellsSet.Invoke();
            GameManager.I.SetLevelName();
            GameManager.I.TimerStart();
        }

        public void SetCell(int index, uint number, Enums.IndexType indexType = Enums.IndexType.Unchangeable)
        {
            _cells[index].SetCellNumber(number, indexType);
        }

        public bool CheckCellChangeability(int index)
        {
            return !_cells[index].isUnchangeable;
        }

        public void SelectCell(int index)
        {
            if(_selectedCellIndex != -1)
                _cells[_selectedCellIndex].Deselect();
            _selectedCellIndex = index;
            _cells[_selectedCellIndex].Select();
            
            HighlightArea(index);
        }

        public void DeselectCells()
        {
            if(_selectedCellIndex != -1)
                _cells[_selectedCellIndex].Deselect();
            _selectedCellIndex = -1;
            HighlightArea(_selectedCellIndex);
        }

        public void HideButton(uint number)
        {
            _buttons[number - 1].Hide();
        }

        public void AlertConflicts(int[] conflicts)
        {
            if (conflicts[0] != -1)
            {
                _cells[conflicts[0]].AlertConflict();
            }
            if (conflicts[1] != -1)
            {
                _cells[conflicts[1]].AlertConflict();
            }
            if (conflicts[2] != -1)
            {
                _cells[conflicts[2]].AlertConflict();
            }
        }
        
        public void AlertCorrects(int[] corrects)
        {
            foreach (int i in corrects)
            {
                _cells[i].AlertCorrect();
            }
        }
        
        public void AlertButton(uint number)
        {
            _buttons[(number + 9)%10].Alert();
        }
        
        public void CloseGrid()
        {
            for(int i = 80; i >= 0; i--)
            {
                _cells[i].DestroyCell();
            }
            _cells = null;
            
            for(int i = 9; i >= 0; i--)
            {
                _buttons[i].DestroyButton();
            }

            _buttons = null;
            HighlightArea(-1);
            OnGridReady = null;
            OnUnchangedCellsSet = null;
            InputManager.I.ClearEvents();
            ThemeManager.I.SudokuPanelSwitch(false);
        }
        
        #region Area Highlights

        [Header("Area Highlights & Separators")]
        
        public RectTransform squareHighlightRect;
        public RectTransform rowHighlightRect;
        public RectTransform columnHighlightRect;
        public Image squareHighlightImage;
        public Image rowHighlightImage;
        public Image columnHighlightImage;
        private bool _areHighlightsActive;

        public RectShowUtility leftSeparator;
        public RectShowUtility rightSeparator;
        public RectShowUtility topSeparator;
        public RectShowUtility bottomSeparator;
        
        private void FadeImage(Image image, bool fadeIn)
        {
            if (fadeIn)
            {
                image.color = ThemeManager.I.areaHighlightColor;
            }
            else
            {
                image.color = Color.clear;
            }
        }

        private void InitHighlighters()
        {
            squareHighlightImage.color = Color.clear;
            rowHighlightImage.color = Color.clear;
            columnHighlightImage.color = Color.clear;
            Rect sqrRect = GetSquareRect(0);
            squareHighlightRect.position = sqrRect.position;
            squareHighlightRect.sizeDelta = sqrRect.size;
            Rect rowRect = GetRowRect(0);
            rowHighlightRect.position = rowRect.position;
            rowHighlightRect.sizeDelta = rowRect.size;
            Rect colRect = GetColumnRect(0); 
            columnHighlightRect.position = colRect.position;
            columnHighlightRect.sizeDelta = colRect.size;
        }
        
        private void InitSeparators()
        {
            leftSeparator.Setup();
            rightSeparator.Setup();
            topSeparator.Setup();
            bottomSeparator.Setup();
            Vector3 gridPos = gridRect.position + (Vector3)gridRect.rect.position;
            leftSeparator.SetScalePos((Vector2)gridPos + new Vector2(1, 1)/3 * gridRect.rect.size, RectShowUtility.OpDirection.Down);
            rightSeparator.SetScalePos((Vector2)gridPos + new Vector2(2, 2)/3 * gridRect.rect.size, RectShowUtility.OpDirection.Down);
            topSeparator.SetScalePos((Vector2)gridPos + new Vector2(1, 2)/3 *gridRect.rect.size, RectShowUtility.OpDirection.Right);
            bottomSeparator.SetScalePos((Vector2)gridPos + new Vector2(2, 1)/3 *gridRect.rect.size, RectShowUtility.OpDirection.Right);
            leftSeparator.Init();
            rightSeparator.Init();
            topSeparator.Init();
            bottomSeparator.Init();
            leftSeparator.ShowIn(true, .4f);
            rightSeparator.ShowIn(true, .7f);
            topSeparator.ShowIn(true, .8f);
            bottomSeparator.ShowIn(true, .85f);
        }

        private void HideSeparators()
        {
            leftSeparator.ShowIn(false, .4f);
            rightSeparator.ShowIn(false, .7f);
            topSeparator.ShowIn(false, .8f);
            bottomSeparator.ShowIn(false, .85f);
        }

        public void HighlightArea(int cellIndex)
        {
            if (cellIndex == -1)
            {
                if (!_areHighlightsActive)
                {
                    return;
                }
                FadeImage(squareHighlightImage, false);
                FadeImage(rowHighlightImage, false);
                FadeImage(columnHighlightImage, false);
                _areHighlightsActive = false;
            }
            else
            {
                if (_areHighlightsActive)
                {
                    StartCoroutine(MoveHighlightsToCell(cellIndex));
                }
                else
                {
                    MoveHighlightsToCellInstant(cellIndex);
                    FadeImage(squareHighlightImage, true);
                    FadeImage(rowHighlightImage, true);
                    FadeImage(columnHighlightImage, true);
                    _areHighlightsActive = true;
                }
            }
        }

        private void MoveHighlightsToCellInstant(int cellIndex)
        {
            rowHighlightRect.position = GetRowRect(cellIndex).position;
            columnHighlightRect.position = GetColumnRect(cellIndex).position;
            squareHighlightRect.position = GetSquareRect(cellIndex).position;
        }
        
        //Regrettable spagetti, devoid from any flexibility. But it works. probably.
        private IEnumerator MoveHighlightsToCell(int cellIndex)
        {
            float moveDuration = 0.12f;
            Vector2 currentRowPos = rowHighlightRect.position;
            Vector2 currentColPos = columnHighlightRect.position;
            Vector2 targetRowPos = GetRowRect(cellIndex).position;
            Vector2 targetColPos = GetColumnRect(cellIndex).position;
            Vector2 targetSqrPos = GetSquareRect(cellIndex).position;
            Color squareHighlightClearColor = ThemeManager.I.areaHighlightColor;
            squareHighlightClearColor.a = 0;
            bool squareNotMoved = true;
            float t = 0;
            while (t < moveDuration)
            {
                rowHighlightRect.position = Vector2.Lerp(currentRowPos, targetRowPos, t / moveDuration);
                columnHighlightRect.position = Vector2.Lerp(currentColPos, targetColPos, t / moveDuration);
                squareHighlightImage.color = Color.Lerp(squareHighlightClearColor, ThemeManager.I.areaHighlightColor,
                    (t / moveDuration - .5f) * 2);
                t += Time.deltaTime;
                if (squareNotMoved)
                {
                    if (t > moveDuration / 2)
                    {
                        squareNotMoved = false;
                        squareHighlightRect.position = targetSqrPos;
                    }
                }

                yield return null;
            }
            rowHighlightRect.position = targetRowPos;
            columnHighlightRect.position = targetColPos;
        }
        
        public Rect GetSquareRect(int index)
        {
            int squareIndex = index / 27 * 3 + (index % 9) / 3;
            Rect squareRect = new Rect();
            squareRect.position = (Vector3)gridRect.rect.position + gridRect.position + (Vector3)(new Vector2(squareIndex % 3, squareIndex / 3) * gridRect.rect.size / 3);
            squareRect.size = new Vector2(gridRect.rect.width / 3, gridRect.rect.height / 3);
            return squareRect;
        }
        
        public Rect GetRowRect(int index)
        {
            Rect rowRect = new Rect();
            rowRect.position = (Vector3)gridRect.rect.position + gridRect.position + (Vector3)(new Vector2(0, index / 9) * gridRect.rect.size / 9);
            rowRect.size = new Vector2(gridRect.rect.width, gridRect.rect.height / 9);
            return rowRect;
        }
        
        public Rect GetColumnRect(int index)
        {
            Rect columnRect = new Rect();
            columnRect.position = (Vector3)gridRect.rect.position + gridRect.position + (Vector3)(new Vector2(index % 9, 0) * gridRect.rect.size / 9);
            columnRect.size = new Vector2(gridRect.rect.width / 9, gridRect.rect.height);
            return columnRect;
        }

        #endregion

        #region Utility Methods

        public static int PositionToCellIndex(Vector2 normalizedPosition)
        {
            int row = (int)(normalizedPosition.x * 9);
            int column = (int)(normalizedPosition.y * 9);
            return row + column * 9;
        }

        public static float GetMDistanceBetweenCells(int index1, int index2)
        {
            return index1 % 9 - index2 % 9 + index1 / 9 - index2 / 9;
        }

        #endregion
    }
}
