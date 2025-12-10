using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Core
{
    // Deterministic RNG using SplitMix64 with named streams
    public static class DeterministicRng
    {
        public enum Stream
        {
            Loot = 0,
            Shop = 1,
            Targeting = 2,
            Projectile = 3,
            Carousel = 4
        }

        [Serializable]
        private class SeedConfig
        {
            public ulong seed = 1UL;
        }

        private struct SplitMix64
        {
            public ulong State;

            public SplitMix64(ulong seed)
            {
                State = seed;
            }

            public ulong NextUInt64()
            {
                ulong z = (State += 0x9E3779B97F4A7C15UL);
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }

            public uint NextUInt32()
            {
                // Use high 32-bits for quality
                return (uint)(NextUInt64() >> 32);
            }
        }

        private static bool _initialized;
        private static ulong _baseSeed = 1UL;
        private static readonly SplitMix64[] _streams = new SplitMix64[5];
        private static readonly object _lock = new object();

        public static ulong CurrentSeed => _baseSeed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit()
        {
            InitFromConfig();
        }

        public static void InitFromConfig()
        {
            if (_initialized) return;
            ulong seed = 1UL;
            try
            {
                var asset = Resources.Load<TextAsset>("match_seed");
                if (asset != null && !string.IsNullOrWhiteSpace(asset.text))
                {
                    var cfg = JsonUtility.FromJson<SeedConfig>(asset.text);
                    if (cfg != null) seed = cfg.seed;
                }
            }
            catch (Exception)
            {
                // Fallback to default seed if anything goes wrong
            }
            SetSeed(seed);
        }

        public static void SetSeed(ulong seed)
        {
            lock (_lock)
            {
                _baseSeed = seed;
                // Derive unique stream seeds using a simple mixing with stream index constants
                for (int i = 0; i < _streams.Length; i++)
                {
                    ulong mixed = Mix64(seed + (ulong)(0x9E3779B185EBCA87UL * (ulong)(i + 1)));
                    _streams[i] = new SplitMix64(mixed);
                }
                _initialized = true;
            }
        }

        private static ulong Mix64(ulong x)
        {
            // Jenkins-like mix (same avalanche as SplitMix seeding)
            x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
            x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
            x = x ^ (x >> 31);
            return x;
        }

        private static SplitMix64 GetStream(Stream s)
        {
            if (!_initialized) InitFromConfig();
            return _streams[(int)s];
        }

        private static void SetStream(Stream s, SplitMix64 value)
        {
            _streams[(int)s] = value;
        }

        // Public APIs
        public static int NextInt(Stream s, int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            uint range = (uint)(maxExclusive - minInclusive);
            lock (_lock)
            {
                var sm = GetStream(s);
                uint limit = uint.MaxValue - (uint.MaxValue % range);
                uint r;
                do { r = sm.NextUInt32(); } while (r >= limit);
                SetStream(s, sm);
                return minInclusive + (int)(r % range);
            }
        }

        public static float NextFloat01(Stream s)
        {
            lock (_lock)
            {
                var sm = GetStream(s);
                // 24-bit mantissa precision to [0,1)
                uint r = sm.NextUInt32();
                SetStream(s, sm);
                return ((r >> 8) & 0x00FFFFFF) * (1.0f / 16777216.0f);
            }
        }

        public static float NextFloat(Stream s, float minInclusive, float maxExclusive)
        {
            float t = NextFloat01(s);
            return minInclusive + (maxExclusive - minInclusive) * t;
        }
    }
}

