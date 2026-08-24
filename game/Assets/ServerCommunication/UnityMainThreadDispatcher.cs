using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;

    private readonly Queue<Action> actions = new Queue<Action>();

    void Awake()
    {
        instance = this;
    }

    public static void Enqueue(Action action)
    {
        if (instance == null)
        {
            Debug.LogError(
                "UnityMainThreadDispatcher não existe na cena!"
            );

            return;
        }

        lock (instance.actions)
        {
            instance.actions.Enqueue(action);
        }
    }

    void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                Action action = actions.Dequeue();

                if (action != null)
                {
                    action.Invoke();
                }
            }
        }
    }
}