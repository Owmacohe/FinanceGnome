using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FGUndoController : MonoBehaviour
{
    public static FGUndoController Instance;

    Stack<Action> undoStack = new();
    
    void Start()
    {
        Instance = this;
    }

    public void SaveUndo(Action undo) => undoStack.Push(undo);

    void Undo()
    {
        if (undoStack.Count > 0)
            undoStack.Pop()?.Invoke();
    }
    
    #region Input
    
    bool control;

    public void OnControl(InputValue value) => control = value.isPressed;
    
    public void OnZ()
    {
        if (control) Undo();
    }
    
    #endregion
}