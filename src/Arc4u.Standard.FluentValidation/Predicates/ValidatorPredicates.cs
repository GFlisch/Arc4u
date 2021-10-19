using Arc4u.Data;

namespace Arc4u.FluentValidation
{
    public static class ValidatorPredicates
    {
        public static bool IsUpdate<TElement>(TElement element) where TElement : IPersistEntity
        {
            return element.PersistChange.Equals(PersistChange.Update);
        }
        public static bool IsInsert<TElement>(TElement element) where TElement : IPersistEntity
        {
            return element.PersistChange.Equals(PersistChange.Insert);
        }

        public static bool IsDelete<TElement>(TElement element) where TElement : IPersistEntity
        {
            return element.PersistChange.Equals(PersistChange.Delete);
        }

        public static bool IsNonet<TElement>(TElement element) where TElement : IPersistEntity
        {
            return element.PersistChange.Equals(PersistChange.None);
        }
    }
}
