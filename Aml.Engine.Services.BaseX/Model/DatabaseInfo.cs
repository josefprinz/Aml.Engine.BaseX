using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Aml.Engine.Services.BaseX.Model
{
    /// <summary>
    /// Information about a database managed by BaseX
    /// </summary>
    public readonly struct DatabaseInfo
    {
        internal DatabaseInfo(XElement element)
        { 
            Name = element.Value;
            if ( long.TryParse (element.Attribute("size")?.Value, out var size))
            {
                Size = size;
            }
            if (int.TryParse(element.Attribute("resources")?.Value, out var resources))
            {
                Resources = resources;
            }
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get;  }

        /// <summary>
        /// Gets the number of resources in the database.
        /// </summary>
        /// <value>
        /// The resources.
        /// </value>
        public int Resources { get;  }

        /// <summary>
        /// Gets the total size of the database.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public long Size { get;  }  
    }
}
