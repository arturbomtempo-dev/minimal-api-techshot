using TuneTrail.Api.Schemas.Requests;
using TuneTrail.Api.Schemas.Responses;
using TuneTrail.Api.Schemas.Results;

namespace TuneTrail.Api.Contract;

public interface IMusicAggregate
{
    Task<ResultSchema<MusicResponse>> GetMusicById(Guid musicId);

    Task<ResultSchema<IEnumerable<MusicResponse>>> ListMusics(
        string? title = null,
        string? artist = null
    );

    Task<ResultSchema<MusicResponse>> CreateMusic(MusicRequest request);
    
    Task<ResultSchema<MusicResponse>> UpdateMusic(Guid musicId, MusicRequest request);

    Task<ResultSchema> DeleteMusic(Guid musicId);
}
