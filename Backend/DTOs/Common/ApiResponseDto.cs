namespace Backend.DTOs.Common;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = [];

    public static ApiResponseDto<T> Ok(T data, string message = "Thao tác thành công")
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }
}

public class ApiResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Errors { get; set; } = [];

    public static ApiResponseDto Ok(string message = "Thao tác thành công")
    {
        return new ApiResponseDto
        {
            Success = true,
            Message = message
        };
    }
}
