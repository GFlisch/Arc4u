using System.Security.Cryptography;
using System.Text;

namespace Arc4u.Security.Cryptography;

public static class HashAlgorithmExtensions
{
    public static string ComputeHash(this HashAlgorithm algorithm, string input, Encoding encoding)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(algorithm);
        ArgumentNullException.ThrowIfNull(encoding);
#else
        //consider arguments
        if (input == null)
        {
            throw new ArgumentNullException("input");
        }

        if (encoding == null)
        {
            throw new ArgumentNullException("encoding");
        }
#endif
        //convert the string into an array of bytes.
        byte[] messageBytes = encoding.GetBytes(input);

        //create the hash value from the array of bytes.
        byte[] hashValue = algorithm.ComputeHash(messageBytes);

        //convert the hash value to string
        return BitConverter.ToString(hashValue);
    }
}
