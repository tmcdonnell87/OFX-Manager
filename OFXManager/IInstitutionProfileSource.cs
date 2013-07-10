using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFXManager
{
    interface IInstitutionProfileSource
    {
        List<Institution> Search(string name);
        bool DownloadInstitutionData(Institution inst);
    }
}
