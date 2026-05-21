using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MfaMethod
{
    Totp = 0,
    Sms = 1,
}