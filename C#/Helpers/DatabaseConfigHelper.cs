namespace ODAL.Helpers
{
    public class DatabaseConfigHelper
    {
        public int OracleDriverCommandTimeout { get; set; }
        public int Varchar2MaxBufferSize { get; set; }
        public int NumberMaxBufferSize { get; set; }
        public string DatabaseDateFormat { get; set; }
        public string Tsk { get; set; }
    }
}
