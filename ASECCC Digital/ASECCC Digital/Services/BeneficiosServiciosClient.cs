using System.Net.Http.Json;
using ASECCC_Digital.Models;

namespace ASECCC_Digital.Services
{
    public interface IBeneficiosServiciosClient
    {
        Task<List<BeneficioServicioModel>> GetAllAsync(string? categoria = null);
        Task<BeneficioServicioModel?> GetByIdAsync(int id);
    }

    public class BeneficiosServiciosClient : IBeneficiosServiciosClient
    {
        private readonly HttpClient _http;

        public BeneficiosServiciosClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BeneficioServicioModel>> GetAllAsync(string? categoria = null)
        {
            var url = "api/BeneficiosServicios";
            if (!string.IsNullOrWhiteSpace(categoria))
                url += $"?categoria={Uri.EscapeDataString(categoria)}";

            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode) return [];

            return await res.Content.ReadFromJsonAsync<List<BeneficioServicioModel>>() ?? [];
        }

        public async Task<BeneficioServicioModel?> GetByIdAsync(int id)
        {
            var res = await _http.GetAsync($"api/BeneficiosServicios/{id}");
            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<BeneficioServicioModel>();
        }
    }
}
