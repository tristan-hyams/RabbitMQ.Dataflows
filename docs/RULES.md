# Rules & Conventions

## Language & Framework

- **Language:** C# (.NET 8.0)
- **Lang version:** `latest` (file-scoped namespaces, pattern matching, etc.)
- **Solution:** `RabbitMQ.Dataflows.sln`
- **Shared build props:** `common.props` (NuGet metadata, SourceLink, analyzers) + `version.props` (version `4.2.0`)

## Build & Run

```bash
# Restore
dotnet restore RabbitMQ.Dataflows.sln

# Build
dotnet build RabbitMQ.Dataflows.sln --configuration Release

# Unit tests (xUnit + Coverlet)
dotnet test ./tests/UnitTests/UnitTests.csproj --configuration Release

# Integration tests require a running RabbitMQ broker
# Run console test projects manually:
#   tests/RabbitMQ.Console.Tests
#   tests/RabbitMQ.ConsumerDataflowService
#   tests/OpenTelemetry.Console.Tests
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

- **Unit tests:** xUnit (`tests/UnitTests/`), run in CI. Coverage via Coverlet (Cobertura format).
- **Integration tests:** Console apps in `tests/` that require a live RabbitMQ broker. Not run in CI.
- **Code coverage:** Reported to Codacy via `codacy-coverage-reporter-action`.

## Security

- `AllowUnsafeBlocks` enabled only in `HouseofCat.Utilities`.
- Encryption supports AES-GCM (128/192/256) via BCL and BouncyCastle.
- Hashing via Argon2 (`Konscious.Security.Cryptography`).
- SSL/TLS and OAuth2 support in RabbitMQ connection options.

## CI/CD

- **Build workflow:** `.github/workflows/build.yml` — triggers on push/PR to `main`, builds on `windows-latest`, .NET 8.x, runs unit tests, uploads coverage to Codacy.
- **Publish workflow:** `.github/workflows/publish.yml` — manual dispatch or push to `publish` branch. Publishes all 8 NuGet packages via `alirezanet/publish-nuget@v3.1.0`. Version from `version.props`.
- **Code quality:** Codacy (`.codacy.yml` excludes `guides/`, `tests/`, `**.md`).

## Release Process

1. Update version in `version.props`
2. Push to `publish` branch or trigger publish workflow manually
3. Each `src/` project is published as an independent NuGet package
4. First package (`HouseofCat.Compression`) gets a git tag; others do not

## Key Config Files

| File | Purpose |
|------|---------|
| `.editorconfig` | C# code style and naming rules |
| `common.props` | Shared MSBuild properties (target framework, NuGet metadata, SourceLink) |
| `version.props` | Centralized version (`4.2.0`) |
| `.codacy.yml` | Codacy analysis exclusions |
| `.github/workflows/build.yml` | CI build + test + coverage |
| `.github/workflows/publish.yml` | NuGet publish pipeline |
| `.gitignore` | Standard .NET gitignore |
