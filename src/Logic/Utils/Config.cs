namespace Logic.Utils
{
    public sealed class Config
    {
        public int NumberOfRetries { get; set; }

        public Config()
        {
        }
        public Config(int numberOfRetries) : this()
        {
            NumberOfRetries = numberOfRetries;
        }

        
    }
}
