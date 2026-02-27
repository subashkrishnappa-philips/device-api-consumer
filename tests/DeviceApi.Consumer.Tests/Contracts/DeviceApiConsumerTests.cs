using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PactNet;
using PactNet.Matchers;
using DeviceApi.Consumer.Tests.Fixtures;
using Xunit;

namespace DeviceApi.Consumer.Tests.Contracts;

/// <summary>
/// Hand-authored consumer-side Pact contract tests for the DeviceApi provider.
///
/// IMPORTANT — Consumer team responsibilities:
/// ─────────────────────────────────────────────────────────────────────────────
///  • Each method here defines ONE interaction (request + expected response).
///  • When the DeviceApi swagger changes, SwaggerContractValidatorTests on the
///    provider side FAILS and outputs a notification listing which consumer
///    teams must act (see contracts/consumers.json for the registry).
///  • A generated skeleton test will appear in the Generated/ folder for every
///    new endpoint — review it, fill in your assertions, and move it here.
///  • Run <c>run-pact-tests.ps1</c> to regenerate the pact file and verify it
///    against the provider.
///
/// PactNet V4 pattern:
///   1. _fixture.CreateBuilder() — new builder per test (test independence).
///   2. pact.UponReceiving(...)...WillRespond()  — register the interaction.
///   3. await pact.VerifyAsync(async ctx => { ... }) — run against mock server.
/// </summary>
public sealed class DeviceApiConsumerTests : IClassFixture<ConsumerPactFixture>
{
    private readonly ConsumerPactFixture _fixture;
    private const string Endpoint = "/api/UpdateDeviceInformation";

    public DeviceApiConsumerTests(ConsumerPactFixture fixture)
    {
        _fixture = fixture;
    }

    // ── Happy-path ────────────────────────────────────────────────────────────

    /// <summary>
    /// Contract: a fully populated valid payload returns 200 OK with the
    /// expected response structure. Field types are matched so the contract
    /// is resilient to value changes (e.g. different timestamps).
    /// </summary>
    [Fact]
    public async Task UpdateDeviceInformation_ValidPayload_Returns200WithExpectedBody()
    {
        var pact = _fixture.CreateBuilder();

        pact
            .UponReceiving("a valid POST request to update device information")
            .WithRequest(HttpMethod.Post, Endpoint)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                serialNumber = "SN-20240001-XYZ",
                username     = "john.doe"
            })
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", Match.Regex(
                "application/json; charset=utf-8",
                "application/json; charset=utf-8"))
            .WithJsonBody(new
            {
                success      = Match.Type(true),
                message      = Match.Type("Device information updated successfully."),
                serialNumber = Match.Type("SN-20240001-XYZ"),
                username     = Match.Type("john.doe"),
                updatedAt    = Match.Type("2026-02-27T10:30:00Z")
            });

        await pact.VerifyAsync(async ctx =>
        {
            using var client = new HttpClient { BaseAddress = ctx.MockServerUri };

            var payload  = new { SerialNumber = "SN-20240001-XYZ", Username = "john.doe" };
            var response = await client.PostAsJsonAsync(Endpoint, payload);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            body.Should().ContainKey("success");
            body.Should().ContainKey("serialNumber");
            body.Should().ContainKey("username");
            body.Should().ContainKey("updatedAt");
        });
    }

    // ── Alternative serial number format ──────────────────────────────────────

    /// <summary>
    /// Contract: a different valid serial number format also returns 200.
    /// Ensures the contract is not pinned to a single device ID value.
    /// </summary>
    [Fact]
    public async Task UpdateDeviceInformation_AlternativeSerialNumber_Returns200()
    {
        var pact = _fixture.CreateBuilder();

        pact
            .UponReceiving("a valid POST request with an alternative serial number format")
            .WithRequest(HttpMethod.Post, Endpoint)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                serialNumber = "DEV-9999-ABCD",
                username     = "jane.smith"
            })
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", Match.Regex(
                "application/json; charset=utf-8",
                "application/json; charset=utf-8"))
            .WithJsonBody(new
            {
                success      = Match.Type(true),
                message      = Match.Type("Device information updated successfully."),
                serialNumber = Match.Type("DEV-9999-ABCD"),
                username     = Match.Type("jane.smith"),
                updatedAt    = Match.Type("2026-02-27T10:30:00Z")
            });

        await pact.VerifyAsync(async ctx =>
        {
            using var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            var payload  = new { SerialNumber = "DEV-9999-ABCD", Username = "jane.smith" };
            var response = await client.PostAsJsonAsync(Endpoint, payload);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        });
    }

    // ── Validation-failure path ───────────────────────────────────────────────

    /// <summary>
    /// Contract: an empty SerialNumber must produce 400 Bad Request.
    /// Pins the validation behaviour so it cannot be silently removed.
    /// </summary>
    [Fact]
    public async Task UpdateDeviceInformation_MissingSerialNumber_Returns400()
    {
        var pact = _fixture.CreateBuilder();

        pact
            .UponReceiving("a POST request with a missing SerialNumber field")
            .WithRequest(HttpMethod.Post, Endpoint)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                serialNumber = "",
                username     = "john.doe"
            })
            .WillRespond()
            .WithStatus(HttpStatusCode.BadRequest)
            .WithHeader("Content-Type", Match.Regex(
                "application/json; charset=utf-8",
                "application/json(;\\s*charset=utf-8)?"));

        await pact.VerifyAsync(async ctx =>
        {
            using var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            var payload  = new { SerialNumber = "", Username = "john.doe" };
            var response = await client.PostAsJsonAsync(Endpoint, payload);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }

    /// <summary>
    /// Contract: an empty Username must produce 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task UpdateDeviceInformation_MissingUsername_Returns400()
    {
        var pact = _fixture.CreateBuilder();

        pact
            .UponReceiving("a POST request with a missing Username field")
            .WithRequest(HttpMethod.Post, Endpoint)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                serialNumber = "SN-10000-ZZZ",
                username     = ""
            })
            .WillRespond()
            .WithStatus(HttpStatusCode.BadRequest)
            .WithHeader("Content-Type", Match.Regex(
                "application/json; charset=utf-8",
                "application/json(;\\s*charset=utf-8)?"));

        await pact.VerifyAsync(async ctx =>
        {
            using var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            var payload  = new { SerialNumber = "SN-10000-ZZZ", Username = "" };
            var response = await client.PostAsJsonAsync(Endpoint, payload);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }
}
