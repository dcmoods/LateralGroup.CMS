namespace LateralGroup.Application.Exceptions
{
    public sealed class ValidationException : Exception
    {
        public IReadOnlyCollection<string> Errors { get; }

        public ValidationException(IEnumerable<string> errors)
            : base("One or more validation failures occurred.")
        {
            Errors = errors?.ToArray() ?? Array.Empty<string>();
        }

        public ValidationException(string error)
            : this(new[] { error })
        {
        }
    }
}
