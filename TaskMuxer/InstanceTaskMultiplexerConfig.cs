using System.ComponentModel.DataAnnotations;

namespace TaskMuxer;

public record InstanceTaskMultiplexerConfig : IValidatableObject
{
    /// <summary>
    ///     Intended to preserve the results of task for the duration defined.
    ///     PreserveExecutionResultDuration must be equal or greater than TimeSpan.Zero
    /// </summary>
    /// <value></value>
    public TimeSpan PreserveExecutionResultDuration { get; set; } = TimeSpan.Zero;

    /// <summary>
    ///     Intended to short circuit a task execution if defined duration is exceeded.
    ///     ExecutionTimeout must be greater than TimeSpan.Zero
    /// </summary>
    /// <returns></returns>
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Intended to short circuit a long running task execution if defined duration is exceeded.
    ///     ExecutionTimeout must be greater than TimeSpan.Zero
    /// </summary>
    /// <returns></returns>
    public TimeSpan LongRunningTaskExecutionTimeout { get; set; } = TimeSpan.FromSeconds(300);

    /// <summary>
    ///     Defines the initial collection capacity for the InstanceTaskMultiplexer
    ///     Must be in [Range(10, 100_000)]
    /// </summary>
    /// <value></value>
    [Range(10, 100_000)]
    public int CollectionCapacity { get; set; } = 100;

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PreserveExecutionResultDuration < TimeSpan.Zero)
        {
            yield return new(
                $"{nameof(PreserveExecutionResultDuration)} must be equal or greater than {TimeSpan.Zero}",
                new[] { nameof(PreserveExecutionResultDuration) }
            );
        }

        if (ExecutionTimeout <= TimeSpan.Zero)
        {
            yield return new($"{nameof(ExecutionTimeout)} must be greater than {TimeSpan.Zero}", new[] { nameof(ExecutionTimeout) });
        }

        if (LongRunningTaskExecutionTimeout < ExecutionTimeout)
        {
            yield return new(
                $"{nameof(LongRunningTaskExecutionTimeout)} must be equal or greater than {ExecutionTimeout}",
                new[] { nameof(LongRunningTaskExecutionTimeout) }
            );
        }
    }
}
