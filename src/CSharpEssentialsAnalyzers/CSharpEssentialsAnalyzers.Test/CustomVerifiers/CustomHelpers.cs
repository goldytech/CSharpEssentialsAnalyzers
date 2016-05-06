namespace CSharpEssentialsAnalyzers.Test.CustomVerifiers
{
    using System.Text.RegularExpressions;

    public class CustomHelpers
    {
        private static Regex namespaceBeforeRegex = new Regex(@"namespace ([ \w\t\.]+?)Before");
        private static string namespaceAfterRegexString = @"namespace {0}After\d*";

        public static string AlterAfterToCommonNamespace(string fixTest, string newCommonNamespace, string namespaceCore)
        {
            var afterRegex = new Regex(string.Format(namespaceAfterRegexString, namespaceCore));
            return afterRegex.Replace(fixTest, newCommonNamespace);
        }

        public static string AlterBeforeToCommonNamespace(string test, string newCommonNamespace)
        {
            return namespaceBeforeRegex.Replace(test, newCommonNamespace);
        }

        public static string GetNamespaceCore(string test)
        {
            var match = namespaceBeforeRegex.Match(test);
            return match.Groups[1].Value;
        }

        public static void MakeCommonNamespaces(ref string before, ref string after)
        {
            var newCommonNamespace = "namespace TestSource";
            var namespaceCore = GetNamespaceCore(before);
            before = AlterBeforeToCommonNamespace(before, newCommonNamespace);
            after = AlterAfterToCommonNamespace(after, newCommonNamespace, namespaceCore);
        }
    }
}