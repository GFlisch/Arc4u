using System.Security;
using System.Text;

namespace System.Xml.Serialization;

/// <summary>
/// Provides a set of static methods for easing use of the <see cref="XmlSerializer"/>.
/// </summary>
public static class XmlSerializerExtensions
{
    /// <summary>
    /// Deserializes an XML document contained in the specified string.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="s">The string that contains the XML document to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="serializer"/> is <c>null</c>. -or-
    /// <paramref name="s"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">An error occurred during deserialization. The original exception is available using the <see cref="Exception.InnerException"/> property.</exception>
    public static object? Deserialize(this XmlSerializer serializer, string s)
    {

        if (s == null)
        {
            return null;
        }

        using var xmlReader = XmlReader.Create(new StringReader(s));
        return serializer.Deserialize(xmlReader);
    }

    /// <summary>
    /// Deserializes an XML document contained in the specified string and encoding style.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="s">The string that contains the XML document to deserialize.</param>
    /// <param name="encodingStyle">The encoding style of the serialized XML.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="serializer"/> is <c>null</c>. -or-
    /// <paramref name="s"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">An error occurred during deserialization. The original exception is available using the <see cref="Exception.InnerException"/> property.</exception>
    public static object? Deserialize(this XmlSerializer serializer, string s, string encodingStyle)
    {
        if (s == null)
        {
            return null;
        }

        using var xmlReader = XmlReader.Create(new StringReader(s));
        return serializer.Deserialize(xmlReader, encodingStyle);
    }

    /// <summary>
    /// Deserializes an XML document contained in the specified file path.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="path">A relative or absolute path of the file that contains the XML document to deserialize.</param>
    /// <param name="o">The deserialized object.</param>        
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
    public static void Deserialize(this XmlSerializer serializer, string path, out object? o)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        o = serializer.Deserialize(stream);
    }

    /// <summary>
    /// Serializes the specified object and writes the XML document to a file using the specified path.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="path">A relative or absolute path for the file used to write the XML document.</param>
    /// <param name="o">The object to serialize.</param>
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
    /// <exception cref="InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="Exception.InnerException"/> property.</exception>
    public static void Serialize(this XmlSerializer serializer
        , string path
        , object o)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        serializer.Serialize(stream, o);
    }

    /// <summary>
    /// Serializes the specified object and 
    /// writes the XML document to a file 
    /// using the specified path and 
    /// references the specified namespaces.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="path">A relative or absolute path for the file used to write the XML document.</param>
    /// <param name="o">The object to serialize.</param>
    /// <param name="namespaces">The <see cref="XmlSerializerNamespaces"/> referenced by the object.</param>
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
    /// <exception cref="InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="Exception.InnerException"/> property.</exception>
    public static void Serialize(this XmlSerializer serializer
        , string path
        , object o
        , XmlSerializerNamespaces namespaces)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        serializer.Serialize(stream, o, namespaces);
    }

    /// <summary>
    /// Serializes the specified object and 
    /// writes the XML document to the specified string.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="o">The object to serialize.</param>
    /// <param name="s">The string used to write the XML document.</param>
    /// <exception cref="ArgumentNullException"><paramref name="serializer"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="Exception.InnerException"/> property.</exception>
    public static void Serialize(this XmlSerializer serializer
        , object o
        , out string s)
    {
        var builder = new StringBuilder();
        using (var xmlWriter = XmlWriter.Create(builder))
        {
            serializer.Serialize(xmlWriter, o);
        }
        s = builder.ToString();
    }

    /// <summary>
    /// Serializes the specified object and 
    /// writes the XML document to the specified string and 
    /// references the specified namespaces.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="o">The object to serialize.</param>
    /// <param name="namespaces">The <see cref="XmlSerializerNamespaces"/> referenced by the object.</param>
    /// <param name="s">The string used to write the XML document.</param>
    /// <exception cref="ArgumentNullException"><paramref name="serializer"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="Exception.InnerException"/> property.</exception>       
    public static void Serialize(this XmlSerializer serializer
        , object o
        , XmlSerializerNamespaces namespaces
        , out string s)
    {
        var builder = new StringBuilder();
        using (var xmlWriter = XmlWriter.Create(builder))
        {
            serializer.Serialize(xmlWriter, o, namespaces);
        }
        s = builder.ToString();
    }

    /// <summary>
    /// Serializes the specified object and 
    /// writes the XML document to the specified string and 
    /// references the specified namespaces and encoding style.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="o">The object to serialize.</param>
    /// <param name="namespaces">The <see cref="XmlSerializerNamespaces"/> referenced by the object.</param>
    /// <param name="encodingStyle">The encoding style of the serialized XML.</param>
    /// <param name="s">The string used to write the XML document.</param>
    /// <exception cref="ArgumentNullException"><paramref name="serializer"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="Exception.InnerException"/> property.</exception>       
    public static void Serialize(this XmlSerializer serializer
        , object o
        , XmlSerializerNamespaces namespaces
        , string encodingStyle
        , out string s)
    {
        var builder = new StringBuilder();
        using (var xmlWriter = XmlWriter.Create(builder))
        {
            serializer.Serialize(xmlWriter, o, namespaces, encodingStyle);
        }
        s = builder.ToString();
    }

    /// <summary>
    /// Serializes the specified object and 
    /// writes the XML document to the specified string and 
    /// references the specified namespaces and encoding style.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="o">The object to serialize.</param>
    /// <param name="namespaces">The <see cref="XmlSerializerNamespaces"/> referenced by the object.</param>
    /// <param name="encodingStyle">The encoding style.</param>
    /// <param name="id">For SOAP encoded messages, the base used to generate id attributes.</param>
    /// <param name="s">The string used to write the XML document.</param>
    /// <exception cref="ArgumentNullException"><paramref name="serializer"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="Exception.InnerException"/> property.</exception>
    public static void Serialize(this XmlSerializer serializer
        , object o
        , XmlSerializerNamespaces namespaces
        , string encodingStyle
        , string id
        , out string s)
    {
        var builder = new StringBuilder();
        using (var xmlWriter = XmlWriter.Create(builder))
        {
            serializer.Serialize(xmlWriter, o, namespaces, encodingStyle, id);
        }
        s = builder.ToString();
    }
}
