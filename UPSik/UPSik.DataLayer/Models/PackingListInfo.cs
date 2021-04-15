using System;
using System.Collections.Generic;
using System.Text;

namespace UPSik.DataLayer.Models
{
    public class PackingListInfo
    {
        public int Id { get; set; }
        public int CourierId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int PackagesCount { get; set; }
        public bool ManuallyManaged { get; set; }
        public bool IsManaged { get; set; }
        public float CourierRating { get; set; }
    } 
}
