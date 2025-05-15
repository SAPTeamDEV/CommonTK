namespace SAPTeam.CommonTK.Status;

/// <summary>
/// Represents type of progress bar.
/// </summary>
public enum ProgressBarType
{
    /// <summary>
    /// Default value.
    /// </summary>
    None,

    /// <summary>
    /// Represents a waiting Progress bar without percentage. Suitable for actions that can't be clearly tracked.
    /// </summary>
    Wait,

    /// <summary>
    /// Represents a block Progress bar that can be increased. Suitable for actions that can be clearly tracked.
    /// </summary>
    Block
}
