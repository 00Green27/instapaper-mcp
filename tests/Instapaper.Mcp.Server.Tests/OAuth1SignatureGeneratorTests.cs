namespace Instapaper.Mcp.Server.Tests;

public class OAuth1SignatureGeneratorTests
{
    private sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public TestTimeProvider(DateTimeOffset now) => _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
    }

    [Fact]
    public void CreateAuthorizationHeader_IncludesExpectedOauthParameters()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000); // fixed time
        var generator = new OAuth1SignatureGenerator(new TestTimeProvider(now));

        var uri = new Uri("https://example.com/resource");
        var parameters = new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" };

        var header = generator.CreateAuthorizationHeader(
            HttpMethod.Get,
            uri,
            consumerKey: "consumer",
            consumerSecret: "secret",
            token: "token",
            tokenSecret: "tokensecret",
            parameters: parameters);

        Assert.StartsWith("OAuth ", header);
        Assert.Contains("oauth_consumer_key=\"consumer\"", header);
        Assert.Contains("oauth_token=\"token\"", header);
        Assert.Contains("oauth_signature=", header);

        var expectedTimestamp = now.ToUnixTimeSeconds().ToString();
        Assert.Contains($"oauth_timestamp=\"{expectedTimestamp}\"", header);
    }

    [Fact]
    public void CreateAuthorizationHeader_EmptyParameters_StillProducesHeader()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1_700_000_100);
        var generator = new OAuth1SignatureGenerator(new TestTimeProvider(now));

        var header = generator.CreateAuthorizationHeader(
            HttpMethod.Post,
            new Uri("https://api.instapaper.com/1/bookmarks/list"),
            consumerKey: "ck",
            consumerSecret: "cs",
            token: null,
            tokenSecret: null,
            parameters: null);

        Assert.StartsWith("OAuth ", header);
        Assert.Contains("oauth_consumer_key=\"ck\"", header);
        Assert.Contains("oauth_signature=", header);
    }
}