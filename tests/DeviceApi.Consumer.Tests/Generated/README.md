# Generated Consumer Test Stubs

This folder is populated automatically by `tools/SwaggerPactGenerator` whenever
a new or uncovered endpoint is detected in the DeviceApi swagger.

## Contents

Files named `*.Generated.cs` are skeleton consumer test classes.

**Do NOT hand-edit files here** — they will be overwritten by the generator on
the next swagger change detection.

## Consumer team workflow

1. Open the generated `.Generated.cs` file.
2. Search for `TODO` comments — fill in:
   - Request body fields
   - Expected response body assertions (use `Match.Type` for resilience)
3. **Move** the completed file to `../Contracts/` (rename, drop `.Generated` suffix).
4. Re-run `run-pact-tests.ps1` to verify the provider satisfies your new contract.
