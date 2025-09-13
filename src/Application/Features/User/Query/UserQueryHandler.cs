//using Application.Abstractions.Data;
//using Domain.DTOs;
//using Domain.Errors;
//using Microsoft.EntityFrameworkCore;
//using SharedKernel;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Application.Features.User.Query
//{
//    public class UserQueryHandler(
//        IApplicationDbContext context
//        )
//    {
//        public async Task<Result<UserDTO>> GetUserById(string userId, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrEmpty(userId))
//                return Result.Failure<UserDTO>(UserErrors.NotFound(userId));
//            var result = await context.Usuarios
//                .Where(x => x.identificacion == userId && x.estado)
//                .Select(x => new UserDTO
//                {
//                    Identificacion = x.identificacion,
//                    Nombre_completo = x.nombre_completo,
//                    Correo = x.correo,
//                    Id_perfil = x.id_perfil,
//                    Estado = x.estado,
//                    Codigo_Habilitacion = x.codigo_Habilitacion,
//                    Codigo_IPS = x.codigo_IPS,
//                    Nombre_IPS = x.nombre_IPS,
//                    Direccion = x.direccion,
//                    FechaNacimiento = x.fecha_nacimiento,
//                    TelefonoCelular = x.telefono_celular,
//                    TelefonoFijo = x.telefono_fijo,
//                    TipoIdentificacion = x.tipo_identificacion,
//                })
//                .FirstOrDefaultAsync(cancellationToken);
//            if (result is null)
//                return Result.Failure<UserDTO>(UserErrors.NotFound(userId));

//            return result;
//        }

//        public async Task<Result<List<UserDTO>>> GetAllUsers(CancellationToken cancellationToken)
//        {
//            return await context.Usuarios
//                .AsNoTracking()
//                .Where(x => x.estado)
//                .Select(x => new UserDTO
//                {
//                    Identificacion = x.identificacion,
//                    Nombre_completo = x.nombre_completo,
//                    Correo = x.correo,
//                    Id_perfil = x.id_perfil,
//                    Codigo_Habilitacion = x.codigo_Habilitacion,
//                    Codigo_IPS = x.codigo_IPS,
//                    Nombre_IPS = x.nombre_IPS,
//                    Estado = x.estado,
//                    Direccion = x.direccion,
//                    FechaNacimiento = x.fecha_nacimiento,
//                    TelefonoCelular = x.telefono_celular,
//                    TelefonoFijo = x.telefono_fijo,
//                    TipoIdentificacion = x.tipo_identificacion,
//                })
//                .ToListAsync(cancellationToken);
//        }


//    }
//}
