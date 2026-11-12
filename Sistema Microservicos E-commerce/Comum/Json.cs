using System.Web.Script.Serialization;

namespace ArteSuave.Comum
{
    /// <summary>Serialização JSON usando o serializador nativo do .NET Framework (sem NuGet).</summary>
    public static class Json
    {
        public static string Serializar(object valor)
        {
            return new JavaScriptSerializer().Serialize(valor);
        }

        public static T Desserializar<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            return new JavaScriptSerializer().Deserialize<T>(json);
        }
    }
}
