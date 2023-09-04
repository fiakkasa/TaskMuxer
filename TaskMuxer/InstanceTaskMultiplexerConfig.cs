using System.ComponentModel.DataAnnotations;

namespace TaskMuxer;

public record InstanceTaskMultiplexerConfig : IValidatableObject
{
    public TimeSpan PreserveExecutionResultDuration { get; set; } = TimeSpan.Zero;

    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PreserveExecutionResultDuration < TimeSpan.Zero)
            yield return new($"{nameof(PreserveExecutionResultDuration)} must be equal or greater than {TimeSpan.Zero}", new[] { nameof(ExecutionTimeout) });

        if (ExecutionTimeout <= TimeSpan.Zero)
            yield return new($"{nameof(ExecutionTimeout)} must be greater than {TimeSpan.Zero}", new[] { nameof(ExecutionTimeout) });
    }
}
