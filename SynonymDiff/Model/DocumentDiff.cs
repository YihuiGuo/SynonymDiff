using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonymDiff.Model
{
    public class DocumentDiff
    {
        public List<string> OldValue { get; set; }
        public string NewValue { get; set; }

    }
}
