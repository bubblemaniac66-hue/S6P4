using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace S6P4.Models
{
    // ---------- GET https://digi-api.com/api/v1/digimon?pageSize=20&page=0 ----------
    public class DigimonListResponse
    {
        [JsonProperty("content")]
        public List<DigimonRef> Content { get; set; }

        [JsonProperty("pageable")]
        public Pageable Pageable { get; set; }
    }

    public class DigimonRef
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("href")] public string Href { get; set; }
        [JsonProperty("image")] public string Image { get; set; }
    }

    public class Pageable
    {
        [JsonProperty("currentPage")] public int CurrentPage { get; set; }   // empieza en 0
        [JsonProperty("elementsOnPage")] public int ElementsOnPage { get; set; }
        [JsonProperty("totalElements")] public int TotalElements { get; set; }
        [JsonProperty("totalPages")] public int TotalPages { get; set; }
    }

    // ---------- GET https://digi-api.com/api/v1/digimon/{id o nombre} ----------
    public class DigimonDetail
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("xAntibody")] public bool XAntibody { get; set; }
        [JsonProperty("releaseDate")] public string ReleaseDate { get; set; }
        [JsonProperty("images")] public List<DigimonImage> Images { get; set; }
        [JsonProperty("levels")] public List<LevelItem> Levels { get; set; }
        [JsonProperty("types")] public List<TypeItem> Types { get; set; }
        [JsonProperty("attributes")] public List<AttributeItem> Attributes { get; set; }
        [JsonProperty("fields")] public List<FieldItem> Fields { get; set; }
        [JsonProperty("descriptions")] public List<DescriptionItem> Descriptions { get; set; }
        [JsonProperty("skills")] public List<SkillItem> Skills { get; set; }
        [JsonProperty("priorEvolutions")] public List<EvolutionItem> PriorEvolutions { get; set; }
        [JsonProperty("nextEvolutions")] public List<EvolutionItem> NextEvolutions { get; set; }

        // ----- Propiedades de ayuda para la vista -----
        [JsonIgnore]
        public string ImagenUrl
        {
            get { return Images != null && Images.Any() ? Images[0].Href : ""; }
        }

        [JsonIgnore]
        public string AtributoPrincipal
        {
            get { return Attributes != null && Attributes.Any() ? Attributes[0].Attribute : "Unknown"; }
        }

        [JsonIgnore]
        public string Descripcion
        {
            get
            {
                if (Descriptions == null || !Descriptions.Any()) return null;
                var d = Descriptions.FirstOrDefault(x => x.Language == "en_us") ?? Descriptions.First();
                return d.Description;
            }
        }

        [JsonIgnore]
        public string NumeroFormateado { get { return "#" + Id.ToString("0000"); } }
    }

    public class DigimonImage { [JsonProperty("href")] public string Href { get; set; } [JsonProperty("transparent")] public bool Transparent { get; set; } }
    public class LevelItem { [JsonProperty("id")] public int Id { get; set; } [JsonProperty("level")] public string Level { get; set; } }
    public class TypeItem { [JsonProperty("id")] public int Id { get; set; } [JsonProperty("type")] public string Type { get; set; } }
    public class AttributeItem { [JsonProperty("id")] public int Id { get; set; } [JsonProperty("attribute")] public string Attribute { get; set; } }
    public class FieldItem { [JsonProperty("id")] public int Id { get; set; } [JsonProperty("field")] public string Field { get; set; } [JsonProperty("image")] public string Image { get; set; } }

    public class DescriptionItem
    {
        [JsonProperty("origin")] public string Origin { get; set; }
        [JsonProperty("language")] public string Language { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
    }

    public class SkillItem
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("skill")] public string Skill { get; set; }
        [JsonProperty("translation")] public string Translation { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
    }

    public class EvolutionItem
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("digimon")] public string Digimon { get; set; }
        [JsonProperty("condition")] public string Condition { get; set; }
        [JsonProperty("image")] public string Image { get; set; }
    }

    // ---------- ViewModels ----------
    public class DigimonListaViewModel
    {
        public List<DigimonRef> Digimon { get; set; } = new List<DigimonRef>();
        public string Q { get; set; }
        public string Nivel { get; set; }
        public int Pagina { get; set; }
        public int TotalPaginas { get; set; }
        public int Total { get; set; }
        public string Error { get; set; }
    }

    public class DigimonDetalleViewModel
    {
        public DigimonDetail Digimon { get; set; }
        public string Error { get; set; }
    }

    // ---------- Traducciones y utilidades ----------
    public static class DigimonHelper
    {
        public static readonly string[] Niveles =
            { "Fresh", "In Training", "Rookie", "Champion", "Ultimate", "Mega", "Armor", "Ultra" };

        private static readonly Dictionary<string, string> NivelesEs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"Fresh","Fresco"},{"In Training","En entrenamiento"},{"Rookie","Novato"},{"Champion","Campeón"},
            {"Ultimate","Definitivo"},{"Mega","Mega"},{"Armor","Armadura"},{"Ultra","Ultra"}
        };

        private static readonly Dictionary<string, string> AtributosEs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"Vaccine","Vacuna"},{"Data","Datos"},{"Virus","Virus"},{"Free","Libre"},
            {"Variable","Variable"},{"Unknown","Desconocido"},{"No Data","Sin datos"}
        };

        public static string NivelEs(string nivel)
        {
            string v;
            return nivel != null && NivelesEs.TryGetValue(nivel, out v) ? v : nivel;
        }

        public static string AtributoEs(string atributo)
        {
            string v;
            return atributo != null && AtributosEs.TryGetValue(atributo, out v) ? v : atributo;
        }

        // "No Data" -> "no-data" (para clases CSS)
        public static string Slug(string texto)
        {
            return string.IsNullOrEmpty(texto) ? "unknown" : texto.Trim().ToLowerInvariant().Replace(" ", "-");
        }
    }
}