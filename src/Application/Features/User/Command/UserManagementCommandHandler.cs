using Application.Abstractions.Data;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.User.Command
{
    public class UserManagementCommandHandler(
        IApplicationDbContext context
        )
    {
        /// <summary>
        /// Changes the password of a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="command">The change password command.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a string indicating success or failure.</returns>
        public async Task<Result<string>> ChangePassword(string userId, ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Result.Failure<string>(Error.Problem("User.InvalidId", "Invalid user ID format"));

            if (command.OldPassword == command.NewPassword)
                return Result.Failure<string>(Error.Problem("User.SamePassword", "New password must be different from old password"));

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userIdGuid, cancellationToken);

            if (user == null)
                return Result.Failure<string>(Error.NotFound("User.NotFound", "User not found"));

            // In a real application, you would verify the old password here
            // For now, we'll just update the password
            user.PasswordHash = command.NewPassword; // In real app, this should be hashed
            user.UpdatedAtDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success(user.Id.ToString());
        }
    }
}