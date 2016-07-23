namespace Panoramio.Server
{
    public class ExecuteResult<TEntity>
    {
        public TEntity Result { get; set; }

        public string ErrorValue { get; set; }

        public int ResultCode { get; set; }
    }
}
