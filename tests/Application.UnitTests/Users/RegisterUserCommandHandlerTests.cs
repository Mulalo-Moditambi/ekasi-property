using Application.Abstractions.Authentication;
using Application.Users.Register;
using Application.UnitTests.Abstractions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Users;

public sealed class RegisterUserCommandHandlerTests : BaseHandlerTest
{
    private static RegisterUserCommand Command =>
        new("test@example.com", "Test", "User", "Password123", "+27821234567");

    [Fact]
    public async Task Handle_Should_ReturnConflict_WhenEmailIsNotUnique()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = Command.Email,
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "hash",
            PhoneNumber = "+27821234567"
        });
        await context.SaveChangesAsync();

        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        var handler = new RegisterUserCommandHandler(context, Substitute.For<IPasswordHasher>(), dateTimeProvider);

        // Act
        Result<Guid> result = await handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.EmailNotUnique);
    }

    [Fact]
    public async Task Handle_Should_CreateUserWithHashedPasswordAndRaiseDomainEvent_WhenValid()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();

        IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Hash(Command.Password).Returns("hashed-password");

        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        var handler = new RegisterUserCommandHandler(context, passwordHasher, dateTimeProvider);

        // Act
        Result<Guid> result = await handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        User user = await context.Users.SingleAsync(u => u.Id == result.Value);
        user.Email.ShouldBe(Command.Email);
        user.PasswordHash.ShouldBe("hashed-password");
        user.PhoneNumber.ShouldBe(Command.PhoneNumber);
        user.DomainEvents.ShouldContain(domainEvent => domainEvent is UserRegisteredDomainEvent);
    }
}
