//using Application.Abstractions.Authentication;
//using Application.Abstractions.Data;
//using Domain.Errors;
//using Microsoft.EntityFrameworkCore;
//using SharedKernel;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Application.Features.User.Command
//{
//    public class UserManagementCommandHandler(
//        IApplicationDbContext context,
//        IPasswordHasher passwordHasher
//        )
//    {
//        /// <summary>
//        /// Changes the password of a user.
//        /// </summary>
//        /// <param name="userId">The ID of the user.</param>
//        /// <param name="newPassword">The new password for the user.</param>
//        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
//        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a string indicating success or failure.</returns>
//        public async Task<Result<string>> ChangePassword(string userId, ChangePasswordCommand command, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrEmpty(userId))
//                return Result.Failure<string>(UserErrors.InvalidId);
//            if (command.OldPassword == command.NewPassword)
//                return Result.Failure<string>(UserErrors.SamePasswordValidation);

//            // Retrieve the user by their ID
//            var user = await context.Usuarios
//                .FirstOrDefaultAsync(u => u.identificacion == userId, cancellationToken);

//            if (user == null)
//                return Result.Failure<string>(UserErrors.NotFound(userId));

//            // Hash the new password
//            var hashedPassword = passwordHasher.Hash(command.NewPassword);

//            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
//            try
//            {
//                // Update the user's password
//                user.clave_hash = hashedPassword;
//                context.Entry(user).State = EntityState.Modified;
//                await context.SaveChangesAsync(cancellationToken);
//                await transaction.CommitAsync(cancellationToken);

//                return user.identificacion;
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync(cancellationToken);
//                return Result.Failure<string>(UserErrors.UnknownDatabaseTransaction(ex.Message));
//            }
//        }
//    }
//}
