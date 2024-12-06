using Entities.Models;
using Microsoft.AspNetCore.Http;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface ICompanyLinks
    {
        LinkResponse TryGenerateLinks(IEnumerable<CompanyDto> companiesDto, string fields, HttpContext httpContext);
    }
}
