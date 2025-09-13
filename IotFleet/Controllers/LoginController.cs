using Application.Features.User.Command;

using IotFleet.Extensions;
using IotFleet.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController(
        //UserManagementQueryHandler userManagementQueryHandler
        )
            : ControllerBase
    {
        //[HttpPost]
        //public async Task<IActionResult> Login([FromBody] LoginCommand request)
        //{
        //    var result = await userManagementQueryHandler.Login(request.Email, request.Password, new CancellationToken());

        //    return result.Match(
        //        value => CustomResults.Success(title: "Login.Success",
        //        result: value, status: StatusCodes.Status201Created),
        //        CustomResults.Problem
        //    );
        //}
    }
}
