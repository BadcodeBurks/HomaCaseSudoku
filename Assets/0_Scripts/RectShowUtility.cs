using System.Collections;
using System.Collections.Generic;
using Sudoku;
using UnityEngine;

public class RectShowUtility : MonoBehaviour
{
    public enum OpDirection
    {
        Down,
        Right
    }
    
    private Vector2 _originPos;
    private Vector2 _originSize;
    private Vector2 _scaleDownPos;
    private Vector2 _scaleDownSize;
    private Vector2 _hidePos;
    private OpDirection _hideDirection;
    private OpDirection _ScaleDirection;

    public bool startHidden;
    public bool startScaled;

    private RectTransform _rectTransform;

    public void Setup()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originPos = _rectTransform.position;
        _originSize = _rectTransform.sizeDelta;
    }
    public void Init()
    {
        if(startHidden) InitHide();
        else if(startScaled) InitScale();
    }
    
    private void InitHide()
    {
        _rectTransform.position = _hidePos;
    }

    private void InitScale()
    {
        _rectTransform.position = _scaleDownPos;
        _rectTransform.sizeDelta = _scaleDownSize;
    }
    
    public void SetScalePos(Vector2 pos, OpDirection dir)
    {
        _scaleDownPos = pos;
        _ScaleDirection = dir;
        _scaleDownSize = _ScaleDirection == OpDirection.Down ? new Vector2(_originSize.x, -_rectTransform.rect.size.y) : new Vector2(-_rectTransform.rect.size.x, _originSize.y);
    }
    
    public void SetHideDir(OpDirection dir)
    {
        _hideDirection = dir;
        _hidePos.x = _hideDirection == OpDirection.Down ? _rectTransform.position.x : (Screen.width + _rectTransform.sizeDelta.x);
        _hidePos.y = _hideDirection == OpDirection.Right ? _rectTransform.position.y : -_rectTransform.sizeDelta.y;
    }

    public void ShowIn(bool show, float waitTime = 0)
    {
        if(startHidden)Show(show, waitTime);
        else if(startScaled)Scale(show, waitTime);
    }
    
    
    public void Show(bool show, float waitTime = 0)
    {
        StartCoroutine(ShowRoutine(show, waitTime));
    }
    
    public void Scale(bool show, float waitTime = 0)
    {
        StartCoroutine(ScaleRoutine(show, waitTime));
    }

    private IEnumerator ShowRoutine(bool show,float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);
        Vector2 startPos = show ? _hidePos : _originPos;
        Vector2 endPos = show ? _originPos : _hidePos;
        float t = 0;
        while (t < .5)
        {
            t += Time.deltaTime;
            _rectTransform.position = Vector2.Lerp(startPos, endPos, GridManager.I.movementCurve.Evaluate(t*2));
            yield return null;
        }
        _rectTransform.position = endPos;
    }

    private IEnumerator ScaleRoutine(bool show, float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);
        Vector2 startPos = show ? _scaleDownPos : _originPos;
        Vector2 endPos = show ? _originPos : _scaleDownPos;
        Vector2 startSize = show ? _scaleDownSize : _originSize;
        Vector2 endSize = show ? _originSize : _scaleDownSize;
        float t = 0;
        while (t < .5)
        {
            t += Time.deltaTime;
            float p = GridManager.I.movementCurve.Evaluate(t * 2);
            _rectTransform.position = Vector2.Lerp(startPos, endPos, p);
            _rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, p);
            yield return null;
        }
        _rectTransform.position = endPos;
        _rectTransform.sizeDelta = endSize;
    }
}
