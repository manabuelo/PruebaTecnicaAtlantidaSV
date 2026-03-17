using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Domain.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Failure_ShouldReturnFailureResultWithMessage()
    {
        var result = Result.Failure("Something went wrong", "Validation");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Something went wrong", result.ErrorMessage);
        Assert.Equal("Validation", result.ErrorCode);
    }

    [Fact]
    public void GenericSuccess_ShouldReturnValueAndSuccess()
    {
        var id = Guid.NewGuid();

        var result = Result<Guid>.Success(id);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value);
    }

    [Fact]
    public void GenericFailure_ShouldReturnNullValueAndError()
    {
        var result = Result<Guid>.Failure("Not found", "NotFound");

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.ErrorCode);
        Assert.Equal(default, result.Value);
    }
}
