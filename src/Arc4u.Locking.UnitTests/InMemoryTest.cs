using Arc4u.Locking.Abstraction;

namespace Arc4u.Locking.UnitTests;

public class InMemoryLockingTest : LockingTest
{
    protected override ILockingDataLayer BuildDataLayer()
    {
        return new MemoryLockingDataLayer();
    }
}