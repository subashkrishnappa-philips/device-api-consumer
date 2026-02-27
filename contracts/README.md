
# contracts/

This directory holds generated contract artefacts. It is **gitignored** for dynamic files.

## Contents

| File | Created by | Committed? |
|---|---|---|
| `DeviceApi-Consumer-DeviceApi.json` | Consumer test run (`dotnet test`) | No — generated at runtime |

## Usage

- The consumer CI publishes `*.json` files under the GitHub Actions artifact **`pact-contracts`**.
- The provider CI downloads this artifact into this directory before running provider verification.
- You should **never edit pact JSON files manually** — they are the output of the consumer test suite.

## In CI (GitHub Actions)

```
Consumer CI (device-api-consumer)
  └─► runs consumer tests
  └─► uploads contracts/*.json  →  artifact: pact-contracts
  └─► dispatches pact-contracts-updated to device-api

Provider CI (device-api)
  └─► downloads artifact: pact-contracts  →  contracts/
  └─► runs DeviceApiProviderTests (replays pact file)
  └─► runs SwaggerContractValidatorTests
  └─► runs SwaggerCoverageTests
```
