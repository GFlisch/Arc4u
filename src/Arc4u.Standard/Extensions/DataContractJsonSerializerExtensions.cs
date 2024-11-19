using System.Text;

namespace System.Runtime.Serialization.Json
{
    /// <summary>
    /// Provides a set of static methods for easing use of the <see cref="DataContractSerializer"/>.
    /// </summary>
    public static class DataContractJsonSerializerExtensions
    {
        /// <summary>
        /// Reads the Json string and returns the deserialized object.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="content">The string used to read the Json document.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serializer"/> is <c>null</c>. -or-
        /// <paramref name="content"/> is <c>null</c>.
        /// </exception>
        public static T ReadObject<T>(this DataContractJsonSerializer serializer, string content)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            if (content == null)
            {
                return default(T);
            }

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return (T)serializer.ReadObject(stream);
        }

        /// <summary>
        /// Reads the Json contained in the specified file path and return the deserialized object via the <c>out</c> parameter.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="stream">A stream that contains the Json document to deserialize.</param>
        /// <param name="graph">The deserialized object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serializer"/> is <c>null</c>. -or-
        /// <paramref name="path"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters.</exception>
        /// <exception cref="NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="FileNotFoundException">The file specified by <paramref name="path"/> does not exist.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="PathTooLongException">The specified <paramref name="path"/>, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        public static void ReadObject<T>(this DataContractJsonSerializer serializer, Stream stream, out T graph)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            graph = (T)serializer.ReadObject(stream);
        }

        /// <summary>
        /// Reads the XML contained in the specified file path and return the deserialized object via the <c>out</c> parameter.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="path">A relative or absolute path of the file that contains the XML document to deserialize.</param>
        /// <param name="graph">The deserialized object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serializer"/> is <c>null</c>. -or-
        /// <paramref name="path"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters.</exception>
        /// <exception cref="NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="FileNotFoundException">The file specified by <paramref name="path"/> does not exist.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="PathTooLongException">The specified <paramref name="path"/>, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        public static void ReadObject<T>(this DataContractJsonSerializer serializer, string fileName, out T graph)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            graph = serializer.ReadObject<T>(File.ReadAllText(fileName));

        }

        /// <summary>
        /// Writes the complete content of the object to a string.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="graph">The object that contains the data to write.</param>
        /// <param name="content">The string used to write the Json document.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serializer"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidDataContractException">the type being serialized does not conform to data contract rules. For example, the <see cref="DataContractAttribute"/> attribute has not been applied to the type.</exception>
        /// <exception cref="SerializationException">there is a problem with the instance being serialized.</exception>
        /// <exception cref="QuotaExceededException">the maximum number of objects to serialize has been exceeded. Check the <see cref="DataContractSerializer.MaxItemsInObjectGraph"/> property.</exception>        
        public static void WriteObject(this DataContractJsonSerializer serializer, object graph, out string content)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            using var stream = new MemoryStream();
            serializer.WriteObject(stream, graph);

            var blob = stream.ToArray();
            content = Encoding.UTF8.GetString(blob, 0, blob.Length);
        }

        /// <summary>
        /// Writes the complete content of the object to a file using the specified path.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="path">A relative or absolute path for the file used to write the XML document.</param>
        /// <param name="graph">The object that contains the data to write.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serializer"/> is <c>null</c>. -or-
        /// <paramref name="path"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters.</exception>
        /// <exception cref="NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="FileNotFoundException">The file specified by <paramref name="path"/> does not exist.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="UnauthorizedAccessException">The access requested is not permitted by the operating system for the specified <paramref name="path"/>. The file or directory is set for read-only access.</exception>
        /// <exception cref="PathTooLongException">The specified <paramref name="path"/>, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="InvalidDataContractException">the type being serialized does not conform to data contract rules. For example, the <see cref="DataContractAttribute"/> attribute has not been applied to the type.</exception>
        /// <exception cref="SerializationException">there is a problem with the instance being serialized.</exception>
        /// <exception cref="QuotaExceededException">the maximum number of objects to serialize has been exceeded. Check the <see cref="DataContractSerializer.MaxItemsInObjectGraph"/> property.</exception>
        public static void WriteObject(this DataContractJsonSerializer serializer, string path, object graph)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            serializer.WriteObject(stream, graph);
        }
    }
}

