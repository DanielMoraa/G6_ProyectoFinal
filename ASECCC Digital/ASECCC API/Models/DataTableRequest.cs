namespace ASECCC_API.Models
{
    public class DataTableRequest
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }

        public DataTableSearch search { get; set; } = new();
    }

    public class DataTableSearch
    {
        public string? value { get; set; }
        public bool regex { get; set; }
    }
}
