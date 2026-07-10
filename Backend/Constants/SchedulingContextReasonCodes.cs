namespace Backend.Constants;

public static class SchedulingContextReasonCodes
{
    public const string NextTermAvailable = "NEXT_TERM_AVAILABLE";
    public const string NoFutureTerm = "NO_FUTURE_TERM";
    public const string InvalidMultipleCurrentTerms = "INVALID_MULTIPLE_CURRENT_TERMS";
    public const string TermAlreadyStarted = "TERM_ALREADY_STARTED";
    public const string TermAlreadyEnded = "TERM_ALREADY_ENDED";
    public const string TermNotNearestFuture = "TERM_NOT_NEAREST_FUTURE";
    public const string CrossCampusTerm = "CROSS_CAMPUS_TERM";
    public const string MissingRequiredData = "MISSING_REQUIRED_DATA";
}
