using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;

    public static void Run(IEnumerator coroutine)
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("CoroutineRunner");
            go.hideFlags = HideFlags.HideAndDontSave;
            _instance = go.AddComponent<CoroutineRunner>();
        }

        _instance.StartCoroutine(coroutine);
    }
}
