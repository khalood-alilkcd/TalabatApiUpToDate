using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class ClientKPIData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int NumOfOrderPerWeek { get; set; }
        public int NumOfOrderPerMonth { get; set; }
        public double TotalMoneyPerWeek { get; set; }
        public double TotalMoneyPerMonth { get; set; }
        public double Bonus { get; set; }
        public double deduct { get; set; }
    }
}
