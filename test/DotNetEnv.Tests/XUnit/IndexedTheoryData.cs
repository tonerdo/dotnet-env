using System.Runtime.CompilerServices;
using Xunit;

namespace DotNetEnv.Tests.XUnit;

public class IndexedTheoryData<T> : TheoryData<string, T>
{
    private int _index = 0;
    public void Add(T p, [CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
    {
        AddRow($"{_index++,4}; {callerMemberName}:{callerLineNumber}", p);
    }
}

public class IndexedTheoryData<T1, T2> : TheoryData<string, T1, T2>
{
    private int _index = 0;
    public void Add(T1 p1, T2 p2, [CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
    {
        AddRow($"{_index++,4}; {callerMemberName}:{callerLineNumber}", p1, p2);
    }
}

public class IndexedTheoryData<T1, T2, T3> : TheoryData<string, T1, T2, T3>
{
    private int _index = 0;
    public void Add(T1 p1, T2 p2, T3 p3, [CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
    {
        AddRow($"{_index++,4}; {callerMemberName}:{callerLineNumber}", p1, p2, p3);
    }
}
