using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.api.attributes
{
    /// <summary>
    /// Experimental feature
    /// </summary>
    public class ExperimentalAttribute : Attribute
    {
        /// <summary>
        /// Tag a feature as experimental
        /// </summary>
        public ExperimentalAttribute()
        {

        }
        /// <summary>
        /// Tag a feature as experimental with tip string
        /// </summary>
        public ExperimentalAttribute(string tip)
        {
            
        }
    }
}
