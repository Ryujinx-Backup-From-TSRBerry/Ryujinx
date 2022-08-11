using Ryujinx.Ava.Common.Ui.ViewModels;
using System;
using System.Globalization;
using System.IO;

namespace Ryujinx.Rsc.Models
{
    public class Driver : BaseModel
    {
        private bool _isSelected;
        public string Name { get; }
        public bool IsSystem { get; }
        public string Path { get; }
        public string DateAdded { get; } = "N/A";
        public string Vendor => Metadata?.Vendor ?? "N/A";
        public string Version => Metadata?.DriverVersion ?? "0.0";
        public string PackageVersion => Metadata?.PackageVersion ?? "0.0";
        public DriverMetadata Metadata { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public Driver(string name, string path, bool isSystem)
        {
            Name = name;
            IsSystem = isSystem;
            Path = path;

            if (path.Contains(System.IO.Path.DirectorySeparatorChar))
            {
                var parent = new FileInfo(path).Directory.Name;
                if (DateTime.TryParseExact(parent, "yyyy-MM-dd HH-mm-ss", null, DateTimeStyles.None, out var date))
                {
                    DateAdded = date.ToString();
                }
            }
        }
    }
}