using System;

namespace Arc4u.Windows.Navigation
{
    public class DuplcateRegionException : Exception
    {
        public DuplcateRegionException(string region) : base($"A region with name: {region} is already registered.")
        {
        }
    }
}
