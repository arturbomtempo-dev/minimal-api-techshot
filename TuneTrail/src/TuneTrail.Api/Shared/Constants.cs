namespace TuneTrail.API.Shared;

public static class Constants
{
    public static class CharacterLimits
    {
        public const int ONE_HUNDRED = 100;
        public const int TWO_HUNDRED = 200;
    }

    public static class RatingRange
    {
        public const int MIN = 0;
        public const int MAX = 10;
    }

    public static class MusicValidationMessages
    {
        public const string TITLE_FIELD = "Title is required and must not exceed 200 characters.";
        public const string ARTIST_FIELD = "Artist is required and must not exceed 100 characters.";
        public const string GENRE_FIELD = "Genre must be a valid value.";
        public const string STATUS_FIELD = "Status must be a valid value.";
        public const string PERSONAL_RATING_FIELD = "Personal rating must be between 0 and 10.";
        public const string PLAY_COUNT_FIELD = "Play count cannot be negative.";
    }
}
