namespace LightQuery.Client
{
    internal class ResponseDeserializationResult<T>
    {
        public T DeserializedValue { get; set; }
        public int NewPageSuggestion { get; set; }
    }
}
