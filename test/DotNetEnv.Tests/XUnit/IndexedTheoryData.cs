using System.Runtime.CompilerServices;
using Xunit;

namespace DotNetEnv.Tests.XUnit;

public class IndexedTheoryData<T> : TheoryData
{
    private int _index = 0;
    public void Add(T p, [CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
    {
        AddRow($"{_index++,4}; {callerMemberName}:{callerLineNumber}", p);
    }
}

public class IndexedTheoryData<T1, T2> : TheoryData
{
    private int _index = 0;
    public void Add(T1 p1, T2 p2, [CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
    {
        AddRow($"{_index++,4}; {callerMemberName}:{callerLineNumber}", p1, p2);
    }
}
