using System.ComponentModel.DataAnnotations;

namespace TaskMuxer;

public record InstanceTaskMultiplexerConfig : IValidatableObject
{
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExecutionTimeout <= TimeSpan.Zero)
            yield return new($"{nameof(ExecutionTimeout)} must be greater than {TimeSpan.Zero}", new[] { nameof(ExecutionTimeout) });
    }
}
