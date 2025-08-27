# Repository Guidelines

## Project Structure & Modules
- `src/Randomizer`: Console app entrypoint (`Program.cs`), commands (`ConsoleCommands/`), core logic (`Graph/`), ROM I/O (`RomModifications/`).
- `Games/*`: Game-specific logic and data loaders (e.g., `Games/Alttp`).
- `tests/RandomizerTests`: MSTest unit tests.
- `asm/`, `data/`: Assembler scripts and runtime data. Do not commit ROMs.

## Build, Test, Run
- Build: `dotnet build ALttPR.sln -c Release`
- Tests: `dotnet test tests/RandomizerTests -c Release --collect:"XPlat Code Coverage"`
- Run randomizer: `dotnet run --project src/Randomizer -- randomize --settings data/settings.json --outdir out`
- Assemble base ROM: `dotnet run --project src/Randomizer -- assemblebaserom --rom /path/ALttP_JP1.0.sfc`
  - Output base ROM: `data/randomizer.sfc`

## Coding Style & Naming
- Language: C# (net9.0), nullable enabled, implicit usings on.
- Formatting: 4-space indentation, keep lines readable; run `dotnet format` before pushing.
- Naming: `PascalCase` for types/methods, `camelCase` for locals/params, private fields start with `_` (e.g., `_logger`).
- Keep modules cohesive: shared graph/algorithms in `Graph/`, game specifics in `Games/<GameName>/`.

## Testing Guidelines
- Framework: MSTest (`[TestClass]`, `[TestMethod]`).
- Location: mirror source folders under `tests/RandomizerTests` (e.g., `Logic/.../FooTest.cs`).
- Add tests for new logic and bug fixes; prefer deterministic seeds where applicable.
- Aim to keep or improve coverage; verify with `dotnet test` (coverage collected via `coverlet.collector`).

## Commits & Pull Requests
- Commits: concise, present-tense summary (e.g., “add entrance shuffle validation”); group related changes.
- PRs: include a clear description, rationale, and links to issues; list noteworthy flags/commands and data changes; update docs where relevant.
- Requirements: all tests pass, `dotnet format` applied, no generated ROMs or large binaries committed.

## Security & Configuration
- Do not commit ROM images or secrets. Use `assemblebaserom` with a legally obtained JP 1.0 ROM to create `data/randomizer.sfc`.
- Config paths are resolved via `Config` (e.g., `data/settings.json`). Prefer options `--settings`, `--outdir` for reproducibility.

