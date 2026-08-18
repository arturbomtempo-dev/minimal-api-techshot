using TuneTrail.API.Shared;

namespace TuneTrail.Api.Schemas.Responses;

public class MusicResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Artist { get; set; } = default!;
    public MusicGenre Genre { get; set; }
    public ListeningStatus Status { get; set; }
    public int? PersonalRating { get; set; }
    public int PlayCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
