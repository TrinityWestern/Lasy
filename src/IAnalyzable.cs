using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface IAnalyzable
    {
        /// <summary>
        /// Get the analyzer for the DB
        /// </summary>
        IDBAnalyzer Analyzer { get; }
    }
}
