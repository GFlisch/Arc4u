using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Standard.Security
{
    /// <summary>
    /// Represents a requirement for certain operations to be authorized.
    /// </summary>
    public class OperationRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationRequirement"/> class with the specified operations.
        /// </summary>
        /// <param name="operations">The operations that are required for authorization.</param>
        public OperationRequirement(params int[] operations)
        {
            _operations = operations;
        }

        private readonly int[] _operations;

        /// <summary>
        /// Gets the operations that are required for authorization.
        /// </summary>
        /// <value>
        /// The operations that are required for authorization.
        /// </value>
        public int[] Operations => _operations;
    }
}
