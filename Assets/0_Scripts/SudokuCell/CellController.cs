using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

namespace Sudoku
{
    public class CellController : MonoBehaviour
    {
        public static Action<uint, int> OnNumberFinished;
        public static Action<uint, int> OnSelectedNumberChanged;

        public static int SelectedIndex = -1;
        public static uint SelectedNumber
        {
            set
            {
                OnSelectedNumberChanged?.Invoke(value, SelectedIndex);
            }
        }
        
        public static void ClearEvents()
        {
            OnNumberFinished = null;
            OnSelectedNumberChanged = null;
        }
    
        public TextMeshProUGUI mainNumber;
        public Image bgImage;
        public uint currentNumber;
        public bool isUnchangeable;
        private int _cellIndex;
        private bool _isHighlighted;
        
        private Coroutine _highlightRoutine;
        private bool _currentlyInRoutine;
    
        public void InitCell(int index)
        {
            _cellIndex = index;
            SetCellNumber(0);
            HighlightCell(false, true);
            OnSelectedNumberChanged += SelectedNumberChanged;
            OnNumberFinished += OnAlertNumberFinished;
        }
    
        public void Select()
        {
            if (currentNumber == 0)
            {
                HighlightCell(true);
            }
            SelectedIndex = (int)_cellIndex;
            SelectedNumber = currentNumber;
        }
    
        public void Deselect()
        {
            SelectedNumber = 0;
            HighlightCell(false);
        }
        
        public void HighlightCell(bool on, bool isInstant = false)
        {
            _isHighlighted = on;
            if (isInstant)
            {
                bgImage.color = on ? ThemeManager.I.cellHighlightColor : Color.clear;
                return;
            }
            if(_currentlyInRoutine)
                StopCoroutine(_highlightRoutine);
        
            _highlightRoutine = StartCoroutine(SetBGColorRoutine(0, on ? ThemeManager.I.cellHighlightColor : Color.clear));
            _currentlyInRoutine = true;
        }

        public void HighlightSameNumber(bool on, float waitTime = 0f, bool isInstant = false)
        {
            _isHighlighted = on;
            if (isInstant)
            {
                bgImage.color = on ? ThemeManager.I.cellSameNumberHighlightColor : Color.clear;
                return;
            }
            if(_currentlyInRoutine)
                StopCoroutine(_highlightRoutine);
            _highlightRoutine = StartCoroutine(SetBGColorRoutine(waitTime, on ? ThemeManager.I.cellSameNumberHighlightColor : Color.clear));
            _currentlyInRoutine = true;
        }
    
        public void SetCellNumber(uint number, Enums.IndexType type = Enums.IndexType.Unchangeable)
        {
            if (number == 0)
            {
                mainNumber.text = "";
                currentNumber = 0;
                return;
            }
            
            currentNumber = number;
            mainNumber.text = number.ToString();
            mainNumber.color = ThemeManager.I.GetTextColor(type);
            if (type == Enums.IndexType.Unchangeable)
                SetCellUnchangeable();
        }
    
        public void SetCellUnchangeable()
        {
            isUnchangeable = true;
        }
    
        private void SelectedNumberChanged(uint cellNumber, int index)
        {
            bool willHighlight = cellNumber == currentNumber;
            if(currentNumber == 0)
            {
                // if(_isHighlighted && !willHighlight)
                //     HighlightCell(false);
                return;
            }

            if (_isHighlighted == willHighlight)
            {
                return;
            }
            float t = GridManager.GetMDistanceBetweenCells(index, _cellIndex) * .02f;
            HighlightSameNumber(willHighlight, t);
        }

        private IEnumerator SetBGColorRoutine(float waitTime, Color color)
        {
            yield return new WaitForSeconds(waitTime);
            Color startColor = bgImage.color;
            float t = 0;
            float changeDuration = .12f;
            while (t < changeDuration)
            {
                t += Time.deltaTime;
                bgImage.color = Color.Lerp(startColor, color, t / changeDuration);
                yield return null;
            }
            bgImage.color = color;
            _currentlyInRoutine = false;
        }

        private void OnAlertNumberFinished(uint number, int index)
        {
            if (number == currentNumber)
            {
                float t = GridManager.GetMDistanceBetweenCells(index, _cellIndex) * .02f;
                StartCoroutine(AlertCorrectInTime(t));
            }
        }

        public IEnumerator AlertCorrectInTime(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            AlertCorrect();
        }

        public void AlertConflict()
        {
            ParticleManager.SpawnParticle(ParticleManager.ParticleType.Incorrect, transform.position,Vector2.up * 50, 80,
                Vector2.up * 100);
        }

        public void AlertCorrect()
        {
            ParticleManager.SpawnParticle(ParticleManager.ParticleType.Correct, transform.position,Vector2.up * 50, 80,
                Vector2.up * 100);
        }

        public void DestroyCell()
        {
            ClearEvents();
            OnSelectedNumberChanged -= SelectedNumberChanged;
            OnNumberFinished -= OnAlertNumberFinished;
            Destroy(gameObject, Random.Range(.5f, 2f));
        }
    }
}

