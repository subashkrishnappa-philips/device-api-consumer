using PactNet;
using DeviceApi.Consumer.Tests.Config;

namespace DeviceApi.Consumer.Tests.Fixtures;

/// <summary>
/// Factory that creates a fresh <see cref="IPactBuilderV4"/> for every test.
///
/// Why a factory instead of a shared builder?
///   PactNet V4 accumulates interactions on a single builder. When
///   <c>VerifyAsync</c> is called it starts a mock server containing ALL
///   accumulated interactions and expects every one to be called in the lambda.
///   Sharing one builder across tests therefore requires each test to replay
///   every previous interaction — which breaks test independence.
///
///   Using a fresh builder per test means each <c>VerifyAsync</c> satisfies
///   exactly one interaction. PactNet merges the output into the same pact
///   JSON file automatically when the consumer/provider names match.
/// </summary>
public sealed class ConsumerPactFixture
{
    /// <summary>
    /// Creates a new PactNet V4 builder pre-configured with the correct
    /// consumer / provider names and output directory.
    /// Call once per test method and dispose after <c>VerifyAsync</c>.
    /// </summary>
    public IPactBuilderV4 CreateBuilder()
    {
        var config = new PactConfig
        {
            PactDir  = PactConstants.PactDir,
            LogLevel = PactLogLevel.Information
        };

        // Ensure the output directory exists before PactNet tries to write.
        Directory.CreateDirectory(PactConstants.PactDir);

        return Pact
            .V4(PactConstants.ConsumerName, PactConstants.ProviderName, config)
            .WithHttpInteractions();
    }
}
