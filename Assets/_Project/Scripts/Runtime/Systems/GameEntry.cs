using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems
{
    public static class GameEntry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("UIBootstrap");
            go.hideFlags = HideFlags.DontSave;
            Object.DontDestroyOnLoad(go);
            go.AddComponent<TestTFT.Scripts.Runtime.UI.UIHudShop>();
        }
    }
}

