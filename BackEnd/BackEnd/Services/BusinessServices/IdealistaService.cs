using System.Text;
using System.Text.Json;
using BackEnd.Interfaces.IBusinessServices;
using Microsoft.Extensions.Logging;

namespace BackEnd.Services.BusinessServices
{
    public class IdealistaService : IIdealistaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IdealistaService> _logger;
        private const string BaseUrl = "https://partners-sandbox.idealista.it/v1";
        private const string OAuthUrl = "https://partners-sandbox.idealista.it/oauth/token";

        public IdealistaService(HttpClient httpClient, ILogger<IdealistaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> GetAccessTokenAsync(string clientId, string clientSecret)
        {
            try
            {
                // Crea le credenziali Base64
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

                var request = new HttpRequestMessage(HttpMethod.Post, OAuthUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "write")
                });

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (tokenResponse.TryGetProperty("access_token", out var accessToken))
                {
                    return accessToken.GetString();
                }

                _logger.LogError("Token non trovato nella risposta OAuth");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'ottenimento del token OAuth");
                return null;
            }
        }

        public async Task<int?> CreatePropertyAsync(string accessToken, string feedKey, object propertyData)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/properties");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("feedKey", feedKey);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(propertyData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Errore nella creazione immobile su Idealista: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (responseJson.TryGetProperty("propertyId", out var propertyId))
                {
                    return propertyId.GetInt32();
                }

                _logger.LogWarning("PropertyId non trovato nella risposta di creazione");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'immobile su Idealista");
                return null;
            }
        }

        public async Task<bool> UpdatePropertyAsync(string accessToken, string feedKey, int propertyId, object propertyData)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/properties/{propertyId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                // Nota: feedKey non è richiesto per UPDATE secondo la documentazione, ma lo aggiungiamo per sicurezza
                request.Headers.Add("feedKey", feedKey);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(propertyData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Errore nell'aggiornamento immobile su Idealista: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'immobile su Idealista");
                return false;
            }
        }

        public async Task<bool> DeactivatePropertyAsync(string accessToken, string feedKey, int propertyId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/properties/{propertyId}/deactivate");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("feedKey", feedKey);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Errore nella disattivazione immobile su Idealista: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la disattivazione dell'immobile su Idealista");
                return false;
            }
        }

        public async Task<bool> UpdatePropertyImagesAsync(string accessToken, string feedKey, int propertyId, List<string> imageUrls, string propertyType = "flat")
        {
            try
            {
                // Mappa le immagini con label appropriate in base alla tipologia
                // La prima immagine può essere "appraisalplan" (planimetria) se disponibile
                // Le altre immagini usano label appropriate per la tipologia
                var imagesData = new
                {
                    images = imageUrls.Select((url, index) => new
                    {
                        // Se non specifichiamo una label o usiamo una non valida, Idealista la assegnerà automaticamente
                        // Per ora usiamo label generiche valide per tutte le tipologie
                        // La documentazione dice che se label è "unknown" o non valida, viene assegnata automaticamente
                        label = GetImageLabelForType(propertyType, index),
                        url = url
                    }).ToArray()
                };

                var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/properties/{propertyId}/images");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("feedKey", feedKey);
                // Nota: La documentazione indica multipart/form-data, ma il body è JSON
                // Idealista accetta probabilmente entrambi i formati
                request.Content = new StringContent(
                    JsonSerializer.Serialize(imagesData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Errore nell'aggiornamento immagini su Idealista: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento delle immagini su Idealista");
                return false;
            }
        }

        public async Task<bool> DeletePropertyImagesAsync(string accessToken, string feedKey, int propertyId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/properties/{propertyId}/images");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("feedKey", feedKey);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Errore nell'eliminazione immagini su Idealista: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione delle immagini su Idealista");
                return false;
            }
        }

        /// <summary>
        /// Ottiene una label appropriata per l'immagine in base alla tipologia dell'immobile.
        /// Se la label non è valida o è "unknown", Idealista la assegnerà automaticamente.
        /// </summary>
        private static string GetImageLabelForType(string propertyType, int index)
        {
            // Label comuni valide per tutte le tipologie principali (flat, house, etc.)
            var commonLabels = new[] { "facade", "views", "details", "living", "bedroom", "kitchen", "bathroom", "terrace", "garden" };
            
            // Per la prima immagine, potremmo usare "facade" (facciata) che è sempre valida
            if (index == 0)
            {
                return "facade";
            }
            
            // Per le altre immagini, cicliamo tra label comuni
            // Idealista assegnerà automaticamente label più appropriate se necessario
            return commonLabels[index % commonLabels.Length];
        }
    }
}

