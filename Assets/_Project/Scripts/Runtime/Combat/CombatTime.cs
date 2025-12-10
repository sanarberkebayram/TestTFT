using UnityEngine;

namespace TestTFT.Scripts.Runtime.Combat
{
    public static class CombatTime
    {
        public const float OvertimeThresholdSeconds = 15f;

        private static bool _started;
        private static float _startTime;
        private static bool _forceOvertime;

        public static void EnsureStarted()
        {
            if (_started) return;
            _started = true;
            _startTime = Time.timeSinceLevelLoad;
        }

        public static void ResetForTests(float startTime = 0f)
        {
            _started = true;
            _startTime = startTime;
            _forceOvertime = false;
        }

        public static void ForceOvertime(bool value)
        {
            _forceOvertime = value;
        }

        public static bool IsOvertime()
        {
            if (_forceOvertime) return true;
            if (!_started) EnsureStarted();
            return (Time.timeSinceLevelLoad - _startTime) >= OvertimeThresholdSeconds;
        }

        public static float GetOvertimeDamageMultiplier()
        {
            return IsOvertime() ? 1.5f : 1f;
        }
    }
}

