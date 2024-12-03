
namespace Arc4u;

/// <summary>
/// Specifies the denominator to consider while performing an <see cref="M:Interval.UnionOf&lt;T&gt;">UnionOf</see> intervals.
/// </summary>
/// <remarks>
/// While performing an <see cref="M:Interval.UnionOf&lt;T&gt;">UnionOf</see> intervals, depending of the specified <see cref="UnionDenominator"/>, the result will be different.
/// If you consider the following intervals:
/// <list type="table">
/// <item>
/// <term>a</term>
/// <description>]∞ ; 3[</description>
/// </item>
/// <item>
/// <term>b</term>
/// <description>]0 ; 5[</description>
/// </item>
/// <item>
/// <term>c</term>
/// <description>[1 ; 2]</description>
/// </item>
/// </list>
/// The union with the <see cref="UnionDenominator.Highest"/> denominator will return an <see cref="IntervalCollection&lt;T&gt;"/> of 1 element as shown below:
/// <list type="table">
/// <item>
/// <term>i</term>
/// <description>]∞ ; 5[</description>
/// </item>
/// </list>
/// The union with the <see cref="UnionDenominator.Lowest"/> denominator will return an <see cref="IntervalCollection&lt;T&gt;"/> of 5 element as shown below:
/// <list type="table">
/// <item>
/// <term>i</term>
/// <description>]∞ ; 0]</description>
/// </item>
/// <item>
/// <term>j</term>
/// <description>]0 ; 1[</description>
/// </item>
/// <item>
/// <term>k</term>
/// <description>[1 ; 2]</description>
/// </item>
/// <item>
/// <term>l</term>
/// <description>]2 ; 3[</description>
/// </item>
/// <item>
/// <term>m</term>
/// <description>[3 ; 5[</description>
/// </item>
/// </list>
/// </remarks>
public enum UnionDenominator
{
    /// <summary>
    /// The lowest denominator is considered.
    /// </summary>
    Lowest,
    /// <summary>
    /// The highest denominator is considered.
    /// </summary>
    Highest,
}
