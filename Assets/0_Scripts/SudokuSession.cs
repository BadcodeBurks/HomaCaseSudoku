using System;
using UnityEngine;

namespace Sudoku
{
    public class SudokuSession
    {
        public Action<int> OnCellSelected;
        public Action OnLevelCompleted;

        private int _selectedCellIndex = -1;
        private uint _selectedCellNumber;
        
        private uint[] _solution;
        private uint[] _currentCells;
        private int[]_numberCounts;
        private int _correctCount;
        private int _wrongCount;
        private readonly Level _level;
        private RectTransform _gridRect;
        private bool sessionEnd = false;
        
        private bool _isCheckingInput;
        private bool _hasSelectedCell;
        private bool _isSelecting;

        public SudokuSession(Level level)
        {
            _isSelecting = false;
            _level = level;
            _solution = level.GetLevelAsArray();
            _currentCells = new uint[81];
            _correctCount = level.givenCellIndices.Length;
            _numberCounts = new int[9];
            foreach (int index in level.givenCellIndices)
            {
                uint value = _solution[index];
                _currentCells[index] = value;
                _numberCounts[value - 1]++;
            }
        }

        public void StartSession()
        {
            GridManager.I.OnUnchangedCellsSet += RegisterInputEvents;
            GridManager.I.StartCoroutine(GridManager.I.SetUnchangeableCells(_currentCells));
            _gridRect = GridManager.I.gridRect;
        }

        public void EndSession()
        {
            GridManager.I.OnUnchangedCellsSet -= RegisterInputEvents;
            UnregisterInputEvents();
        }
        
        private void RegisterInputEvents()
        {
            InputManager.I.OnTouchBegin += RegisterCheckInput;
            InputManager.I.OnTouchEnd += UnregisterCheckInput;
            InputManager.I.OnButtonClick += OnButtonClick;
        }

        public void UnregisterInputEvents()
        {
            InputManager.I.OnTouchBegin -= RegisterCheckInput;
            InputManager.I.OnTouchEnd -= UnregisterCheckInput;
            InputManager.I.OnButtonClick -= OnButtonClick;
        }

        private void RegisterCheckInput(Vector3 inputPos)
        {
            InputManager.I.OnTouchPosChanged += CheckInput;
            if (IsScreenPointInGrid(inputPos, out Vector2 pos))
            {
                _isSelecting = true;
            }
        }
        private void UnregisterCheckInput(Vector3 inputPos)
        {
            _isSelecting = false;
            InputManager.I.OnTouchPosChanged -= CheckInput;
            if (!IsScreenPointInGrid(inputPos, out Vector2 pos) && _selectedCellIndex != -1)
            {
                if (sessionEnd) return;
                GridManager.I.DeselectCells();
                if (_currentCells[_selectedCellIndex] != _selectedCellNumber)
                {
                    SelectCell(_selectedCellIndex);
                }
                else
                {
                    _selectedCellIndex = -1;
                    _hasSelectedCell = false;
                }
            }
        }

        private void CheckInput(Vector3 inputPos)
        {
            Vector2 pos;
            _isSelecting = IsScreenPointInGrid(inputPos, out pos) || _isSelecting;
            if(!_isSelecting) return;
            int touchIndex = GridManager.PositionToCellIndex(pos);
            if (touchIndex == _selectedCellIndex) return;
            _selectedCellIndex = touchIndex;
            SelectCell(touchIndex);
        }
        
        private bool IsScreenPointInGrid(Vector3 screenPos, out Vector2 pos)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_gridRect, screenPos, null, out pos))
            {
                pos = (pos - _gridRect.rect.min) / _gridRect.rect.size;
                Vector2 prevPos = pos;
                pos.x = Mathf.Clamp(pos.x, 0, 0.999f);
                pos.y = Mathf.Clamp(pos.y, 0, 0.999f);
                return pos == prevPos;
            }

            return false;
        }
        
        private void SelectCell(int index)
        {
            GridManager.I.SelectCell(index);
            _selectedCellIndex = index;
            _hasSelectedCell = true;
            OnCellSelected?.Invoke(index);
            _selectedCellNumber = _currentCells[index];
        }

        private void OnButtonClick(uint number)
        {
            if (_selectedCellIndex != -1)
            {
                if (GridManager.I.CheckCellChangeability(_selectedCellIndex))
                {
                    bool wasCorrect = CheckIndexCorrect(_selectedCellIndex);
                    if(wasCorrect) return; //Trigger event?
                    if (number == _selectedCellNumber) number = 0;
                    _currentCells[_selectedCellIndex] = number;
                    bool isEmpty = number == 0;
                    bool isCorrect = CheckIndexCorrect(_selectedCellIndex);
                    GridManager.I.SetCell(_selectedCellIndex, number, isCorrect ? Enums.IndexType.Default : Enums.IndexType.Incorrect);
                    if (isEmpty)
                        return;
                    if (isCorrect)
                    {
                        _correctCount++;
                        _numberCounts[number - 1]++;
                        int[] correctIndices = new int[1];
                        correctIndices[0] = _selectedCellIndex;
                        
                        if (_correctCount == 81)
                        {
                            
                            sessionEnd = true;
                            OnLevelCompleted?.Invoke();
                            OnCellSelected = null;
                            OnLevelCompleted = null;
                            return;
                        }

                        if (CheckNumberCount(number))
                        {
                            CellController.OnNumberFinished.Invoke(number, _selectedCellIndex);
                            GridManager.I.HideButton(number);
                        }
                        else GridManager.I.AlertCorrects(correctIndices);
                    }
                    else
                    {
                        int[] conflictIndices = new int[4];
                        conflictIndices[0]= _selectedCellIndex;
                        CheckRow(_selectedCellIndex, out int conflictID);
                        conflictIndices[1] = conflictID;
                        CheckColumn(_selectedCellIndex, out conflictID);
                        conflictIndices[2] = conflictID;
                        CheckSquare(_selectedCellIndex, out conflictID);
                        conflictIndices[3] = conflictID;
                        GridManager.I.AlertConflicts(conflictIndices);
                    }
                }
                else
                {
                    GridManager.I.AlertButton(number);
                }
            }
            else
            {
                GridManager.I.AlertButton(number);
            }
        }
        
        private bool CheckRow(int index, out int conflictID)
        {
            int row = index / 9;
            for (int i = 0; i < 9; i++)
            {
                int id = row * 9 + i;
                if (id == index) continue;
                if (_currentCells[id] == _currentCells[index])
                {
                    conflictID = id;
                    return false;
                }
            }
            conflictID = -1;
            return true;
        }
        
        private bool CheckColumn(int index, out int conflictID)
        {
            int column = index % 9;
            for (int i = 0; i < 9; i++)
            {
                int id = column + i * 9;
                if (id == index) continue;
                if (_currentCells[id] == _currentCells[index])
                {
                    conflictID = id;
                    return false;
                }
            }
            conflictID = -1;
            return true;
        }
        
        private bool CheckSquare(int index, out int conflictID)
        {
            int square = index / 27 * 3 + index % 9 / 3;
            for (int i = 0; i < 9; i++)
            {
                int id = square / 3 * 27 + square % 3 * 3 + i / 3 * 9 + i % 3;
                if (id == index) continue;
                if (_currentCells[id] == _currentCells[index])
                {
                    conflictID = id;
                    return false;
                }
            }
            conflictID = -1;
            return true;
        }

        private bool CheckNumberCount(uint number)
        {
            return _numberCounts[number - 1] >= 9;
        }
        
        private bool CheckIndexCorrect(int index)
        {
            return _currentCells[index] == _solution[index];
        }
    }
}