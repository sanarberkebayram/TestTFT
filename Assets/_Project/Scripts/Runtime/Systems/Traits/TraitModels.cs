using System;
using System.Collections.Generic;

namespace TestTFT.Scripts.Runtime.Systems.Traits
{
    [Serializable]
    public sealed class TraitEffect
    {
        public string stat;   // e.g., "attackDamage", "attackSpeed"
        public float value;   // percentage or flat depending on stat semantics
    }

    [Serializable]
    public sealed class TraitBreakpoint
    {
        public int threshold;           // units required
        public TraitEffect[] effects;   // effects granted at this breakpoint
    }

    [Serializable]
    public sealed class TraitDef
    {
        public string id;               // unique id, e.g., "warrior"
        public string name;             // display name
        public TraitBreakpoint[] breakpoints;
        public bool emblemAvailable;    // basic flag for MVP
    }

    [Serializable]
    public sealed class TraitDatabaseData
    {
        public TraitDef[] traits;
    }

    public sealed class TraitState
    {
        public string id;
        public string name;
        public int count;               // current unique unit count contributing
        public int achievedIndex;       // -1 if none, otherwise index in breakpoints
        public TraitEffect[] bonuses;   // current effects (null or empty if none)
    }

    public sealed class TraitsComputed
    {
        public List<TraitState> states = new List<TraitState>();
    }
}

