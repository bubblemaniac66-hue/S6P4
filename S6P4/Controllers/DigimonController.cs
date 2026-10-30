using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using S6P4.Models;

namespace S6P4.Controllers
{
    public class DigimonController : Controller
    {
        private const int PorPagina = 20;

        // HttpClient único y reutilizable (buena práctica: no crear uno por petición)
        private static readonly HttpClient _client = CrearCliente();

        private static HttpClient CrearCliente()
        {
            // Digi-API usa HTTPS: .NET Framework necesita TLS 1.2
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var cliente = new HttpClient
            {
                BaseAddress = new Uri("https://digi-api.com/api/v1/"),
                Timeout = TimeSpan.FromSeconds(30)
            };

            cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            cliente.DefaultRequestHeaders.UserAgent.ParseAdd("S6P4/1.0");

            // Digi-API es pública y NO requiere token.
            // Con otra API que sí lo pida, aquí agregarías:
            // cliente.DefaultRequestHeaders.Authorization =
            //     new AuthenticationHeaderValue("Bearer", "TU_TOKEN_AQUI");

            return cliente;
        }

        // GET: /Digimon?q=agumon&nivel=Rookie&pagina=1
        public async Task<ActionResult> Index(string q = null, string nivel = null, int pagina = 1)
        {
            if (pagina < 1) pagina = 1;
            var vm = new DigimonListaViewModel { Q = q, Nivel = nivel, Pagina = pagina };

            try
            {
                // La API empieza a contar las páginas desde 0
                string url = "digimon?pageSize=" + PorPagina + "&page=" + (pagina - 1);
                if (!string.IsNullOrWhiteSpace(q)) url += "&name=" + Uri.EscapeDataString(q.Trim());
                if (!string.IsNullOrWhiteSpace(nivel)) url += "&level=" + Uri.EscapeDataString(nivel);

                var resp = await _client.GetAsync(url);

                if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    vm.TotalPaginas = 1;   // sin resultados para ese filtro
                    return View(vm);
                }

                resp.EnsureSuccessStatusCode();

                string json = await resp.Content.ReadAsStringAsync();
                var lista = JsonConvert.DeserializeObject<DigimonListResponse>(json);

                vm.Digimon = lista.Content ?? new List<DigimonRef>();
                vm.Total = lista.Pageable != null ? lista.Pageable.TotalElements : vm.Digimon.Count;
                vm.TotalPaginas = lista.Pageable != null && lista.Pageable.TotalPages > 0 ? lista.Pageable.TotalPages : 1;
            }
            catch (HttpRequestException ex)
            {
                vm.Error = "No se pudo conectar con Digi-API: " + ex.Message;
            }
            catch (TaskCanceledException)
            {
                vm.Error = "La solicitud tardó demasiado (timeout). Intenta de nuevo.";
            }
            catch (JsonException ex)
            {
                vm.Error = "Error al interpretar el JSON recibido: " + ex.Message;
            }

            return View(vm);
        }

        // GET: /Digimon/Detalle/289   o   /Digimon/Detalle/agumon
        public async Task<ActionResult> Detalle(string id)
        {
            var vm = new DigimonDetalleViewModel();

            try
            {
                var resp = await _client.GetAsync("digimon/" + Uri.EscapeDataString(id ?? ""));

                if (!resp.IsSuccessStatusCode)
                {
                    vm.Error = "No se encontró ningún Digimon con el nombre o número \"" + id + "\".";
                    return View(vm);
                }

                string json = await resp.Content.ReadAsStringAsync();
                vm.Digimon = JsonConvert.DeserializeObject<DigimonDetail>(json);
            }
            catch (Exception ex)
            {
                vm.Error = "Ocurrió un error al consultar la API: " + ex.Message;
            }

            return View(vm);
        }
    }
}