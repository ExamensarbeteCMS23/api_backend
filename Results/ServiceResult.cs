namespace api_backend.Results
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public static ServiceResult Ok(string? message = null) => new()
        {
            Success = true,
            Message = message,
        };
        public static ServiceResult Fail(string message, IEnumerable<string>? errors = null) =>
            new() { Success = false, Message = message, Errors = errors };
    }
    public class ServiceResult<T> : ServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string? message = null) => new()
        {
            Success = true,
            Message = message,
            Data = data
        };
        public static new ServiceResult<T> Fail(string message, IEnumerable<string>? errors = null) => new() { Success = false, Message = message, Errors = errors };
    }
}
