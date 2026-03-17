using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MundialCorporativo.Api.Common;
using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Api.Tests.Common;

public class ApiResultMapperTests
{
    private static ControllerBase Controller()
    {
        var controller = new TestController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public void Success_ReturnsOk200()
    {
        var result = Controller().ToActionResult(Result.Success());

        var ok = Assert.IsType<OkResult>(result);
        Assert.Equal(200, ok.StatusCode);
    }

    [Fact]
    public void ValidationFailure_ReturnsBadRequest()
    {
        var result = Controller().ToActionResult(Result.Failure("bad input", "Validation"));

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public void NotFoundFailure_ReturnsNotFound()
    {
        var result = Controller().ToActionResult(Result.Failure("not found", "NotFound"));

        var nf = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, nf.StatusCode);
    }

    [Fact]
    public void ConflictFailure_ReturnsConflict()
    {
        var result = Controller().ToActionResult(Result.Failure("duplicate", "Conflict"));

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);
    }

    [Fact]
    public void UnknownErrorCode_ReturnsBadRequest()
    {
        var result = Controller().ToActionResult(Result.Failure("oops", "Unknown"));

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }

    // Minimal controller needed to use extension methods
    private class TestController : ControllerBase { }
}
