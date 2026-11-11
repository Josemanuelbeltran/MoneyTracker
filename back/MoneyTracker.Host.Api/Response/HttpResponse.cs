namespace MoneyTracker.Host.Api.Response
{
    // Clase genérica para dar formato estándar a las respuestas de la API
    public class HttpResponse<T>
    {
        // Indica si la operación fue exitosa
        public bool Success { get; set; }

        // Datos que se devuelven cuando todo va bien
        public T? Data { get; set; }

        // Código de estado HTTP cuando ocurre un error
        public int? StatusCode { get; set; } // Solo para errores

        // Mensaje de error explicativo
        public string? ErrorMessage { get; set; }

        // Método estático para generar una respuesta exitosa
        public static HttpResponse<T> Ok(T data) => new()
        {
            Success = true,
            Data = data
        };

        // Método estático para generar una respuesta de error
        public static HttpResponse<T> Fail(string errorMessage, int statusCode) => new()
        {
            Success = false,
            StatusCode = statusCode,
            ErrorMessage = errorMessage
        };
    }
}