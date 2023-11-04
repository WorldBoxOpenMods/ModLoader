using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NCMS
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class NCMod
    {
        public string author;

        public string description;

        public string iconPath;

        public string name;

        public string path;

        public int targetGameBuild = 444;

        public string version;
    }
}
