using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Models.Requests
{
    public class CreateRequestDto
    {
        public string Title { get; set; }
        public string Description { get; set; }

        //more detail #TODO
        //public int CreatedBy { get; set; }
    }
}
