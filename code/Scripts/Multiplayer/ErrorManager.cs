using System;
using System.Collections.Generic;
using UnityEngine;


public class ErrorManager : MonoBehaviour
{
    public static ErrorManager Instance { get; private set; }
    private readonly List<string> _errors = new();
    public IReadOnlyList<string> Errors => _errors;

    public event Action OnErrorsChanged;


    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; } // destroy the duplicate instance
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddError(string msg)
    {
        _errors.Add(msg);
        OnErrorsChanged?.Invoke();
    }

    public void DismissError(string msg)
    {
        _errors.Remove(msg);
        OnErrorsChanged?.Invoke();
    }

    public void ClearErrors()
    {
        _errors.Clear();
        OnErrorsChanged?.Invoke();
    }
}