using Microsoft.EntityFrameworkCore;

namespace Arc4u.EfCore;

public static class PersisteChangeExtension
{
    public static EntityState Convert(this Arc4u.Data.PersistChange persist)
    {
        switch (persist)
        {
            case Arc4u.Data.PersistChange.Delete:
                return EntityState.Deleted;
            case Arc4u.Data.PersistChange.Update:
                return EntityState.Modified;
            case Arc4u.Data.PersistChange.Insert:
                return EntityState.Added;
        }

        return EntityState.Unchanged;
    }
}
