using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoneyTracker.Domain.Integration;

namespace MoneyTracker.Application.Integration;

/// <summary>
/// Manda los gastos de mascota a PetRecording (misma máquina: en producción
/// http://localhost:5003, en local http://localhost:5150).
///
/// Nunca lanza. Si PetRecording está caída, responde 4xx o tarda, el gasto de
/// MoneyTracker ya está guardado y aquí solo queda el aviso en el log: apuntar
/// un gasto no puede depender de que la otra app esté viva.
/// </summary>
public class PetRecordingClient : IPetRecordingClient
{
    /// <summary>Cabecera que valida PetRecording; su valor es Sync:ApiKey.</summary>
    private const string ApiKeyHeader = "X-Sync-Key";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Sin esto, un "+" o una tilde de la descripción viajan escapados en
        // notación unicode. Llega bien igual, pero es ilegible en los logs.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly HttpClient _http;
    private readonly ILogger<PetRecordingClient> _logger;
    private readonly string? _baseUrl;
    private readonly string? _apiKey;

    public PetRecordingClient(HttpClient http, IConfiguration configuration, ILogger<PetRecordingClient> logger)
    {
        _http = http;
        _logger = logger;
        _baseUrl = configuration["PetRecording:BaseUrl"];
        _apiKey = configuration["Sync:ApiKey"];
    }

    public async Task UpsertExpenseAsync(PetExpenseSync expense, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured("mandar", expense.MtExpenseId)) return;

        try
        {
            using var request = BuildRequest(HttpMethod.Post, "/api/sync/expenses");
            request.Content = JsonContent.Create(expense, options: JsonOptions);

            using var response = await _http.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Gasto {ExpenseId} enviado a PetRecording (mascota {PetId})", expense.MtExpenseId, expense.PetId);
                return;
            }

            // 400 = los importes no cuadran o faltan datos · 404 = esa mascota no existe allí
            // · 401 = la clave no coincide. Los tres son cosa nuestra, no un corte de red.
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "PetRecording rechazó el gasto {ExpenseId}: {Status} {Body}",
                expense.MtExpenseId, (int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el gasto {ExpenseId} a PetRecording", expense.MtExpenseId);
        }
    }

    public async Task DeleteExpenseAsync(int mtExpenseId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured("borrar", mtExpenseId)) return;

        try
        {
            // Allí siempre devuelve 204, aunque el gasto no existiera.
            using var request = BuildRequest(HttpMethod.Delete, $"/api/sync/expenses/{mtExpenseId}");
            using var response = await _http.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                _logger.LogError(
                    "PetRecording no pudo borrar el gasto {ExpenseId}: {Status}",
                    mtExpenseId, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo borrar el gasto {ExpenseId} en PetRecording", mtExpenseId);
        }
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, new Uri(new Uri(_baseUrl!), path));
        request.Headers.Add(ApiKeyHeader, _apiKey);
        return request;
    }

    /// <summary>
    /// Sin URL o sin clave no se llama a nadie. Pasa en un entorno donde no se ha
    /// puesto la configuración, y no es motivo para ensuciar el log con errores.
    /// </summary>
    private bool IsConfigured(string action, int expenseId)
    {
        if (!string.IsNullOrWhiteSpace(_baseUrl) && !string.IsNullOrWhiteSpace(_apiKey))
            return true;

        _logger.LogWarning(
            "PetRecording sin configurar (PetRecording:BaseUrl / Sync:ApiKey): no se va a {Action} el gasto {ExpenseId}",
            action, expenseId);
        return false;
    }
}
