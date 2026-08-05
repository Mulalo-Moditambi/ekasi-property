using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Logout;

internal sealed class LogoutCommandHandler(IApplicationDbContext context)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        RefreshToken? refreshToken = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == command.RefreshToken, cancellationToken);

        // Logging out with a token that is already gone is the desired end state, not an
        // error — returning a failure here would only tell a caller which tokens are live.
        if (refreshToken is not null)
        {
            context.RefreshTokens.Remove(refreshToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
