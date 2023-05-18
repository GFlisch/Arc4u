using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Security
{
    public class OperationRequirement : IAuthorizationRequirement
    {
        public OperationRequirement(params int[] operations)
        {
            _operations = operations;
        }

        private readonly int[] _operations;

        public int[] Operations => _operations;
    }
}
