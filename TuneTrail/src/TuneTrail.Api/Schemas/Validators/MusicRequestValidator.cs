using TuneTrail.Api.Schemas.Requests;
using TuneTrail.Api.Schemas.Results;
using static TuneTrail.Api.Shared.Constants;

namespace TuneTrail.Api.Schemas.Validators;

public static class MusicRequestValidator
{
    public static List<ResultError> ValidationErrors(this MusicRequest request)
    {
        var errors = new List<ResultError>();

        if (
            string.IsNullOrWhiteSpace(request.Title)
            || request.Title.Length > CharacterLimits.TWO_HUNDRED
        )
        {
            errors.Add(
                ResultError.RequiredField(
                    nameof(request.Title),
                    MusicValidationMessages.TITLE_FIELD
                )
            );
        }

        if (
            string.IsNullOrWhiteSpace(request.Artist)
            || request.Artist.Length > CharacterLimits.ONE_HUNDRED
        )
        {
            errors.Add(
                ResultError.RequiredField(
                    nameof(request.Artist),
                    MusicValidationMessages.ARTIST_FIELD
                )
            );
        }

        if (!Enum.IsDefined(request.Genre))
        {
            errors.Add(
                ResultError.InvalidField(nameof(request.Genre), MusicValidationMessages.GENRE_FIELD)
            );
        }

        if (!Enum.IsDefined(request.Status))
        {
            errors.Add(
                ResultError.InvalidField(
                    nameof(request.Status),
                    MusicValidationMessages.STATUS_FIELD
                )
            );
        }

        if (
            request.PersonalRating.HasValue
            && (
                request.PersonalRating < RatingRange.MIN || request.PersonalRating > RatingRange.MAX
            )
        )
        {
            errors.Add(
                ResultError.InvalidField(
                    nameof(request.PersonalRating),
                    MusicValidationMessages.PERSONAL_RATING_FIELD
                )
            );
        }

        if (request.PlayCount < 0)
        {
            errors.Add(
                ResultError.InvalidField(
                    nameof(request.PlayCount),
                    MusicValidationMessages.PLAY_COUNT_FIELD
                )
            );
        }

        return errors;
    }
}
