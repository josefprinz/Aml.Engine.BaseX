using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Aml.Engine.Services.BaseX.Model
{
    /// <summary>
    /// Information about a document resource in a BaseX database
    /// </summary>
    public class DocumentInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInfo"/> struct.
        /// </summary>
        /// <param name="element">The element.</param>
        internal DocumentInfo (string databaseName, XElement element)
        {
            Name = element.Value;
            if (long.TryParse(element.Attribute("size")?.Value, out var size))
            {
                Size = size;
            }
            DatabaseName = databaseName;
        }

        internal DocumentInfo(string databaseName, string documentName)
        {
            Name = documentName;
            DatabaseName = databaseName;
        }

        /// <summary>
        /// Gets the name of the document file.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }
        public string DatabaseName { get; }

        /// <summary>
        /// Gets the total size of the document file.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public long Size { get; }
    }
}
