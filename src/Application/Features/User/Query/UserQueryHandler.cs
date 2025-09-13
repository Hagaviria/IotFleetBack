using Application.Abstractions.Data;
using Domain.Models;
using Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserEntity = Domain.Models.User;

namespace Application.Features.User.Query
{
    public class UserQueryHandler(
        IApplicationDbContext context
        )
    {
        public async Task<Result<UserEntity>> GetUserById(string userId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Result.Failure<UserEntity>(Error.NotFound("User.NotFound", "User not found"));

            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userIdGuid && u.USU_ESTADO, cancellationToken);

            if (user == null)
                return Result.Failure<UserEntity>(Error.NotFound("User.NotFound", "User not found"));

            return Result.Success(user);
        }

        public async Task<Result<List<UserEntity>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var users = await context.Users
                .AsNoTracking()
                .Where(u => u.USU_ESTADO)
                .ToListAsync(cancellationToken);

            return Result.Success(users);
        }
    }
}