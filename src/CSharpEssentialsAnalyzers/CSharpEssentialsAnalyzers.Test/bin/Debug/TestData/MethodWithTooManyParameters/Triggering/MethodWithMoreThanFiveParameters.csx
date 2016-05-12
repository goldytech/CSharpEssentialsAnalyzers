namespace CSharpEssentialsAnalyzers.Test.TestData.MethodWithTooManyParameters.Triggering
{
    public class MethodWithMoreThanFiveParameters
    {
        public void Method1(string p1, string p2, int p3, int p4, double p5)
        {
            var str = "Hello World";
            p1 = str;
        }
    }
}