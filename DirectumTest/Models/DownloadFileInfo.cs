using System.Text.Json.Serialization;

namespace DirectumTest.Models
{
    public class DownloadFileInfo
    {
        [JsonPropertyName("VersionId")]
        public int VersionId { get; set; }
        [JsonPropertyName("TextVersion")]
        public string TextVersion { get; set; }
        [JsonPropertyName("FiasCompleteDbfUrl")]
        public string FiasCompleteDbfUrl { get; set; }
        [JsonPropertyName("FiasCompleteXmlUrl")]
        public string FiasCompleteXmlUrl { get; set; }
        [JsonPropertyName("FiasDeltaDbfUrl")]
        public string FiasDeltaDbfUrl { get; set; }
        [JsonPropertyName("FiasDeltaXmlUrl")]
        public string FiasDeltaXmlUrl { get; set; }
        [JsonPropertyName("Kladr4ArjUrl")]
        public string Kladr4ArjUrl { get; set; }
        [JsonPropertyName("Kladr47ZUrl")]
        public string Kladr47ZUrl { get; set; }
        [JsonPropertyName("GarXMLFullURL")]
        public string GarXMLFullUrl { get; set; }
        [JsonPropertyName("GarXMLDeltaURL")]
        public string GarXMLDeltaUrl { get; set; }
        [JsonPropertyName("ExpDate")]
        public DateTime ExpDate { get; set; }
        [JsonPropertyName("Date")]
        public string Date { get; set; }
    }
}
