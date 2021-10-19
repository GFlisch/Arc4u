using System;

namespace Arc4u.MongoDB.Exceptions
{
    public class TypeMappedToMoreThanOneCollectionException<TEntity> : Exception
    {
        public TypeMappedToMoreThanOneCollectionException(int times) : base($"{times} registration exist for {typeof(TEntity).FullName}.")
        {

        }
    }
}
