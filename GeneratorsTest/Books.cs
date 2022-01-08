using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorsTest
{
    //Demo class which should be used as a base to generate our viewmodels
    public class Books : IGeneratable
    {
        public string? ISBN { get; set; }
        public int FaISBN { get; set; }
        public string? Author { get; set; }
        public int FaAuthor { get; set; }
    }
}
