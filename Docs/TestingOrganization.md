# Testing Organization — Phase 1

## Folders

- `Assets/Tests/EditMode/` — Editor / unit-style tests (later)
- `Assets/Tests/PlayMode/` — Play Mode integration tests (later)
- `Assets/Scripts/Tests/` — Optional small runtime helper scripts

## Rules

- Do not put gameplay production code in `Assets/Tests`.
- Prefer Unity Test Framework tests once package is installed.
- Phase 1 does not require written gameplay tests.

## Manual smoke test (Phase 1)

1. Open `Assets/Scenes/Bootstrap.unity`
2. Press Play
3. Console should show `[BounderTrail][Bootstrap]` info logs
