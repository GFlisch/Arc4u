namespace Arc4u.Security.Cryptography
{
    /// <summary>
    ///     Provides methods for decrypting application secrets like passwords.
    /// </summary>
    public interface IDecryptionProvider
    {
        /// <summary>
        ///     Decrypts given cypher text to plain text.
        /// </summary>
        public string Decrypt(string cypher);
    }
}