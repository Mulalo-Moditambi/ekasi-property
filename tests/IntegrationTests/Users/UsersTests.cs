using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.Users;

public sealed class UsersTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_Should_ReturnUserId()
    {
        // Act
        Guid userId = await RegisterUserAsync(UniqueEmail());

        // Assert
        userId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Login_Should_ReturnAccessToken()
    {
        // Arrange
        string email = UniqueEmail();
        await RegisterUserAsync(email);

        // Act
        AccessTokens tokens = await LoginAsync(email);

        // Assert
        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Should_SetRefreshTokenAsHttpOnlyCookie()
    {
        // Arrange
        string email = UniqueEmail();
        await RegisterUserAsync(email);

        // Act
        HttpResponseMessage response = await LoginResponseAsync(email);

        // Assert
        response.EnsureSuccessStatusCode();
        CurrentRefreshToken().ShouldNotBeNullOrWhiteSpace();

        string cookie = response.Headers.GetValues("Set-Cookie")
            .First(c => c.StartsWith(RefreshTokenCookieName, StringComparison.Ordinal));

        cookie.ShouldContain("httponly", Case.Insensitive);
        cookie.ShouldContain("samesite=strict", Case.Insensitive);
    }

    [Fact]
    public async Task Login_Should_NotReturnRefreshTokenInBody()
    {
        // Arrange
        string email = UniqueEmail();
        await RegisterUserAsync(email);

        // Act
        HttpResponseMessage response = await LoginResponseAsync(email);
        string body = await response.Content.ReadAsStringAsync();

        // Assert — the long-lived credential must never be readable from script.
        body.ShouldNotContain("refreshToken", Case.Insensitive);
    }

    [Fact]
    public async Task Login_Should_ReturnProblem_WhenPasswordIsInvalid()
    {
        // Arrange
        string email = UniqueEmail();
        await RegisterUserAsync(email);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "users/login",
            new { email, password = "WrongPassword1" });

        // Assert
        response.IsSuccessStatusCode.ShouldBeFalse();
    }

    [Fact]
    public async Task RefreshToken_Should_RotateTheCookie()
    {
        // Arrange
        string email = UniqueEmail();
        await RegisterUserAsync(email);
        await LoginAsync(email);
        string? original = CurrentRefreshToken();

        // Act — the cookie rides along automatically, as it would from a browser.
        HttpResponseMessage response = await PostWithCsrfAsync("users/refresh-token");

        // Assert
        response.EnsureSuccessStatusCode();
        AccessTokens? rotated = await response.Content.ReadFromJsonAsync<AccessTokens>();
        rotated!.AccessToken.ShouldNotBeNullOrWhiteSpace();
        CurrentRefreshToken().ShouldNotBe(original);
    }

    [Fact]
    public async Task RefreshToken_Should_ReturnForbidden_WhenCsrfTokenIsMissing()
    {
        // Arrange
        string email = UniqueEmail();
        await RegisterUserAsync(email);
        await LoginAsync(email);

        // Act — a valid session cookie but no CSRF header, which is what a cross-site
        // forgery attempt looks like.
        HttpResponseMessage response = await HttpClient.PostAsync("users/refresh-token", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RefreshToken_Should_ReturnUnauthorized_WhenCookieIsMissing()
    {
        // Act
        HttpResponseMessage response = await PostWithCsrfAsync("users/refresh-token");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_Should_Fail_WhenRotatedTokenIsReused()
    {
        // Arrange
        string email = UniqueEmail();
        await RegisterUserAsync(email);
        await LoginAsync(email);
        string original = CurrentRefreshToken()!;

        (await PostWithCsrfAsync("users/refresh-token")).EnsureSuccessStatusCode();

        // Act — put the pre-rotation token back and present it again.
        SetRefreshToken(original);
        HttpResponseMessage response = await PostWithCsrfAsync("users/refresh-token");

        // Assert
        response.IsSuccessStatusCode.ShouldBeFalse();
    }

    [Fact]
    public async Task Logout_Should_RevokeTheRefreshToken()
    {
        // Arrange
        string email = UniqueEmail();
        await RegisterUserAsync(email);
        await LoginAsync(email);
        string original = CurrentRefreshToken()!;

        // Act
        HttpResponseMessage logout = await PostWithCsrfAsync("users/logout");

        // Assert
        logout.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        SetRefreshToken(original);
        HttpResponseMessage afterLogout = await PostWithCsrfAsync("users/refresh-token");

        afterLogout.IsSuccessStatusCode.ShouldBeFalse();
    }

    [Fact]
    public async Task Logout_Should_Succeed_WhenNoCookieIsPresent()
    {
        // Act
        HttpResponseMessage response = await PostWithCsrfAsync("users/logout");

        // Assert — logging out when already logged out is the desired end state.
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AntiforgeryToken_Should_ReturnTokenAndSetCookie()
    {
        // Act
        string token = await GetCsrfTokenAsync();

        // Assert
        token.ShouldNotBeNullOrWhiteSpace();
        Cookies.GetCookies(new Uri("http://localhost"))["ekasi.csrf"].ShouldNotBeNull();
    }
}
