using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Models;
using Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.User.Query
{
    public class UserManagementQueryHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenProvider tokenProvider
        )
    {
        /// <summary>
        /// Authenticates a user with the provided email and password.
        /// </summary>
        /// <param name="Email">The email of the user.</param>
        /// <param name="Password">The password of the user.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a login response if successful.</returns>
        public async Task<Result<LoginResponseDTO>> Login(string Email, string Password, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo == Email && u.Estado, cancellationToken);

            if (user == null)
                return Result.Failure<LoginResponseDTO>(Error.NotFound("User.NotFound", "User not found"));

       
            if (!passwordHasher.Verify(Password, user.PasswordHash))
                return Result.Failure<LoginResponseDTO>(Error.Problem("User.InvalidPassword", "Invalid password"));

          
            string token = tokenProvider.Create(user);

            var loginResponse = new LoginResponseDTO
            {
                Identificacion = user.Id.ToString(),
                Nombre = user.Nombre_completo,
                Correo = user.Correo,
                Token = token,
                IdPerfil = user.Id_perfil,
                NombrePerfil = user.Nombre_perfil
            };

            return Result.Success(loginResponse);
        }

        /// <summary>
        /// Retrieves the screen permissions for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a list of user permissions if successful.</returns>
        public async Task<Result<List<UserPermissionsDTO>>> GetUserScreenPermissionsAsync(string userId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Result.Failure<List<UserPermissionsDTO>>(Error.Problem("User.InvalidId", "Invalid user ID format"));

            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userIdGuid && u.Estado, cancellationToken);

            if (user == null)
                return Result.Failure<List<UserPermissionsDTO>>(Error.NotFound("User.NotFound", "User not found"));

       
            var userPermissions = new UserPermissionsDTO
            {
                ProfileId = 1, 
                Permissions = new List<ScreenPermission>()
            };

            return Result.Success(new List<UserPermissionsDTO> { userPermissions });
        }
    }
}