using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TuneTrail.Api.Contract;
using TuneTrail.Api.Schemas.Requests;
using TuneTrail.Api.Schemas.Responses;
using TuneTrail.Api.Schemas.Results;

namespace TuneTrail.Api.Module;

public class MusicModule : IRegisterModule
{
    public void RegisterModule(WebApplication app)
    {
        var musicGroup = app.MapGroup("/musics").WithTags("Musics");

        musicGroup
            .MapGet("/{id}", GetMusicById)
            .WithName("GetMusicById")
            .WithDescription("Gets a single music entry by its unique identifier.");

        musicGroup
            .MapGet("/", ListMusics)
            .WithName("ListMusics")
            .WithDescription("Lists music entries, optionally filtering by title and artist.");

        musicGroup
            .MapPost("/", CreateMusic)
            .WithName("CreateMusic")
            .WithDescription("Creates a new music entry.");

        musicGroup
            .MapPut("/{id}", UpdateMusic)
            .WithName("UpdateMusic")
            .WithDescription("Updates an existing music entry.");

        musicGroup
            .MapDelete("/{id}", DeleteMusic)
            .WithName("DeleteMusic")
            .WithDescription("Soft deletes an existing music entry.");
    }

    public static async Task<
        Results<Ok<MusicResponse>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>
    > GetMusicById([FromRoute] Guid id, [FromServices] IMusicAggregate musicAggregate)
    {
        var result = await musicAggregate.GetMusicById(id);

        if (result.IsFailure)
        {
            if (result.Code == ResultError.MusicNotFound.Code)
                return TypedResults.NotFound(new ErrorResponse(result));

            return TypedResults.BadRequest(new ErrorResponse(result));
        }

        return TypedResults.Ok(result.Value);
    }

    public static async Task<
        Results<Ok<IEnumerable<MusicResponse>>, BadRequest<ErrorResponse>>
    > ListMusics(
        [FromQuery] string? title,
        [FromQuery] string? artist,
        [FromServices] IMusicAggregate musicAggregate
    )
    {
        var result = await musicAggregate.ListMusics(title, artist);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ErrorResponse(result));

        return TypedResults.Ok(result.Value);
    }

    public static async Task<
        Results<Created<MusicResponse>, BadRequest<ErrorResponse>>
    > CreateMusic([FromBody] MusicRequest request, [FromServices] IMusicAggregate musicAggregate)
    {
        var result = await musicAggregate.CreateMusic(request);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ErrorResponse(result));

        return TypedResults.Created($"/musics/{result.Value.Id}", result.Value);
    }

    public static async Task<
        Results<Ok<MusicResponse>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>
    > UpdateMusic(
        [FromRoute] Guid id,
        [FromBody] MusicRequest request,
        [FromServices] IMusicAggregate musicAggregate
    )
    {
        var result = await musicAggregate.UpdateMusic(id, request);

        if (result.IsFailure)
        {
            if (result.Code == ResultError.MusicNotFound.Code)
                return TypedResults.NotFound(new ErrorResponse(result));

            return TypedResults.BadRequest(new ErrorResponse(result));
        }

        return TypedResults.Ok(result.Value);
    }

    public static async Task<
        Results<NoContent, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>
    > DeleteMusic([FromRoute] Guid id, [FromServices] IMusicAggregate musicAggregate)
    {
        var result = await musicAggregate.DeleteMusic(id);

        if (result.IsFailure)
        {
            if (result.Code == ResultError.MusicNotFound.Code)
                return TypedResults.NotFound(new ErrorResponse(result));

            return TypedResults.BadRequest(new ErrorResponse(result));
        }

        return TypedResults.NoContent();
    }
}
