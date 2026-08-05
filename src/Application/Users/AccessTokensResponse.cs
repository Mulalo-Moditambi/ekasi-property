namespace Application.Users;

/// <summary>
/// The refresh token never reaches the browser as JSON — the endpoint moves it into an
/// httpOnly cookie and returns only the access token. The expiry rides along so the
/// cookie lifetime matches the persisted token instead of restating the same constant.
/// </summary>
public sealed record AccessTokensResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresOnUtc);
