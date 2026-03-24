# Project Structure

## Overview

**RabbitMQ.Dataflows** is a .NET library suite for building high-performance, durable RabbitMQ-powered workflows. It provides connection/channel pooling, publish/consume abstractions, TPL Dataflow-based processing pipelines, and pluggable serialization, compression, and encryption — all with built-in OpenTelemetry tracing.

Published as 8 independent NuGet packages under the `HouseofCat.*` namespace.

## Package Table

| Package | Purpose | Leaf? |
|---------|---------|-------|
| `HouseofCat.Utilities` | Foundational utilities: logging, OpenTelemetry helpers, RecyclableMemoryStream, Guard, extensions, random data | Yes (leaf) |
| `HouseofCat.Hashing` | Argon2 hashing via `IHashingProvider` | Yes (leaf + Utilities) |
| `HouseofCat.Serialization` | Pluggable serialization: System.Text.Json, Newtonsoft.Json, MessagePack via `ISerializationProvider` | Yes (leaf + Utilities) |
| `HouseofCat.Compression` | Pluggable compression: Gzip, Brotli, Deflate, LZ4 via `ICompressionProvider` / `ICodecProvider` | Yes (leaf + Utilities) |
| `HouseofCat.Encryption` | Pluggable encryption: AES-GCM (BCL + BouncyCastle), AES-CBC streams via `IEncryptionProvider` | Yes (leaf + Utilities) |
| `HouseofCat.Dataflows` | Generic TPL Dataflow engine: `Pipeline`, `DataflowEngine`, `ChannelBlock`, `IWorkState`, `BaseDataflow` | Hub |
| `HouseofCat.RabbitMQ` | RabbitMQ integration: connection/channel pools, Publisher, Consumer, ConsumerDataflow, RabbitService, Topologer | Hub (apex) |
| `HouseofCat.Data` | Database abstractions: Dapper helpers, multi-dialect query building (SqlKata), `DataTransformer` pipeline | Hub |

## Dependency Architecture

```
HouseofCat.Utilities  (no project deps)
  |
  +-- HouseofCat.Hashing
  +-- HouseofCat.Serialization
  +-- HouseofCat.Compression
  +-- HouseofCat.Encryption
  |
  +-- HouseofCat.Dataflows
  |     deps: Compression, Encryption, Serialization, Utilities
  |
  +-- HouseofCat.RabbitMQ  (apex)
  |     deps: Compression, Dataflows, Encryption, Hashing, Serialization, Utilities
  |
  +-- HouseofCat.Data
        deps: Compression, Encryption, Hashing, Serialization, Utilities
```

**Leaf packages** (Hashing, Serialization, Compression, Encryption) depend only on Utilities and external NuGet packages. They can be used independently.

**Hub packages** (Dataflows, RabbitMQ, Data) compose multiple leaf packages. `HouseofCat.RabbitMQ` is the primary apex library that most consumers will reference.

## Directory Layout

```
RabbitMQ.Dataflows/
  Directory.Build.props         # Shared MSBuild properties (TFM, NuGet metadata, SourceLink)
  Directory.Packages.props      # Centralized NuGet package versions
  version.props                 # Default version (5.0.0), overridden by CI tag
  Test.Build.props              # Auto-imported test infra for *.Test(s|ing) projects
  RabbitMQ.Dataflows.slnx       # Solution file (SLNX format)
  src/
    HouseofCat.Utilities/       # Extensions, helpers, logging, OpenTelemetry, RecyclableMemoryStream
    HouseofCat.Hashing/         # IHashingProvider, ArgonHashingProvider
    HouseofCat.Serialization/   # ISerializationProvider, JsonProvider, MessagePackProvider
    HouseofCat.Compression/     # ICompressionProvider, Gzip/Brotli/Deflate/LZ4 + Recyclable variants
    HouseofCat.Encryption/      # IEncryptionProvider, AesGcm/BouncyAesGcm + Recyclable variants
    HouseofCat.Dataflows/       # IWorkState, BaseDataflow, Pipeline, DataflowEngine, ChannelBlock
    HouseofCat.RabbitMQ/        # Pools, Publisher, Consumer, ConsumerDataflow, RabbitService, Topologer
    HouseofCat.Data/            # DataTransformer, DbConnectionFactory, DapperHelper, QueryBuilding
    Apps/
      Aspire.AppHost/                       # .NET Aspire orchestrator (RabbitMQ container + management UI)
      RabbitMQ.Dataflows.ServiceDefaults/   # Aspire service defaults (OTel, health checks, resilience)
      RabbitMQ.Console.Tests/               # RabbitMQ integration tests (require live broker)
      RabbitMQ.ConsumerDataflowService/     # ConsumerDataflow integration test
      OpenTelemetry.Console.Tests/          # OpenTelemetry span propagation tests
    Tests/
      UnitTests/                # xUnit unit tests (compression, encryption, hashing, serialization, transforms)
  guides/
    rabbitmq/                   # Step-by-step guides: ConnectionPools, ChannelPools, Publisher, Consumer, etc.
    csharp/                     # C# language guides (compression, parallelism, etc.)
    golang/                     # Go dependency patching guide
    ml/                         # ML/AI guides (Stable Diffusion)
  docs/                         # Agent context docs (this file)
```

## Integration Points

| Need | Package | Key Types |
|------|---------|-----------|
| RabbitMQ pub/sub | `HouseofCat.RabbitMQ` | `RabbitService`, `Publisher`, `Consumer` |
| Durable connection pooling | `HouseofCat.RabbitMQ` | `ConnectionPool`, `ChannelPool` |
| Workflow processing | `HouseofCat.RabbitMQ` | `ConsumerDataflow<TState>`, `ConsumerDataflowService<TState>` |
| Simple pipelines | `HouseofCat.Dataflows` | `Pipeline<TIn,TOut>`, `DataflowEngine<TIn,TOut>` |
| Topology management | `HouseofCat.RabbitMQ` | `Topologer`, `TopologyConfig` |
| Serialization | `HouseofCat.Serialization` | `JsonProvider`, `NewtonsoftJsonProvider`, `MessagePackProvider` |
| Compression | `HouseofCat.Compression` | `GzipProvider`, `BrotliProvider`, `DeflateProvider`, `LZ4StreamProvider` |
| Encryption | `HouseofCat.Encryption` | `AesGcmEncryptionProvider`, `BouncyAesGcmEncryptionProvider` |
| Hashing | `HouseofCat.Hashing` | `ArgonHashingProvider` |
| Database access | `HouseofCat.Data` | `DbConnectionFactory`, `DapperHelper`, query building services |
| Data transformation | `HouseofCat.Data` | `DataTransformer`, `RecyclableTransformer` |
| OpenTelemetry | `HouseofCat.Utilities` | `OpenTelemetryHelpers` |
| Logging | `HouseofCat.Utilities` | `LogHelpers` |
| Local dev environment | `Aspire.AppHost` | RabbitMQ container with management UI |
