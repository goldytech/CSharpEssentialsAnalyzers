namespace CSharpEssentialsAnalyzers
{
    public static class DiagnosticIdsGeneration
    {
        public static string ToDiagnosticId(this DiagnosticIds diagnosticId)
                   => $"CSEA{(int)diagnosticId:D3}";
    }

    public enum DiagnosticIds
    {
        /// <summary>
        /// The not yet set.
        /// </summary>
        NotYetSet = 0,

        /// <summary>
        /// The empty catch clause.
        /// </summary>
        EmptyCatchClause = 1
    }
}