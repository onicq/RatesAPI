using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RatesAPI.Models
{
    public class Currency
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string Date { get; set; }
    }
}
