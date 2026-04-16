namespace Model.SETSDB
{
    public class PC_Master
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }
}