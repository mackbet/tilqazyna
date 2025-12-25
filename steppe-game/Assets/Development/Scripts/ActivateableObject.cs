using System;
using UnityEngine;

[Serializable]
public class ActivateableObject
{
    [SerializeField] private GameObject gameObject;
    [SerializeField] private State state;

    public void Call()
    {
        if (state == State.Activate)
            gameObject.SetActive(true);
        else if (state == State.Deactivate)
            gameObject.SetActive(false);
        else if (state == State.Switch)
            gameObject.SetActive(!gameObject.activeInHierarchy);

    }
    [Serializable]
    public enum State
    {
        None,
        Activate,
        Deactivate,
        Switch
    }
}
