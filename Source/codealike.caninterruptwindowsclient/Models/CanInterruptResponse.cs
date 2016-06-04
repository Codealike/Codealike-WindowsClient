using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codealike.CanInterruptWindowsClient.Models
{
    public class CanInterruptResponse
    {
        public string Status { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int DriverValue { get; set; }
    }
}
