# Rules & Conventions

## Language & Framework

- **Language:** C# (.NET 10.0)
- **Lang version:** `latest` (file-scoped namespaces, pattern matching, etc.)
- **Solution:** `RabbitMQ.Dataflows.slnx` (SLNX format)
- **Build props:** `Directory.Build.props` (target framework, NuGet metadata, SourceLink, analyzers)
- **Package versions:** `Directory.Packages.props` (central package management)
- **Version:** `version.props` (default `5.0.0`, overridden by CI via `/p:Version`)
- **Test infrastructure:** `Test.Build.props` (auto-imported for `*.Test`, `*.Tests`, `*.Testing` projects)

## Build & Run

```bash
# Restore
dotnet restore RabbitMQ.Dataflows.slnx

# Build
dotnet build RabbitMQ.Dataflows.slnx --configuration Release

# Unit tests (xUnit + Coverlet)
dotnet test src/Tests/UnitTests/UnitTests.csproj --configuration Release

# Integration tests require a running RabbitMQ broker.
# Use Aspire AppHost for local dev:
dotnet run --project src/Apps/Aspire.AppHost/Aspire.AppHost.csproj
# Or run console test projects manually:
#   src/Apps/RabbitMQ.Console.Tests
#   src/Apps/RabbitMQ.ConsumerDataflowService
#   src/Apps/OpenTelemetry.Console.Tests
```

## Style & Formatting

Enforced via `.editorconfig`:

- **Indent:** 4 spaces (tab width 4)
- **Line endings:** CRLF
- **Namespaces:** File-scoped (`csharp_style_namespace_declarations = file_scoped:warning`)
- **Braces:** Prefer braces (`csharp_prefer_braces = true`)
- **`var` usage:** Explicit type preference is disabled (`IDE0008` severity none) — both `var` and explicit types are acceptable
- **`new(...)` shorthand:** Disabled (`csharp_style_implicit_object_creation_when_type_is_apparent = false`) — use `new ClassName()` explicitly
- **Primary constructors:** Disabled (`csharp_style_prefer_primary_constructors = false`)
- **Collection expressions:** Disabled (`dotnet_style_prefer_collection_expression = never`)
- **Collection initializers:** Disabled (`dotnet_style_collection_initializer = false`)

## Naming Conventions

From `.editorconfig`:

- **Types** (class, struct, interface, enum): PascalCase
- **Interfaces:** Prefix with `I` (e.g., `IConnectionPool`, `ISerializationProvider`)
- **Properties, events, methods:** PascalCase
- **Private fields:** No enforced prefix — the codebase uses both `_field` and `field` patterns
- **Constants:** PascalCase

Project naming pattern: `HouseofCat.{Domain}` (e.g., `HouseofCat.RabbitMQ`, `HouseofCat.Compression`).

## Idiomatic Patterns

### Provider Pattern
Core functionality is abstracted behind provider interfaces:
- `ISerializationProvider` — serialize/deserialize
- `ICompressionProvider` — compress/decompress
- `IEncryptionProvider` — encrypt/decrypt
- `IHashingProvider` — hash key generation

Each interface has multiple implementations (e.g., `GzipProvider`, `BrotliProvider`, `LZ4StreamProvider` for compression).

### IWorkState Contract
`IWorkState` is the fundamental data contract for dataflow steps. It carries:
- `Data` dictionary for step-to-step communication
- `IsFaulted` / `EDI` for error propagation
- `WorkflowSpan` for OpenTelemetry distributed tracing

`IRabbitWorkState` extends this with RabbitMQ-specific fields (`ReceivedMessage`, `SendMessage`).

### Channel-Based Pools
Connection and channel pools use `System.Threading.Channels.Channel<T>` as bounded, thread-safe queues — not `ConcurrentQueue` or `BlockingCollection`.

### RecyclableMemoryStream
Performance-critical compression and encryption providers have `Recyclable*` variants that use `Microsoft.IO.RecyclableMemoryStream` to reduce GC pressure.

### Fluent Dataflow Builder
`ConsumerDataflow<TState>` uses a fluent builder pattern: `WithBuildState()`, `WithDecryptionStep()`, `AddStep()`, `WithFinalization()`, etc.

### Performance Conventions
- `MethodImpl(MethodImplOptions.AggressiveInlining)` for performance-critical methods
- `ConfigureAwait(false)` on all async calls
- `ValueTask` return types for memory efficiency in hot paths
- `ReadOnlySpan<byte>` for zero-allocation byte manipulation

## Error Handling

- Dataflows (v2) catch exceptions in steps and set `IWorkState.IsFaulted` + `IWorkState.EDI` rather than letting them propagate.
- Pipelines (v1) do **not** catch exceptions — thrown exceptions kill the pipeline.
- Guard class (`HouseofCat.Utilities.Errors.Guard`) for argument validation.

## Observability

- **OpenTelemetry:** Native distributed tracing for publish/consume via `OpenTelemetryHelpers` in `HouseofCat.Utilities`.
- Trace context propagated through RabbitMQ message headers.
- `BaseDataflow<TState>` instruments every step with OpenTelemetry spans.
- Logging via `ILogger<T>` through `LogHelpers` static singleton.

## Testing

- **Unit tests:** xUnit (`src/Tests/UnitTests/`), run in CI. Coverage via Coverlet (Cobertura format).
- **Integration tests:** Console apps in `src/Apps/` that require a live RabbitMQ broker. Not run in CI.
- **Local dev:** .NET Aspire AppHost provides RabbitMQ container with management UI.
- **Code coverage:** Reported to Codacy via `codacy-coverage-reporter-action`.

## Security

- `AllowUnsafeBlocks` enabled only in `HouseofCat.Utilities`.
- Encryption supports AES-GCM (128/192/256) via BCL and BouncyCastle.
- Hashing via Argon2 (`Konscious.Security.Cryptography`).
- SSL/TLS and OAuth2 support in RabbitMQ connection options.

## CI/CD

- **Build workflow:** `.github/workflows/build.yml` — triggers on push/PR to `main`, builds on `windows-latest`, .NET 10.x, runs unit tests, uploads coverage to Codacy. Excludes `src/Apps/**` projects.
- **Publish workflow:** `.github/workflows/publish.yml` — triggers on version tags (`v*.*.*`) or manual `workflow_dispatch`. Extracts version from tag (falls back to manual input). Builds, packs, and pushes all 8 library packages to NuGet.org. Creates a GitHub Release with generated notes.
- **Code quality:** Codacy (`.codacy.yml` excludes `guides/`, `tests/`, `**.md`).

## Release Process

1. Tag the commit: `git tag v5.1.0 && git push origin v5.1.0`
2. Publish workflow triggers automatically, extracts version from tag
3. Each `src/HouseofCat.*` project is packed and pushed as an independent NuGet package
4. GitHub Release created with auto-generated notes
5. **Manual fallback:** Trigger `workflow_dispatch` from Actions tab with version input

## Key Config Files

| File | Purpose |
|------|---------|
| `.editorconfig` | C# code style and naming rules |
| `Directory.Build.props` | Shared MSBuild properties (target framework, NuGet metadata, SourceLink) |
| `Directory.Packages.props` | Centralized NuGet package versions |
| `version.props` | Default version (`5.0.0`), overridden by CI tag |
| `Test.Build.props` | Auto-imported test infrastructure for `*.Test(s\|ing)` projects |
| `.codacy.yml` | Codacy analysis exclusions |
| `.github/workflows/build.yml` | CI build + test + coverage |
| `.github/workflows/publish.yml` | NuGet publish pipeline (tag-triggered) |
