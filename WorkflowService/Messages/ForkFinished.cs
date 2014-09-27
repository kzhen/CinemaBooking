using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowService.Messages
{
  internal class ForkFinished
  {
    public string PhoneNumber { get; set; }
    public bool Abort { get; set; }
    public string ForkToKeyword { get; set; }
  }
}
