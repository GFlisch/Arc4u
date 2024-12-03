using System.Diagnostics.CodeAnalysis;
using Arc4u.Data;

namespace Arc4u.UnitTest.Database.EfCore.Model;

public class Contract : IPersistEntity, IComparer<Contract>, IEqualityComparer<Contract>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    /// <summary>
    /// We do not compare an object on its PersistChange.
    /// </summary>
    public PersistChange PersistChange { get; set; }

    public int Compare(Contract? x, Contract? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        int[] comparisons =
        [
            y.Id.CompareTo(x.Id),
            string.Compare(y.Name, x.Name),
            string.Compare(y.Reference, x.Reference),
            DateTime.Compare(y.StartDate, x.StartDate),
            DateTime.Compare(y.EndDate, x.EndDate),
        ];
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

    public bool Equals(Contract? x, Contract? y)
    {
        return Compare(x, y) == 0;
    }

    public int GetHashCode([DisallowNull] Contract obj)
    {
        return obj.Id.GetHashCode();
    }
}
