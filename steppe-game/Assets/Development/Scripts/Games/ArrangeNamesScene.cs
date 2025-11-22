using System;
using UnityEngine;

public class ArrangeNamesScene : MonoBehaviour
{
    [SerializeField] private ArrangeNamesSocket[] sockets;
    private int count = 0;

    public event Action OnSceneCompleted;

    private void OnEnable()
    {
        foreach (ArrangeNamesSocket socket in sockets)
        {
            socket.OnSocketEntered += SocketEntered;
        }
    }

    private void OnDisable()
    {
        foreach (ArrangeNamesSocket socket in sockets)
        {
            socket.OnSocketEntered -= SocketEntered;
        }
    }

    private void SocketEntered(ArrangeNamesSocket socket)
    {
        socket.OnSocketEntered -= SocketEntered;
        count++;

        if (count == sockets.Length)
            OnSceneCompleted?.Invoke();
    }
}
