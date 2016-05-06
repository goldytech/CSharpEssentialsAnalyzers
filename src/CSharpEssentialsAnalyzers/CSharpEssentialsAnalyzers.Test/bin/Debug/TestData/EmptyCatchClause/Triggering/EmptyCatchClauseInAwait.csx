namespace CSharpEssentialsAnalyzers.Test.TestData.EmptyCatchClause
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class EmptyCatchClauseInAwait
    {
        public async void Foo()
        {
            var x = 42;
            await Task.Delay(100);
            try
            {
                Debug.WriteLine("EmptyCatchClauseInAwait");
            }
            catch
            {
                
            }
        }
    }
}