using Application.Abstractions.Data;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserEntity = Domain.Models.User;

namespace Application.Features.User.Command
{
    public class UserCommandHandler(
        IApplicationDbContext context
        )
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">The user create command containing user details.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a string user ID if successful.</returns>
        public async Task<Result<string>> CreateUser(UserCreateCommand user, CancellationToken cancellationToken)
        {
            if (user == null)
                return Result.Failure<string>(Error.Problem("User.Null", "User cannot be null"));

            if (string.IsNullOrWhiteSpace(user.Identificacion))
                return Result.Failure<string>(Error.Problem("User.InvalidId", "User ID is required"));

  
            var existingUser = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == user.Correo, cancellationToken);

            if (existingUser != null)
                return Result.Failure<string>(Error.Conflict("User.EmailExists", "User with this email already exists"));

            var newUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = user.Correo,
                FirstName = user.Nombre_completo,
                LastName = "",
                PasswordHash = user.Contraseña, 
                Role = "User",
                CreatedAtDate = DateTime.UtcNow,
                UpdatedAtDate = DateTime.UtcNow,
                USU_ESTADO = true
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success(newUser.Id.ToString());
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="userID">The ID of the user to update.</param>
        /// <param name="user">The user update command with updated details.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the updated user details if successful.</returns>
        public async Task<Result<UserEntity>> UpdateUser(string userID, UserUpdateCommand user, CancellationToken cancellationToken)
        {
            if (user == null)
                return Result.Failure<UserEntity>(Error.Problem("User.Null", "User cannot be null"));

            if (!Guid.TryParse(userID, out var userId))
                return Result.Failure<UserEntity>(Error.Problem("User.InvalidId", "Invalid user ID format"));

            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (existingUser == null)
                return Result.Failure<UserEntity>(Error.NotFound("User.NotFound", "User not found"));

            existingUser.Email = user.Correo;
            existingUser.FirstName = user.Nombre_completo;
            existingUser.PasswordHash = user.Contraseña; // In real app, this should be hashed
            existingUser.UpdatedAtDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success(existingUser);
        }

        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object indicating success or failure.</returns>
        public async Task<Result> DeleteUser(string userId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Result.Failure(Error.Problem("User.InvalidId", "Invalid user ID format"));

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userIdGuid, cancellationToken);

            if (user == null)
                return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

            user.USU_ESTADO = false;
            user.UpdatedAtDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}