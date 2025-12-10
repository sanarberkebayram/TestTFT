# DECISIONS

Record noteworthy architectural choices and why.

- Date: 2025-12-10
  - Decision: Use IMGUI (OnGUI) for minimal HUD + Shop MVP instead of uGUI/UITK.
  - Context/Alternatives: uGUI/UITK with prefabs and scripts would be production-ready but slower to scaffold in this session and requires assets/prefabs. IMGUI allows single-script overlay to validate inputs and loop quickly.
  - Rationale: Meet Issue #14 acceptance quickly with no asset dependencies; keep code contained and easy to replace later.
  - Impact: Faster delivery; visual fidelity limited; later refactor to uGUI/UITK planned.
