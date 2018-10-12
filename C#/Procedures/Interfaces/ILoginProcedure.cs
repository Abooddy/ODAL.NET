namespace ODAL.Procedures
{
    public interface ILoginProcedure : IBaseProcedure
    {
        string PUsername { get; set; }
        string PPassword { get; set; }
        string PInst { get; set; }
    }
}