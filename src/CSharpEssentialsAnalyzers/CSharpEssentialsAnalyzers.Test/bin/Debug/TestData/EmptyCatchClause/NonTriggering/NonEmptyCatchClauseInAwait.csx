using System.Threading.Tasks;

namespace CSharpEssentialsAnalyzers.Test.TestData.EmptyCatchClause.NonTriggering
{
    using System.Diagnostics;

    class NonEmptyCatchClauseInAwait
    {
        public async void Foo()
        {
            var x = 42;
            await Task.Delay(100).ConfigureAwait(false);
            try
            {
                Debug.WriteLine("EmptyCatchClauseInAwait");
            }
            catch
            {
                Debug.WriteLine("Not empty");
            }
        }
    }
}
