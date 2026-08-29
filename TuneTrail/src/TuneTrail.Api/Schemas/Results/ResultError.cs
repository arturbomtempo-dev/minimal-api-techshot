namespace TuneTrail.Api.Schemas.Results;

public readonly struct ResultError
{
    public ResultError(string message, string code)
    {
        Message = message;
        Code = code;
    }

    public string Message { get; }
    public string Code { get; }

    /*
     * Error Code Pattern: E + Operation + Module + Id
     *
     * [ Operation | Code ]        [ Module  | Code ]
     * [ Generic   | 00   ]        [ Generic | 00   ]
     * [ Insert    | 10   ]        [ Music   | 01   ]
     * [ Update    | 20   ]
     * [ Get       | 30   ]        [ Id: 01 to 99 ]
     * [ Delete    | 40   ]
     *
     * Example: E300101 -> Get / Music / first error
     */

    // Generic errors (00)
    public static readonly ResultError UnexpectedError = new(
        "An unexpected error occurred.",
        "E000000"
    );

    public static ResultError RequiredField(string field, string message) =>
        new($"The field {field} is required. {message}", "E100099");

    public static ResultError InvalidField(string field, string message) =>
        new($"The field {field} is invalid. {message}", "E100098");

    // Music errors (01)
    public static readonly ResultError MusicNotFound = new("Music not found.", "E300101");

    public static readonly ResultError DuplicatedMusic = new(
        "There is already a music with this title for this artist.",
        "E100101"
    );

    public static readonly ResultError ErrorOnListingMusics = new(
        "Error listing musics.",
        "E300102"
    );

    public static readonly ResultError ErrorOnCreatingMusic = new(
        "Error creating music.",
        "E100102"
    );

    public static readonly ResultError ErrorOnUpdatingMusic = new(
        "Error updating music.",
        "E200101"
    );

    public static readonly ResultError ErrorOnDeletingMusic = new(
        "Error deleting music.",
        "E400101"
    );
}
