namespace Isusov.Time.Calendar
{
    /// <summary>
    /// Specifies the type of leap year rule applied when determining whether a year is a leap year.
    /// </summary>
    /// <remarks>Use this enumeration to select the leap year calculation method for calendar systems. The
    /// available options include no leap year rule, a rule based on every Nth year, or the standard Gregorian rule. The
    /// choice of rule affects how years are classified as leap years in date and time calculations.</remarks>
    public enum LeapYearRuleType
    {
        None = 0,
        EveryNthYear = 1,
        Gregorian = 2
    }
}
