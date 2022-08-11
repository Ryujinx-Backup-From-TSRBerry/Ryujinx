namespace Ryujinx.Rsc.Models
{
    public class DriverMetadata
    {
        public int SchemaVersion { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string PackageVersion { get; set; }
        public string Vendor { get; set; }
        public string DriverVersion { get; set; }
        public int MinApi { get; set; }
        public string LibraryName { get; set; }
    }
}