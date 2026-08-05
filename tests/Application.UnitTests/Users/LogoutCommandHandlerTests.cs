using Application.UnitTests.Abstractions;
using Application.Users.Logout;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Users;

public sealed class LogoutCommandHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_Should_RemoveTheRefreshToken()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        await SeedRefreshTokenAsync(context, "live-token");

        var handler = new LogoutCommandHandler(context);

        // Act
        Result result = await handler.Handle(new LogoutCommand("live-token"), default);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await context.RefreshTokens.AnyAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_Should_Succeed_WhenTokenIsAlreadyGone()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();

        var handler = new LogoutCommandHandler(context);

        // Act
        Result result = await handler.Handle(new LogoutCommand("missing-token"), default);

        // Assert — the end state is already what the caller asked for, so this is not an
        // error; failing would also reveal which tokens are live.
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_LeaveOtherSessionsIntact()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        await SeedRefreshTokenAsync(context, "this-device");
        await SeedRefreshTokenAsync(context, "other-device");

        var handler = new LogoutCommandHandler(context);

        // Act
        await handler.Handle(new LogoutCommand("this-device"), default);

        // Assert — logging out of one browser must not sign the user out everywhere.
        RefreshToken remaining = await context.RefreshTokens.SingleAsync();
        remaining.Token.ShouldBe("other-device");
    }

    private static async Task SeedRefreshTokenAsync(TestDbContext context, string token)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{token}@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = user.Id,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
            User = user
        });

        await context.SaveChangesAsync();
    }
}
