using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Bootstrap
{
    public static class GameEntry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (Object.FindObjectOfType<UIBootstrap>() == null)
            {
                var go = new GameObject("_Bootstrap");
                go.AddComponent<UIBootstrap>();
                Object.DontDestroyOnLoad(go);
            }
        }
    }
}

