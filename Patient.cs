using System;
using System.Text.Json.Serialization;

namespace MedicalAppAPI;

public class Patient
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("birthDate")]
    public string BirthDate { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }
    
    [JsonPropertyName("info")]
    public string Info { get; set; }

    [JsonPropertyName("medications")]
    public string Medications { get; set; }

    [JsonPropertyName("procedure")]
    public string Procedure { get; set; }

    // حقول العلامات الحيوية
    [JsonPropertyName("pulse")]
    public string Pulse { get; set; }

    [JsonPropertyName("bp")]
    public string Bp { get; set; }

    [JsonPropertyName("bg")]
    public string Bg { get; set; }

    [JsonPropertyName("o2")]
    public string O2 { get; set; }

    // 🟢 التعديل هنا: أضفنا علامة الاستفهام (?) لتصبح nullable وقابلة للاستقبال بدون إجبار من الفرونتند
    [JsonPropertyName("addedBy")]
    public string? AddedBy { get; set; } = "غير مسجل";
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}