using TuneTrail.Api.Data.Database.Entities.Base;
using TuneTrail.Api.Shared;

namespace TuneTrail.Api.Data.Database.Entities;

/// <summary>
/// Table that stores each music entry of the personal listening log.
/// </summary>
public class Music : BaseEntity
{
    /// <summary>
    /// Unique identifier of the music entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the song, e.g. "Bohemian Rhapsody".
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Performer of the song, e.g. "Queen".
    /// </summary>
    public string Artist { get; set; } = default!;

    /// <summary>
    /// Musical genre of the song.
    /// </summary>
    public MusicGenre Genre { get; set; }

    /// <summary>
    /// Current listening status in the personal log.
    /// </summary>
    public ListeningStatus Status { get; set; }

    /// <summary>
    /// Optional personal score from 0 to 10.
    /// </summary>
    public int? PersonalRating { get; set; }

    /// <summary>
    /// How many times the song was played.
    /// </summary>
    public int PlayCount { get; set; }
}
