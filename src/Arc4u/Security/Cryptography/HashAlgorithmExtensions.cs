using System.Security.Cryptography;
using System.Text;

namespace Arc4u.Security.Cryptography;

public static class HashAlgorithmExtensions
{
    public static string ComputeHash(this HashAlgorithm algorithm, string input, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(algorithm);
        ArgumentNullException.ThrowIfNull(encoding);

        //convert the string into an array of bytes.
        var messageBytes = encoding.GetBytes(input);

        //create the hash value from the array of bytes.
        var hashValue = algorithm.ComputeHash(messageBytes);

        //convert the hash value to string
        return BitConverter.ToString(hashValue);
    }
}
