using System.Diagnostics.CodeAnalysis;
using Arc4u.Data;

namespace Arc4u.UnitTest.Database.EfCore.Model
{
    public class Contract : IPersistEntity, IComparer<Contract>, IEqualityComparer<Contract>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Reference { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        /// <summary>
        /// We do not compare an object on its PersistChange.
        /// </summary>
        public PersistChange PersistChange { get; set; }

        public int Compare(Contract x, Contract y)
        {
            int[] comparisons = new int[5];

            comparisons[0] = y.Id.CompareTo(x.Id);
            comparisons[1] = string.Compare(y.Name, x.Name);
            comparisons[2] = string.Compare(y.Reference, x.Reference);
            comparisons[3] = DateTime.Compare(y.StartDate, x.StartDate);
            comparisons[4] = DateTime.Compare(y.EndDate, x.EndDate);

            if (comparisons.All(x => x == 0))
            {
                return 0;
            }

            var positives = comparisons.Where(x => x > 0).Count();

            if (positives >= 1)
            {
                return 1;
            }

            return -1;

        }

        public bool Equals(Contract x, Contract y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode([DisallowNull] Contract obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
