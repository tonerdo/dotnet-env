using System.Runtime.CompilerServices;
using Xunit;

namespace DotNetEnv.Tests.XUnit;

public class IndexedTheoryData<TData> : TheoryData
{
    private int _index = 0;
    public void Add(TData p1, [CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
    {
        AddRow($"{_index++,4}; {callerMemberName}:{callerLineNumber}", p1);
    }
}
