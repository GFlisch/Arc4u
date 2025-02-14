using Arc4u.Caching;

namespace Arc4u.OAuth2.Token;
public interface ICacheHelper
{
    ICache GetCache();
}