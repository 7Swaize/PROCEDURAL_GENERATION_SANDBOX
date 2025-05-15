#if NEWTONSOFT_JSON
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace VS.Utilities.Files
{
    /// <summary>
    /// Interface defining methods for saving, loading, and clearing data with optional encryption support.
    /// </summary>
    public interface IDataServiceStrategy
    {
        /// <summary>
        /// Saves data to the specified relative path, overriding any existing file. Optionally encrypts the data.
        /// </summary>
        /// <typeparam name="T">The type of the data to save.</typeparam>
        /// <param name="relativePath">The relative path where the data will be saved.</param>
        /// <param name="data">The data to save.</param>
        /// <param name="encrypter">Optional encrypter used to encrypt the data before saving. Pass null for no encryption. Encryption not yet implemented.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// Returns true if the data was saved successfully; otherwise, false.
        /// </returns>
        public Task<bool> SaveDataWithOverrideAsync<T>(string relativePath, T data, [CanBeNull] IEncrypter encrypter);
        
        /// <summary>
        /// Saves data to the specified relative path without overwriting any existing file. Optionally encrypts the data.
        /// If a file already exists, this method merges the new data with the existing data.
        /// </summary>
        /// <typeparam name="T">The type of the data to save.</typeparam>
        /// <param name="relativePath">The relative path where the data will be saved.</param>
        /// <param name="data">The data to save.</param>
        /// <param name="encrypter">Optional encrypter used to encrypt the data before saving. Pass null for no encryption. Encryption not yet implemented.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// Returns true if the data was saved successfully; otherwise, false.
        /// </returns>
        public Task<bool> SaveDataWithoutOverrideAsync<T>(string relativePath, T data, [CanBeNull] IEncrypter encrypter);
        
        /// <summary>
        /// Loads data from the specified relative path. Optionally decrypts the data if an encrypter is provided.
        /// </summary>
        /// <typeparam name="T">The type of the data to load.</typeparam>
        /// <param name="relativePath">The relative path where the data is stored.</param>
        /// <param name="encrypter">Optional encrypter used to decrypt the data after loading. Pass null for no decryption. Encryption not yet implemented.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// Returns the loaded data of type <typeparamref name="T"/>.
        /// </returns>
        public Task<T> LoadDataAsync<T>(string relativePath, [CanBeNull] IEncrypter encrypter);
        
        /// <summary>
        /// Deletes data stored at the specified relative path.
        /// </summary>
        /// <param name="relativePath">The relative path where the data is stored.</param>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist or cannot be deleted.</exception>
        public void ClearData(string relativePath);
    }
    
    public abstract class AbstractDataServiceStrategy : IDataServiceStrategy
    {
        public abstract Task<bool> SaveDataWithoutOverrideAsync<T>(string relativePath, T data, IEncrypter encrypter);
        public abstract Task<T> LoadDataAsync<T>(string relativePath, IEncrypter encrypted);
        
        protected string GetDataPath(string relativePath) => Application.persistentDataPath + relativePath;
        
        public virtual async Task<bool> SaveDataWithOverrideAsync<T>(string relativePath, T data, IEncrypter encrypter)
        {
            string path = GetDataPath(relativePath);

            try
            {
                if (encrypter != null)
                {
                    await using FileStream fileStream = new FileStream(path, FileMode.Create);
                    await encrypter.WriteEncryptedData(data, fileStream);
                }
                else
                {
                    var serializedData = JsonConvert.SerializeObject(data, Formatting.Indented);
                    await File.WriteAllTextAsync(path, serializedData, Encoding.UTF8);
                }
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to save data due to: {e.Message} {e.StackTrace}");
                return false;
            }
        }
        
        public virtual void ClearData(string relativePath)
        {
            string path = Application.persistentDataPath + relativePath;

            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                throw new FileNotFoundException($"Unable to delete data due to: {e.Message} {e.StackTrace}");
            }
        }
        
    }

    public class CollectionsDataServiceStrategy : AbstractDataServiceStrategy
    {
        public override async Task<bool> SaveDataWithoutOverrideAsync<T>(string relativePath, T data, IEncrypter encrypter)
        {
            string path = GetDataPath(relativePath);

            try
            {
                if (File.Exists(path))
                {
                    var fileContent = await File.ReadAllTextAsync(path);
                    JsonSerializerSettings settings = GetSerializerSettings(typeof(T));
                    
                    var existingData = JsonConvert.DeserializeObject<T>(fileContent, settings) ?? data;

                    if (data is IEnumerable<T> newDataList &&
                        existingData is IEnumerable<T> existingDataList)
                    {
                        var mergedList = new List<T>(existingDataList);
                        mergedList.AddRange(newDataList);

                        var serializedContent = JsonConvert.SerializeObject(mergedList, Formatting.Indented);
                        await File.WriteAllTextAsync(path, serializedContent, Encoding.UTF8);
                    }
                }
                else
                {
                    var serializedContent = JsonConvert.SerializeObject(data, Formatting.Indented);
                    await File.WriteAllTextAsync(path, serializedContent,  Encoding.UTF8);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to save data due to: {e.Message} {e.StackTrace}");
                return false;
            }
        }

        public override async Task<T> LoadDataAsync<T>(string relativePath, IEncrypter encrypter)
        {
            string path = GetDataPath(relativePath);
            
            if (!File.Exists(path))
                throw new FileNotFoundException($"File does not exist: {path}");

            try
            {
                var fileContent = await File.ReadAllTextAsync(path);
                JsonSerializerSettings settings = GetSerializerSettings(typeof(T));
                
                var data = JsonConvert.DeserializeObject<T>(fileContent, settings);

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to load data due to: {e.Message} {e.StackTrace}");
                throw;
            }
        }
        
        private JsonSerializerSettings GetSerializerSettings(Type element)
        {
            var elementType = ReflectionUtilities.GetCollectionElementType(element);
            var converter = ConverterFactory.GetConverter(elementType);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = converter != null
                    ? new List<JsonConverter> { converter }
                    : new List<JsonConverter>()
            };
            
            return settings;
        }
        
    }

    public class SingleObjectDataServiceStrategy : AbstractDataServiceStrategy
    {
            
        public override async Task<bool> SaveDataWithoutOverrideAsync<T>(string relativePath, T data, IEncrypter encrypter)
        {
            // If the data you are planning to save will be a saved as a collection in the future,
            // just store it initially as a collection using CollectionsDataServiceStrategy
            
            // For the now the method just overrides all data.
            return await SaveDataWithOverrideAsync(relativePath, data, encrypter);
        }

        public override async Task<T> LoadDataAsync<T>(string relativePath, IEncrypter encrypter)
        {
            string path = GetDataPath(relativePath);
            
            if (!File.Exists(path))
                throw new FileNotFoundException($"File does not exist: {path}");

            try
            {
                var fileContent = await File.ReadAllTextAsync(path);
                JsonSerializerSettings settings = GetSerializerSettings(typeof(T));

                var data = JsonConvert.DeserializeObject<T>(fileContent, settings);

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to load data due to: {e.Message} {e.StackTrace}");
                throw;
            }
        }
        
        private JsonSerializerSettings GetSerializerSettings(Type element)
        {
            var converter = ConverterFactory.GetConverter(element);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = converter != null
                    ? new List<JsonConverter> { converter }
                    : new List<JsonConverter>()
            };
            
            return settings;
        }
    }

    /// <summary>
    /// Factory class for creating instances of <see cref="IDataServiceStrategy"/> implementations.
    /// Determines the appropriate strategy based on the type of data being handled.
    /// </summary>
    public static class DataServiceStrategyFactory
    {
        /// <summary>
        /// Creates an instance of an <see cref="IDataServiceStrategy"/> implementation based on the specified data type.
        /// </summary>
        /// <typeparam name="T">The type of data to be handled by the strategy.</typeparam>
        /// <returns>
        /// An instance of an <see cref="IDataServiceStrategy"/> suitable for handling the specified data type.
        /// </returns>
        public static IDataServiceStrategy GetStrategy<T>()
        {
            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                return new CollectionsDataServiceStrategy();
            }

            return new SingleObjectDataServiceStrategy();
        }
    }
    
    
    // SOURCES:
    
        // Concepts:
            // Data persistence and encryption: https://www.youtube.com/watch?v=mntS45g8OK4
            // Factory pattern: https://www.youtube.com/watch?v=Z1CDJASi4SQ&list=PLnJJ5frTPwRMCCDVE_wFIt3WIj163Q81V&index=2
            // Strategy pattern: https://www.youtube.com/watch?v=QrxiD2dfdG4&list=PLnJJ5frTPwRMCCDVE_wFIt3WIj163Q81V&index=3
            // Asynchronous programming: https://www.youtube.com/watch?v=il9gl8MH17s
                                      // https://www.youtube.com/watch?v=3GhKdDCvtKE&t=917s
            // Nullable types: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types

        // Specifics:
            // typeof and GetType: https://stackoverflow.com/questions/983030/type-checking-typeof-gettype-or-is
            // Getting collection item types: https://stackoverflow.com/questions/4452590/c-sharp-get-the-item-type-for-a-generic-list
            // Appending onto json file: https://stackoverflow.com/questions/20626849/how-to-append-a-json-file-without-disturbing-the-formatting
            // JsonSerializerSettings: https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonSerializerSettings.htm
            
        // Custom Json Converter:
            // General Design: https://stackoverflow.com/questions/8030538/how-to-implement-custom-jsonconverter-in-json-net
            // Dictionary Converter Design: https://code-maze.com/csharp-convert-a-jobject-to-a-dictionary/
            // jObject reader: https://stackoverflow.com/questions/50498795/unexpected-initial-token-endobject-when-populating-object-exception-deserializ
            // jObject.Properties(): https://www.newtonsoft.com/json/help/html/jobjectproperties.htm
    
        // Future:
            // Implement in some form later: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to
            
        // AI wrote the doc comments because it's a lot of work to write
}
#endif