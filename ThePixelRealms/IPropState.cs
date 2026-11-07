using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public interface IPropState
    {
        double currentShield { get; set; }

        Rectangle ShieldBarFill { get; }

        double appleHealAmount { get; }
    }
}
