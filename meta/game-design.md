# Team Fight Tactics — Game Design (Lean)

Scope:
- Auto-battler inspired design for internal prototype; not Riot IP.
- Follow `meta/gdd-protocol.md` (no contradictions; mark TBDs explicitly).

Pillars:
- Strategic drafting, economy mastery, compositional synergies.
- Automated combat clarity; readable spells and outcomes.
- High variability across runs with controlled RNG.

Core Loop:
- Draft units/items → arrange board → auto-combat → resolve economy → level/roll → repeat.

Match Structure:
- Players: 8 (FFA), shared round pacing.
- Health: Start [TBD] HP; lose HP on round loss by opponent board strength.
- Rounds: PvE opener → PvP cycles → Carousel intervals → Boss/loot rounds as milestones.
- Victory: Last player standing.

Board & Units:
- Grid: Hex or square 7x4-ish footprint (exact dims TBD); front/back rows matter.
- Bench: Holds [TBD] units; overflow sells or blocks buys.
- Unit Tiers: 1–5 cost; rarer tiers have lower shop odds (table per level).
- Star-Upgrades: Combine 3 copies → +star; 3x 2-star → 3-star.
- Traits: Each unit has 1–3 traits; breakpoints grant bonuses (scaling TBD).
- Stats: HP, AD, AS, Armor, MR, Crit, Mana, Range, Tags.
- Targeting: Melee prefers nearest; ranged prefers nearest within range; tie-breakers deterministic.

Economy & Shop:
- Gold Sources: Base income, interest, win/loss streaks, loot rounds, sells.
- Interest: +1 per 10g up to cap [TBD]; paid after combat.
- Streaks: Win/Loss streaks grant bonus gold [TBD] with soft cap.
- Shop: 5 slots per roll; reroll cost [TBD]; lock preserves shop.
- Leveling: Buy XP; level increases board slots and shop odds quality.

Items & Augments:
- Item Components: Drop from PvE/loot; 2 components combine into completed items.
- Completed Items: Provide stats + unique effect; limit [TBD] per unit.
- Distribution: Carousel guarantees access to components; pity rules for fairness.
- Augments (optional): Draft 1–3 times per match from rarity-weighted pool; build-defining effects.

Combat Rules:
- Prep Phase: Place units, equip items, choose augments.
- Start: All units gain initial mana [TBD]; generate mana on basic attacks and when hit.
- Abilities: Cast at full mana; spell effects defined per unit (damage, CC, heals, summons).
- Positioning: Collision, walk speed, and pathing simple and deterministic.
- Resolution: Fight ends on team wipe or timeout [TBD] → draw handling [TBD].
- Damage to Player: Based on surviving enemy unit stars + base round damage [TBD].

Progression & Fairness:
- Shop Odds by Level: Weighted table; monotonic increase for higher-cost units.
- Carousel Order: Reverse-standings priority; one pick per player; overtime tie-breaker deterministic.
- Pity/Bad-Luck Protection: Floors on components, gold, and copies by stage [TBD].

Content Schema (ScriptableObjects):
- UnitDef: id, name, cost, traits[], baseStats, spell, aiProfile, vfx/sfx refs.
- TraitDef: id, name, breakpoints[], effects per breakpoint, emblem rules.
- ItemComponentDef: id, name, stats, combineMap (componentA+componentB→ItemDef).
- ItemDef: id, name, stats, uniqueEffect, stacking rules.
- AugmentDef: id, rarity, constraints, effect, rollWeight, exclusivity tags.
- ShopTable: per-level odds for costs 1–5; stage-based modifiers.
- StageDef: sequence of rounds (PvE/PvP/Carousel), loot tables, difficulty.

Balance Knobs (Tuning):
- Economy: base income, interest cap, streak values, XP cost curve, reroll cost.
- Board Power: level-to-slot curve, HP start, damage formula, round timer.
- Combat: mana on hit/attack, AS caps, crit formula, armor/MR curves, range breakpoints.
- Items: stat budgets, unique effect power levels, stacking rules.
- Traits: breakpoint values, emblem access, vertical vs horizontal power.
- RNG: loot variance bounds, carousel weights, augment roll rules.

UX/Clarity:
- Shop readability: cost color coding, trait badges, duplicate highlighting, odds table by level.
- Board clarity: front/back indicators, aggro markers, cast bars, CC icons.
- Post-round: damage breakdown, economy summary, next-round preview.
- Controls: drag-drop for units/items; undo sell confirmation; lock/roll hotkeys.

Game Modes:
- Standard: Full match pacing with all systems.
- Labs (optional): Shorter pacing; fewer stages; quicker power spikes.
- Sandbox: Single-player test with deterministic seeds.

AI/Bots (Optional):
- Draft: Heuristic comp goals, econ thresholds, pivot rules.
- Combat: Positioning templates + counters; item heuristics.
- Performance: Budgeted decisions per prep phase only.

Telemetry (If enabled):
- Track: econ decisions (interest kept), roll counts, level timing, comp tags, itemization patterns.
- Combat: damage sources, cast timings, survival times, over-time DPS.
- Fairness: loot distribution, augment RNG, carousel picks.

Technical Notes:
- Determinism: Seed per match; shop/loot/targeting seeded; record/replay support.
- Data: SO-driven content, addressable assets; versioned balance patches.
- Performance: Burst where applicable for sim; object pooling for VFX.

Open Questions [TBD]:
- Exact board dimensions and bench size.
- Numeric curves for economy (interest cap, streak thresholds, XP costs).
- Player damage formula and round timer.
- Item cap per unit and duplicate unique rules.
- Augment cadence and rarity distribution.

Appendix: Initial Defaults (Proposed, placeholder)
- Players: 8; Start HP: 100; Board slots at Lvl 1–9: 2,3,4,5,6,7,8,8,9.
- Shop odds table and all numeric values intentionally left TBD for balancing passes.

