#if NEWTONSOFT_JSON
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace VS.Utilities.Files
{
    internal abstract class JsonCreationConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);
        public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            
            JObject jObject = JObject.Load(reader);
            T target = Create(objectType, jObject);
            
            serializer.Populate(jObject.CreateReader(), target);
            
            return target;
            
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value);
    }
    
    internal sealed class ScriptableObjectConverter : JsonCreationConverter<ScriptableObject>
    {
        protected override ScriptableObject Create(Type objectType, JObject jObject)
        {
            return ScriptableObject.CreateInstance(objectType);
        }
    }

    internal sealed class DictionaryConverter<TKey, TValue> : JsonCreationConverter<Dictionary<TKey, TValue>>
    {
        protected override Dictionary<TKey, TValue> Create(Type objectType, JObject jObject)
        {
            return new Dictionary<TKey, TValue>();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            
            JObject jObject = JObject.Load(reader);
            Dictionary<TKey, TValue> target = Create(objectType, jObject);

            foreach (JProperty property in jObject.Properties())
            {
                TKey key = serializer.Deserialize<TKey>(new JTokenReader(property.Name));

                var customConverter = ConverterFactory.GetConverter(typeof(TValue));
                
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Converters = customConverter != null
                        ? new List<JsonConverter> { customConverter }
                        : new List<JsonConverter>()
                };
                
                JsonSerializer tempSerializer = JsonSerializer.Create(settings);
                TValue value = tempSerializer.Deserialize<TValue>(property.Value.CreateReader());
                
                target[key] = value;
            }
            
            return target;
        }
    }
    
    internal static class ConverterFactory
    {
        public static JsonConverter GetConverter(Type elementType)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(elementType))
            {
                return new ScriptableObjectConverter();
            }
            
            if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var genericArgs = elementType.GetGenericArguments();
                var converterType = typeof(DictionaryConverter<,>).MakeGenericType(genericArgs);
                return (JsonConverter)Activator.CreateInstance(converterType);
            }

            return null;
        }
    }
}
#endif