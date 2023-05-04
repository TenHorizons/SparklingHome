using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SparklingHome.Models
{
    public class News
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string PostedBy { get; set; }
        public string DatePosted { get; set; }
        public string Image { get; set; }
    }
}
