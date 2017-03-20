using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ScheMe
{
    public abstract class Tool
    {
        public abstract void Click(Point point);
    }

    public class AddNodeTool : Tool
    {
        public override void Click(Point point)
        {
            throw new NotImplementedException();
        }
    }
}
