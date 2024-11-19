namespace Arc4u.Security.Principal
{
    /// <summary>
    /// The class extend the security identifier to generate a custom sid based on a Guid.
    /// It is useful if you need to use Azman with a custom sid.
    /// </summary>
    public static class SecurityIdentifierExt
    {
        /// <summary>
        /// Generate a S-1-9 SecurityIdentifier based on a random <see cref="Guid"/>.
        /// </summary>
        /// <returns>A S-1-9 <see cref="SecurityIdentifier"/></returns>
        public static string S19()
        {
            return S19(Guid.NewGuid());
        }

        public static string ToS19(this Guid guid)
        {
            return S19(guid);
        }

        /// <summary>
        /// Generate a S-1-9 string SecurityIdentifier based on a given <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">A specific <see cref="Guid"/> generating a constant <see cref="SecurityIdentifier"/></param>
        /// <returns>A S-1-9 <see cref="SecurityIdentifier"/></returns>
        public static string S19(Guid guid)
        {
            byte[] guidArray = guid.ToByteArray();
            var SidArray = new byte[16];

            Array.Copy(guidArray, 0, SidArray, 0, 16);

            return $"S-1-9-{BitConverter.ToUInt32(SidArray, 0)}-{BitConverter.ToUInt32(SidArray, 4)}-{BitConverter.ToUInt32(SidArray, 8)}-{BitConverter.ToUInt32(SidArray, 12)}";
        }

    }
}
