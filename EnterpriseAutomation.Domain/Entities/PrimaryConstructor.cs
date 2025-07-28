using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{
    public class PrimaryConstructor (string name , string family )
    {
        public string name { get; set; } = name;
        public string family { get; set; } = family;
    }
}
