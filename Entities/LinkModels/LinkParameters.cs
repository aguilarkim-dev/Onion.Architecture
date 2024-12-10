using Microsoft.AspNetCore.Http;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Entities.LinkModels
{
    public record LinkEmployeeParameters(EmployeeParameters EmployeeParameters, HttpContext Context);
    public record LinkCompanyParameters(CompanyParameters CompanyParameters, HttpContext Context);
}
