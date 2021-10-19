namespace Arc4u.Security.Principal
{
    public interface IApplicationContext
    {
        AppPrincipal Principal { get; }

        void SetPrincipal(AppPrincipal principal);
    }
}
