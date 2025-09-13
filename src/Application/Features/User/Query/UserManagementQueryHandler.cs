//using Application.Abstractions.Authentication;
//using Domain.Errors;
//using Domain.Models;
//using Microsoft.EntityFrameworkCore;
//using SharedKernel;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Application.Abstractions.Data;
//using Domain.DTOs;
//using System.Collections.Generic;


//namespace Application.Features.User.Query
//{
//    public class UserManagementQueryHandler(
//        IApplicationDbContext context,
//        IPasswordHasher passwordHasher,
//        ITokenProvider tokenProvider)
//    {
//        /// <summary>
//        /// Authenticates a user with the provided email and password.
//        /// </summary>
//        /// <param name="Email">The email of the user.</param>
//        /// <param name="Password">The password of the user.</param>
//        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
//        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a string token if successful.</returns>
//        public async Task<Result<LoginResponseDTO>> Login(string Email, string Password, CancellationToken cancellationToken)
//        {
//            // search the user in db.
//            //var query = await _postgresContext.Usercompanies.Where(x => x.Login == loginDTO.Username && x.State == true).ToListAsync();
//            Usuarios user = await context.Usuarios
//                .Include(u => u.id_perfilNavigation)
//                .AsNoTracking()
//                .Where(u => u.correo == Email && u.estado)
//                .FirstOrDefaultAsync(cancellationToken);

//            if (user is null)
//                return Result.Failure<LoginResponseDTO>(UserErrors.NotFoundByName);

//            if (user.clave_hash == null)
//                return Result.Failure<LoginResponseDTO>(UserErrors.PasswordNotSet);
//            if (!passwordHasher.Verify(passwordToVerify: Password, passwordHash: user.clave_hash))
//                return Result.Failure<LoginResponseDTO>(UserErrors.NotFoundByName);

//            string token = tokenProvider.GenerateJWTToken(user);
//            //tokenProvider.ValidateToken(token);

//            return new LoginResponseDTO
//            {
//                Identificacion = user.identificacion,
//                Nombre = user.nombre_completo,
//                Correo = user.correo,
//                Token = token,
//                NombreIPS = user.nombre_IPS,
//                CodigoIPS = user.codigo_IPS,
//                CodigoHabilitacion = user.codigo_Habilitacion,
//                IdPerfil = user.id_perfil,
//                NombrePerfil = user.id_perfilNavigation.nombre_perfil,
//            };
//        }

//        /// <summary>
//        /// Retrieves the screen permissions for a user.
//        /// </summary>
//        /// <param name = "userId" > The ID (cedula) of the user.</param>
//        /// <param name = "cancellationToken" > Cancellation token to cancel the operation.</param>
//        /// <returns>A task that represents the asynchronous operation.The task result contains a Result object with a list of user permissions if successful.</returns>
//        public async Task<Result<List<UserPermissionsDTO>>> GetUserScreenPermissionsAsync(string userId, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrEmpty(userId))
//                return Result.Failure<List<UserPermissionsDTO>>(UserErrors.InvalidId);

//            // Retrieve the user by their ID to get the profile ID
//            var user = await context.Usuarios
//                .AsNoTracking()
//                .Where(u => u.identificacion == userId && u.estado)
//                .Select(u => new { u.id_perfil })
//                .FirstOrDefaultAsync(cancellationToken);

//            if (user == null)
//                return Result.Failure<List<UserPermissionsDTO>>(UserErrors.NotFound(userId));

//            // Fetch the screen permissions associated with the profile ID
//            var permissions = await context.permisos_por_perfil
//                .AsNoTracking()
//                .Where(p => p.id == user.id_perfil)
//                .Select(p => new ScreenPermission
//                {
//                    ScreenId = p.id_pantalla,
//                    Screen = p.id_pantallaNavigation.nombre_pantalla,
//                    Route = p.id_pantallaNavigation.ruta_frontend,
//                    Icon = "",
//                    Permissions = new Permission
//                    {
//                        View = p.puede_leer,
//                        Add = p.puede_crear,
//                        Edit = p.puede_actualizar,
//                        Delete = p.puede_eliminar,
//                        Special = false,
//                    }
//                })
//                .ToListAsync(cancellationToken);

//            // Map the permissions to the UserPermissionsDTO object
//            var userPermissions = new UserPermissionsDTO
//            {
//                ProfileId = user.id_perfil,
//                Permissions = permissions
//            };

//            return Result.Success(new List<UserPermissionsDTO> { userPermissions });
//        }
//    }
//}
