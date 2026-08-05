using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;

namespace IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest
{
    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        /*
         * The cookie container is owned here rather than left to CreateClient so tests can
         * inspect and rewrite individual cookies. Adding a "Cookie" header by hand instead
         * would collide with the handler's own, producing two headers and an ambiguous read
         * on the server.
         *
         * The base address carries the /api prefix that endpoints are grouped under, so
         * tests keep addressing "users/login". The trailing slash is required — without it
         * Uri resolution drops the last segment and every request 404s.
         */
        HttpClient = factory.CreateDefaultClient(
            new Uri("http://localhost/api/"),
            new CookieContainerHandler(Cookies));
    }

    protected HttpClient HttpClient { get; }

    protected CookieContainer Cookies { get; } = new();

    protected const string RefreshTokenCookieName = "ekasi.refreshToken";

    protected const string CsrfHeaderName = "X-CSRF-TOKEN";

    private static readonly Uri CookieOrigin = new("http://localhost");

    /// <summary>Login and refresh return only the access token; the refresh token is a cookie.</summary>
    protected sealed record AccessTokens(string AccessToken);

    private sealed record AntiforgeryTokenResponse(string Token);

    protected static string UniqueEmail() => $"test-{Guid.NewGuid():N}@example.com";

    /// <summary>
    /// The refresh token is httpOnly, so tests that need its value (rotation, revocation)
    /// read it off the cookie container.
    /// </summary>
    protected string? CurrentRefreshToken() =>
        Cookies.GetCookies(CookieOrigin)[RefreshTokenCookieName]?.Value;

    /// <summary>Puts a specific refresh token back on the client, to replay a stale one.</summary>
    protected void SetRefreshToken(string value) =>
        Cookies.Add(CookieOrigin, new Cookie(RefreshTokenCookieName, value, "/"));

    /// <summary>
    /// Fetches a CSRF token, which also plants its paired cookie in the container. Refresh
    /// and logout reject anything that cannot present both halves.
    /// </summary>
    protected async Task<string> GetCsrfTokenAsync()
    {
        HttpResponseMessage response = await HttpClient.GetAsync("antiforgery/token");
        response.EnsureSuccessStatusCode();

        AntiforgeryTokenResponse? token =
            await response.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>();

        return token!.Token;
    }

    /// <summary>POSTs with a valid CSRF token attached, the way the SPA does.</summary>
    protected async Task<HttpResponseMessage> PostWithCsrfAsync(string path)
    {
        string csrf = await GetCsrfTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add(CsrfHeaderName, csrf);

        return await HttpClient.SendAsync(request);
    }

    protected async Task<Guid> RegisterUserAsync(string email)
    {
        var request = new
        {
            email,
            firstName = "Test",
            lastName = "User",
            password = "Password123"
        };

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("users/register", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    protected Task<HttpResponseMessage> LoginResponseAsync(string email) =>
        HttpClient.PostAsJsonAsync("users/login", new { email, password = "Password123" });

    protected async Task<AccessTokens> LoginAsync(string email)
    {
        HttpResponseMessage response = await LoginResponseAsync(email);
        response.EnsureSuccessStatusCode();

        AccessTokens? tokens = await response.Content.ReadFromJsonAsync<AccessTokens>();

        return tokens!;
    }

    protected async Task<(Guid UserId, AccessTokens Tokens)> RegisterAndLoginAsync()
    {
        string email = UniqueEmail();
        Guid userId = await RegisterUserAsync(email);
        AccessTokens tokens = await LoginAsync(email);

        return (userId, tokens);
    }

    protected void Authenticate(string accessToken)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
