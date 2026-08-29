using Microsoft.EntityFrameworkCore;
using TuneTrail.Api.Contract;
using TuneTrail.Api.Data.Database.Entities;
using TuneTrail.Api.IoC.Context;
using TuneTrail.Api.Schemas.Requests;
using TuneTrail.Api.Schemas.Responses;
using TuneTrail.Api.Schemas.Responses.Mapping;
using TuneTrail.Api.Schemas.Results;
using TuneTrail.Api.Schemas.Validators;

namespace TuneTrail.Api.Aggregate;

public class MusicAggregate : IMusicAggregate
{
    private readonly TuneTrailDbContext _dbContext;
    private readonly ILogger<MusicAggregate> _logger;

    public MusicAggregate(TuneTrailDbContext dbContext, ILogger<MusicAggregate> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ResultSchema<MusicResponse>> GetMusicById(Guid musicId)
    {
        try
        {
            var music = await _dbContext
                .Set<Music>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == musicId && !m.Deleted);

            if (music is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.MusicNotFound);

            var response = MusicResponseMapping.MapToResponse(music);
            if (response is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.UnexpectedError);

            return ResultSchema<MusicResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting music. Id: {MusicId}", musicId);
            return ResultSchema<MusicResponse>.Fail(ResultError.UnexpectedError);
        }
    }

    public async Task<ResultSchema<IEnumerable<MusicResponse>>> ListMusics(
        string? title = null,
        string? artist = null
    )
    {
        try
        {
            var query = _dbContext.Set<Music>().AsNoTracking().Where(m => !m.Deleted);

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(m => EF.Functions.ILike(m.Title, $"%{title}%"));

            if (!string.IsNullOrWhiteSpace(artist))
                query = query.Where(m => EF.Functions.ILike(m.Artist, $"%{artist}%"));

            var musics = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();

            var response = musics
                .Select(MusicResponseMapping.MapToResponse)
                .OfType<MusicResponse>()
                .ToList();

            return ResultSchema<IEnumerable<MusicResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error listing musics. Title: {Title} | Artist: {Artist}",
                title,
                artist
            );

            return ResultSchema<IEnumerable<MusicResponse>>.Fail(ResultError.ErrorOnListingMusics);
        }
    }

    public async Task<ResultSchema<MusicResponse>> CreateMusic(MusicRequest request)
    {
        try
        {
            var errors = request.ValidationErrors();
            if (errors.Count != 0)
                return ResultSchema<MusicResponse>.Fail(errors[0]);

            var title = request.Title.Trim();
            var artist = request.Artist.Trim();

            var alreadyExists = await _dbContext
                .Set<Music>()
                .AnyAsync(m =>
                    EF.Functions.ILike(m.Title, title)
                    && EF.Functions.ILike(m.Artist, artist)
                    && !m.Deleted
                );

            if (alreadyExists)
                return ResultSchema<MusicResponse>.Fail(ResultError.DuplicatedMusic);

            var music = BuildNewMusic(request);

            _dbContext.Add(music);
            await _dbContext.SaveChangesAsync();

            var response = MusicResponseMapping.MapToResponse(music);
            if (response is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.UnexpectedError);

            return ResultSchema<MusicResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating music. Title: {Title}", request.Title);
            return ResultSchema<MusicResponse>.Fail(ResultError.ErrorOnCreatingMusic);
        }
    }

    public async Task<ResultSchema<MusicResponse>> UpdateMusic(Guid musicId, MusicRequest request)
    {
        try
        {
            var errors = request.ValidationErrors();
            if (errors.Count != 0)
                return ResultSchema<MusicResponse>.Fail(errors[0]);

            var music = await _dbContext
                .Set<Music>()
                .FirstOrDefaultAsync(m => m.Id == musicId && !m.Deleted);

            if (music is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.MusicNotFound);

            UpdateMusicData(music, request);

            await _dbContext.SaveChangesAsync();

            var response = MusicResponseMapping.MapToResponse(music);
            if (response is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.UnexpectedError);

            return ResultSchema<MusicResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating music. Id: {MusicId}", musicId);
            return ResultSchema<MusicResponse>.Fail(ResultError.ErrorOnUpdatingMusic);
        }
    }

    public async Task<ResultSchema> DeleteMusic(Guid musicId)
    {
        try
        {
            var music = await _dbContext
                .Set<Music>()
                .FirstOrDefaultAsync(m => m.Id == musicId && !m.Deleted);

            if (music is null)
                return ResultSchema.Fail(ResultError.MusicNotFound);

            music.Deleted = true;

            await _dbContext.SaveChangesAsync();

            return ResultSchema.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting music. Id: {MusicId}", musicId);
            return ResultSchema.Fail(ResultError.ErrorOnDeletingMusic);
        }
    }

    #region Helper methods

    /// <summary>
    /// Creates a new Music instance from the request values.
    /// </summary>
    private static Music BuildNewMusic(MusicRequest request) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Artist = request.Artist.Trim(),
            Genre = request.Genre,
            Status = request.Status,
            PersonalRating = request.PersonalRating,
            PlayCount = request.PlayCount,
        };

    /// <summary>
    /// Copies the request values into an existing Music instance.
    /// </summary>
    private static void UpdateMusicData(Music music, MusicRequest request)
    {
        music.Title = request.Title.Trim();
        music.Artist = request.Artist.Trim();
        music.Genre = request.Genre;
        music.Status = request.Status;
        music.PersonalRating = request.PersonalRating;
        music.PlayCount = request.PlayCount;
    }

    #endregion Helper methods
}
