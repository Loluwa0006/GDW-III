using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class BufferHelper : MonoBehaviour
{

    List<InputAction> actions = new();
    [SerializeField] List<string> inputNames = new();
    [SerializeField] bool isHoldable = false;
    [SerializeField] private int defaultDuration = 8;
    int currentDuration = 0;


    bool initialized = false;

    int window = 0;
    public bool Buffered => window > 0;

    string actionBuffered = "";

    public void InitBuffer(PlayerInput pInput)
    {
        if (initialized)
        {
            return;
        }

        foreach (string input in inputNames)
        {
            InputAction action = pInput.actions.FindAction(input);
            if (action == null)
            {
                Debug.LogWarning("Could not find action of name " + input + " in player input.");
                continue;
            }
            actions.Add(action);
        }
        currentDuration = defaultDuration;
        initialized = true;

    }

    private void Update()
    {
        if (initialized)
        {
            foreach (InputAction action in actions)
            {
                if (action.WasPerformedThisFrame() || isHoldable && action.IsPressed())
                {
                    actionBuffered = action.name;
                    window = currentDuration;
                    break;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (initialized && window > 0)
        {
            window--;
            if (window <= 0)
            {
              actionBuffered = "";
            }
        }
    } 

    public void Consume()
    {
        window = 0;
        actionBuffered = "";
    }

    public string GetBufferedInput()
    {
        return actionBuffered;
    }

    public void OverrideDuration(int duration)
    {
        currentDuration = duration;
    }

    public void ResetOverrideDuration()
    {
        currentDuration = defaultDuration;
    }
}
