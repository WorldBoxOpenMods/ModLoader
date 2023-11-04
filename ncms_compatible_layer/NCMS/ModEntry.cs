using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCMS
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    [AttributeUsage(AttributeTargets.Class)]
    public class ModEntry : Attribute
    {
    }
}
