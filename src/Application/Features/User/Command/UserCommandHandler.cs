//using Application.Abstractions.Authentication;
//using Application.Abstractions.Data;
//using Domain.DTOs;
//using Domain.Errors;
//using Domain.Models;
//using Microsoft.EntityFrameworkCore;
//using SharedKernel;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Application.Features.User.Command
//{
//    public class UserCommandHandler(
//            IApplicationDbContext context,
//            IPasswordHasher passwordHasher,
//            IDateTimeProvider dateTimeProvider
//            )
//    {
//        /// <summary>
//        /// Updates an existing user.
//        /// </summary>
//        /// <param name = "user" > The user command with updated details. <seealso cref = "UserUpdateCommand" /></ param >
//        /// < param name= "cancellationToken" > Cancellation token to cancel the operation.</param>
//        /// <returns>A task that represents the asynchronous operation.The task result contains a Result object with the updated user details if successful.</returns>
//        public async Task<Result<UserDTO>> UpdateUser(string userID, UserUpdateCommand user, CancellationToken cancellationToken)
//        {
//            //validate null values
//            if (user is null)
//                return Result.Failure<UserDTO>(UserErrors.UserNull);
//            //validate null and white space
//            if (string.IsNullOrWhiteSpace(user.Identificacion) || string.IsNullOrWhiteSpace(userID))
//                return Result.Failure<UserDTO>(UserErrors.InvalidId);
//            //validate if the user id match the user id in the user object
//            if (user.Identificacion != userID)
//                return Result.Failure<UserDTO>(UserErrors.IdConflict);

//            //getting the user
//            var existingUser = await context.Usuarios
//                .FirstOrDefaultAsync(u => u.identificacion == user.Identificacion, cancellationToken);
//            if (existingUser is null)
//                return Result.Failure<UserDTO>(UserErrors.NotFound(user.Identificacion));

//            //validate duplicate email
//            if (existingUser.correo != user.Correo)
//            {
//                bool _isDuplicateEmail = await context.Usuarios
//                .AsNoTracking()
//                .AnyAsync(u => u.correo == user.Correo, cancellationToken);
//                if (_isDuplicateEmail)
//                    return Result.Failure<UserDTO>(UserErrors.EmailNotUnique);
//            }

//            // Use a transaction scope instead of context.Database.BeginTransactionAsync
//            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
//            try
//            {
//                existingUser.nombre_completo = user.Nombre_completo;
//                existingUser.correo = user.Correo;
//                existingUser.clave_hash = passwordHasher.Hash(user.Contraseña);
//                existingUser.id_perfil = user.Id_perfil;
//                existingUser.estado = user.Estado;
//                existingUser.creado_en = dateTimeProvider.LocalTimeNow;
//                existingUser.nombre_IPS = user.Nombre_IPS;
//                existingUser.codigo_IPS = user.Codigo_IPS;
//                existingUser.codigo_Habilitacion = user.Codigo_Habilitacion;
//                existingUser.direccion = user.Direccion;
//                existingUser.telefono_fijo = user.TelefonoFijo;
//                existingUser.telefono_celular = user.TelefonoCelular;
//                existingUser.fecha_nacimiento = user.FechaNacimiento;
//                existingUser.tipo_identificacion = user.TipoIdentificacion;

//                //saving changes
//                context.Entry(existingUser).State = EntityState.Modified;
//                await context.SaveChangesAsync(cancellationToken);
//                await transaction.CommitAsync(cancellationToken);

//                return new UserDTO
//                {
//                    Identificacion = existingUser.identificacion,
//                    Nombre_completo = existingUser.nombre_completo,
//                    Correo = existingUser.correo,
//                    Id_perfil = existingUser.id_perfil,
//                    Estado = existingUser.estado,
//                };
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync(cancellationToken);
//                return Result.Failure<UserDTO>(UserErrors.UnknownDatabaseTransaction(ex.Message));
//            }
//        }
//        /// <summary>
//        /// Deletes a user by their ID.
//        /// </summary>
//        /// <param name="userId">The ID of the user to delete.</param>
//        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
//        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object indicating success or failure.</returns>
//        public async Task<Result> DeleteUser(string userId, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrEmpty(userId))
//                return Result.Failure(UserErrors.UserNull);

//            //getting the user
//            var existingUser = await context.Usuarios
//                .FirstOrDefaultAsync(u => u.identificacion == userId, cancellationToken);
//            if (existingUser is null)
//                return Result.Failure(UserErrors.NotFound(userId));

//            // Use a transaction scope instead of context.Database.BeginTransactionAsync
//            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
//            try
//            {
//                existingUser.estado = false;
//                context.Entry(existingUser).State = EntityState.Modified;
//                await context.SaveChangesAsync(cancellationToken);
//                await transaction.CommitAsync(cancellationToken);

//                return Result.Success();
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync(cancellationToken);
//                return Result.Failure(UserErrors.UnknownDatabaseTransaction(ex.Message));
//            }
//        }
//        /// <summary>
//        /// Creates a new user.
//        /// </summary>
//        /// <param name="user">The user create command containing user details. <seealso cref="UserCreateCommand"/></param>
//        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
//        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a string user ID if successful.</returns>
//        public async Task<Result<string>> CreateUser(UserCreateCommand user, CancellationToken cancellationToken)
//        {
//            if (user == null) //validate null
//                return Result.Failure<string>(UserErrors.UserNull);
//            if (string.IsNullOrWhiteSpace(user.Identificacion))
//                return Result.Failure<string>(UserErrors.InvalidId);
//            //validate duplicate email
//            bool _isDuplicateEmail = await context.Usuarios
//                .AsNoTracking()
//                .AnyAsync(u => u.correo == user.Correo, cancellationToken);
//            if (_isDuplicateEmail)
//                return Result.Failure<string>(UserErrors.EmailNotUnique);

//            var _newUser = new Usuarios
//            {
//                identificacion = user.Identificacion,
//                nombre_completo = user.Nombre_completo,
//                correo = user.Correo,
//                clave_hash = passwordHasher.Hash(user.Contraseña),
//                id_perfil = user.Id_perfil,
//                estado = user.Estado,
//                creado_en = dateTimeProvider.LocalTimeNow,
//                codigo_Habilitacion = user.Codigo_Habilitacion,
//                codigo_IPS = user.Codigo_IPS,
//                nombre_IPS = user.Nombre_IPS,
//                direccion = user.Direccion,
//                telefono_celular = user.TelefonoCelular,
//                telefono_fijo = user.TelefonoFijo,
//                fecha_nacimiento = user.FechaNacimiento,
//                tipo_identificacion = user.TipoIdentificacion,
//            };
//            // Use a transaction scope instead of context.Database.BeginTransactionAsync
//            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
//            try
//            {
//                context.Usuarios.Add(_newUser);
//                await context.SaveChangesAsync(cancellationToken);
//                await transaction.CommitAsync(cancellationToken);

//                return _newUser.identificacion;
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync(cancellationToken);
//                return Result.Failure<string>(UserErrors.UnknownDatabaseTransaction(ex.Message));
//            }
//        }
//    }
//}
