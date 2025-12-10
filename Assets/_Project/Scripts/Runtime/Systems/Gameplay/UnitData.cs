using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Gameplay
{
    // Minimal unit identity for shop/bench/combines
    public sealed class UnitData : MonoBehaviour
    {
        public string UnitName;
        public int Cost;
        public int Star = 1;
    }
}

