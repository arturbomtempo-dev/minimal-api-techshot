using TuneTrail.Api.Data.Database.Entities;

namespace TuneTrail.Api.Schemas.Responses.Mapping;

public static class MusicResponseMapping
{
    public static MusicResponse? MapToResponse(Music? music)
    {
        if (music is null)
            return null;

        return new MusicResponse
        {
            Id = music.Id,
            Title = music.Title,
            Artist = music.Artist,
            Genre = music.Genre,
            Status = music.Status,
            PersonalRating = music.PersonalRating,
            PlayCount = music.PlayCount,
            CreatedAt = music.CreatedAt,
            UpdatedAt = music.UpdatedAt,
        };
    }
}
