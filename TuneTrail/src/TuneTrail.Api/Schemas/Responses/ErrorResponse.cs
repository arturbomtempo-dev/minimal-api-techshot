using TuneTrail.Api.Schemas.Results;

namespace TuneTrail.Api.Schemas.Responses;

public class ErrorResponse
{
    public string Code { get; set; }
    public string Message { get; set; }

    public ErrorResponse(ResultSchema result)
    {
        Code = result.Code;
        Message = result.Message;
    }
}
