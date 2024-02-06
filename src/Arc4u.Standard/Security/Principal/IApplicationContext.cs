namespace Arc4u.Security.Principal
{
    public interface IApplicationContext
    {
        AppPrincipal Principal { get; }

        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        /// <value>The activity ID.</value>
        string ActivityID { get; set; }

        void SetPrincipal(AppPrincipal principal);
    }
}
