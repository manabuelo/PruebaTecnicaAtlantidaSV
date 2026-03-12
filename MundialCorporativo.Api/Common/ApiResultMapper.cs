using Microsoft.AspNetCore.Mvc;
using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Api.Common;

public static class ApiResultMapper
{
    public static IActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok();
        }

        return result.ErrorCode switch
        {
            "Validation" => controller.BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode }),
            "NotFound" => controller.NotFound(new { error = result.ErrorMessage, code = result.ErrorCode }),
            "Conflict" => controller.Conflict(new { error = result.ErrorMessage, code = result.ErrorCode }),
            _ => controller.BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode })
        };
    }
}
