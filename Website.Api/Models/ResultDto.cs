namespace Website.Models
{
    /// <summary>
    /// Represents the result of an operation that returns data.
    /// Contains the success state, an optional message, and the resulting data when the operation succeeds.
    /// </summary>
    public class ResultDto<T>
    {
        private ResultDto(bool isSucceeded, List<string>? messages, T? data)
        {
            Messages = messages;
            Data = data;
            IsSucceeded = isSucceeded;
        }

        public bool IsSucceeded { get; set; }
        public bool IsFailure => !IsSucceeded;

        public List<string>? Messages { get; set; }
        public T? Data { get; set; }


        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static ResultDto<T> Success(T? data, List<string>? messages = null) => 
            new ResultDto<T>(true, messages, data);

        /// <summary>
        /// Creates a failed result with an optional error message.
        /// </summary>
        public static ResultDto<T> Failure(T? data, List<string>? messages = null) =>
            new ResultDto<T>(false, messages, data);
    }


    /// <summary>
    /// Represents the result of an operation that does not return data.
    /// Indicates whether the operation succeeded or failed, along with an optional message.
    /// Typically used for validation results or commands that do not produce a value.
    /// </summary>
    public class ResultDto
    {
        private ResultDto(bool isSucceeded, List<string>? messages)
        {
            Messages = messages;
            IsSucceeded = isSucceeded;
        }

        public bool IsSucceeded { get; set; }
        public bool IsFailure => !IsSucceeded;

        public List<string>? Messages { get; set; }


        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static ResultDto Success(List<string>? messages) =>
            new ResultDto(true, messages);

        /// <summary>
        /// Creates a failed result with an optional error message.
        /// </summary>
        public static ResultDto Failure(List<string>? messages) =>
            new ResultDto(false, messages);
    }
}
