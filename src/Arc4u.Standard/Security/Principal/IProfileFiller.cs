using System.Security.Principal;

namespace Arc4u.Security.Principal
{
    /// <summary>
    /// The inerface define the method use to fill a user profile based on the
    /// three parameters (WindowsIdentity, Sid and user name).
    /// </summary>
    public interface IProfileFiller
    {
        /// <summary>
        /// Gets the profile.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        UserProfile GetProfile(IIdentity identity);
    }
}
