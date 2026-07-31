using System.Net;

namespace Citas.Psicologicas.Helpers;

/// <summary>Wrapper genérico para respuestas de la API</summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}

/// <summary>Factory de ApiResponse</summary>
public static class ApiResponseHelper
{
    public static ApiResponse<T> Ok<T>(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        StatusCode = HttpStatusCode.OK
    };

    public static ApiResponse<T> Fail<T>(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new()
    {
        Success = false,
        Data = default,
        Message = message,
        StatusCode = statusCode
    };
}
