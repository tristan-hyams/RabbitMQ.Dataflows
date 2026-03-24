---
name: agentify
description: Bootstrap a repo with ML context files (CLAUDE.md, copilot-instructions.md, AGENTS.md) as thin shims pointing to docs/RULES.md and docs/STRUCTURE.md, then review for accuracy.
user-invocable: true
---

# Agentify a Repository

Bootstrap the three ML context files as thin shims and ensure the central docs exist.

## Step 1 — Discover repo layout

- Determine the repo root (look for `.git/`)
- Check for existing files: `.claude/CLAUDE.md`, `.github/copilot-instructions.md`, `AGENTS.md` (repo root or any subfolder)
- Check for existing `docs/RULES.md` and `docs/STRUCTURE.md`
- Read any existing versions of all five files to understand current state
- Identify the primary language, build system, and package structure

### Framework detection

Detect the primary language and framework to seed RULES.md with language-appropriate sections:

- **Go**: look for `go.mod`, `.golangci.yml`, `Makefile`
- **Python**: look for `pyproject.toml`, `setup.py`, `requirements.txt`, `.flake8`, `ruff.toml`
- **TypeScript/JavaScript**: look for `package.json`, `tsconfig.json`, `.eslintrc.*`, `biome.json`
- **Rust**: look for `Cargo.toml`, `clippy.toml`
- **C# / .NET**: look for `*.sln`, `*.csproj`, `Directory.Build.props`, `.editorconfig`, `global.json`
- **Java/Kotlin**: look for `build.gradle`, `pom.xml`

Use detected language to inform idiomatic patterns, naming conventions, and toolchain sections in RULES.md.

### Existing docs discovery

Scan for existing documentation and config that agents should know about without reading in full:

- `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`
- `Makefile`, `Taskfile.yml`, `justfile`
- `docker-compose.yml`, `Dockerfile`
- Linter configs: `.golangci.yml`, `.eslintrc.*`, `ruff.toml`, `biome.json`, `.prettierrc`
- CI configs: `.github/workflows/`, `.gitlab-ci.yml`, `Jenkinsfile`
- `.env.example`, `.env.test`
- `openapi.yaml`, `swagger.json`, `proto/` directories

Reference discovered files from RULES.md so agents know they exist without loading them all.

## Step 2 — Bootstrap docs/RULES.md (if it doesn't exist)

Create `docs/RULES.md` by analyzing the codebase. Include:

- Style & formatting rules (linters, formatters, line length)
- Naming conventions observed in the code
- Idiomatic patterns for the primary language (error handling, dependency injection, etc.)
- Testing conventions (test runner, integration test patterns, env/config approach)
- Security conventions
- Observability conventions (logging, tracing, metrics)
- Common commands (build, test, lint, deploy scripts)
- Release workflow
- References to discovered config files (linters, CI, Docker, API specs) — don't duplicate their content, just note they exist and their purpose

If it already exists, note it for the review step.

## Step 3 — Bootstrap docs/STRUCTURE.md (if it doesn't exist)

Create `docs/STRUCTURE.md` by analyzing the codebase. Include:

- Project overview (what is this repo, what does it do)
- Package/module table with purpose descriptions
- Dependency architecture (what depends on what, ASCII tree if applicable)
- Leaf vs hub packages/modules
- Integration points table (need → package mapping)

If it already exists, note it for the review step.

## Step 4 — Evaluate graduated pointers

Beyond the two core docs, check if the repo warrants additional focused docs. Create them if there is enough substance, otherwise fold the content into RULES.md:

| Doc | Create when... |
|-----|---------------|
| `docs/TESTING.md` | Multiple test patterns, integration tests, fixtures, or notable env/config conventions |
| `docs/DEPLOYMENT.md` | CI/CD pipelines, release scripts, or multi-environment deploy config |
| `docs/API.md` | OpenAPI specs, proto files, or significant API surface |

Only create additional docs if they would meaningfully reduce RULES.md size. Don't split for the sake of splitting.

## Step 5 — Create the three thin shim files

Create these files with identical structure — each is a selective context loader pointing to the docs that exist:

### .claude/CLAUDE.md
```markdown
# CLAUDE.md
<!-- last reviewed: {YYYY-MM-DD} -->

All coding rules, conventions, commands, and standards:
**[docs/RULES.md](../docs/RULES.md)**

Package architecture, dependencies, and integration points:
**[docs/STRUCTURE.md](../docs/STRUCTURE.md)**
```

### .github/copilot-instructions.md
```markdown
# Copilot Instructions
<!-- last reviewed: {YYYY-MM-DD} -->

All coding rules, conventions, commands, and standards:
**[docs/RULES.md](../docs/RULES.md)**

Package architecture, dependencies, and integration points:
**[docs/STRUCTURE.md](../docs/STRUCTURE.md)**
```

### AGENTS.md (at repo root)
```markdown
# AGENTS.md
<!-- last reviewed: {YYYY-MM-DD} -->

All coding rules, conventions, commands, and standards:
**[docs/RULES.md](../docs/RULES.md)**

Package architecture, dependencies, and integration points:
**[docs/STRUCTURE.md](../docs/STRUCTURE.md)**
```

Replace `{YYYY-MM-DD}` with today's date.

If graduated pointer docs were created (TESTING.md, DEPLOYMENT.md, API.md), add them to all three shims:
```markdown
Testing patterns and configuration:
**[docs/TESTING.md](../docs/TESTING.md)**
```

Adjust relative paths based on actual file locations. If the repo uses a workspace subfolder (e.g. `v1/`), place AGENTS.md there and fix paths accordingly.

### Path validation

After writing all shim files, verify that every relative path in each shim resolves to an actual file. Report any broken links.

## Step 6 — Review for accuracy

Compare RULES.md and STRUCTURE.md against the actual codebase:

- Are all packages/modules listed?
- Is the dependency tree accurate?
- Are the coding conventions actually followed in the code?
- Are the commands correct and runnable?
- Are there undocumented patterns that should be captured?
- Are discovered config files (linters, CI, Docker) referenced?
- Do all relative paths in shim files resolve correctly?
- Flag any discrepancies or recommendations to the user

### Staleness check

If docs already existed, check the `<!-- last reviewed: -->` comment. If older than 90 days or missing, flag it and update the date after review.

Present a summary of what was created, what was found, and any recommendations.
