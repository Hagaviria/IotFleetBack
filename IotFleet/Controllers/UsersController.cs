using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using Application.Features.User.Command;
using Application.Features.User.Query;
using IotFleet.Infrastructure;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(
        UserCommandHandler userCommand,
        UserQueryHandler userQuery,
        UserManagementCommandHandler userManagementCommand,
        UserManagementQueryHandler userManagementQuery
        ) : ControllerBase
    {
        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var result = await userQuery.GetAllUsers(new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(value),
                CustomResults.Problem
            );
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var result = await userQuery.GetUserById(id, new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(value),
                CustomResults.Problem
            );
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] UserCreateCommand user)
        {
            if (!ModelState.IsValid)
                return CustomResults.Problem(Result.Failure(Error.Problem("General.ModelInvalid", ModelState.SerializeModelStateErrors())));

            var result = await userCommand.CreateUser(user, new CancellationToken());
            return result.Match(
                value => CustomResults.Success(title: "User.Created",
                result: value, status: StatusCodes.Status201Created),
                CustomResults.Problem
            );
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, [FromBody] UserUpdateCommand user)
        {
            if (!ModelState.IsValid)
                return CustomResults.Problem(Result.Failure(Error.Problem("General.ModelInvalid", ModelState.SerializeModelStateErrors())));

            var result = await userCommand.UpdateUser(id, user, new CancellationToken());
            return result.Match(
                value => CustomResults.Success(value, title: "User.Updated"),
                CustomResults.Problem
            );
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await userCommand.DeleteUser(id, new CancellationToken());
            return result.Match(
                () => CustomResults.Success<object>(result: null, title: "User.Deleted", status: StatusCodes.Status202Accepted),
                CustomResults.Problem
            );
        }

        [HttpPost("{id}/ChangePassword")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordCommand passwordCommand)
        {
            if (!ModelState.IsValid)
                return CustomResults.Problem(Result.Failure(Error.Problem("General.ModelInvalid", ModelState.SerializeModelStateErrors())));

            var result = await userManagementCommand.ChangePassword(id, passwordCommand, new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(value),
                CustomResults.Problem
            );
        }

        [HttpGet("{id}/Permissions")]
        public async Task<IActionResult> GetUserScreenPermissions(string id)
        {
            var result = await userManagementQuery.GetUserScreenPermissionsAsync(id, new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(value),
                CustomResults.Problem
            );
        }
    }
}
