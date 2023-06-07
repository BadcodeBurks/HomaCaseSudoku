using System;
using Burk.Core;
using UnityEngine;

public class InputManager : MonoBehaviourSingleton<InputManager>
{
    public Action<Vector3> OnTouchPosChanged;
    public Action<Vector3> OnTouchBegin;
    public Action<Vector3> OnTouchEnd;
    public Action<uint> OnButtonClick;
    public Vector3 TouchPos { get; private set; }

    private void Update()
    {
        Vector3 oldPos = TouchPos;
#if UNITY_EDITOR
        
        if (Input.GetMouseButtonDown(0))
        {
            TouchPos = Input.mousePosition;
            OnTouchBegin?.Invoke(TouchPos);
        }
        else if (Input.GetMouseButton(0))
        {
            TouchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            TouchPos = Input.mousePosition;
            OnTouchEnd?.Invoke(TouchPos);
        }

#else
        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                TouchPos = touch.position;
                OnTouchBegin?.Invoke(TouchPos);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                TouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                TouchPos = touch.position;
                OnTouchEnd?.Invoke(TouchPos);
            }
        }
        
#endif
        if (TouchPos != oldPos)
        {
            OnTouchPosChanged?.Invoke(TouchPos);
        }
    }

    public void ClearEvents()
    {
        OnTouchPosChanged = null;
        OnTouchBegin = null;
        OnTouchEnd = null;
        OnButtonClick = null;
    }
    
    public void OnSudokuButtonClick(uint number)
    {
        OnButtonClick?.Invoke(number);
    }
}