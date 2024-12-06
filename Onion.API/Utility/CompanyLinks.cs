using Contracts.Interfaces;
using Entities.LinkModels;
using Entities.Models;
using Microsoft.Net.Http.Headers;
using Shared.DataTransferObjects;

namespace Onion.API.Utility
{
    public class CompanyLinks : ICompanyLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<CompanyDto> _dataShaper;

        public CompanyLinks(LinkGenerator linkGenerator, IDataShaper<CompanyDto> dataShaper)
        {
            _linkGenerator = linkGenerator;
            _dataShaper = dataShaper;
        }

        public LinkResponse TryGenerateLinks(IEnumerable<CompanyDto> companiesDto, string fields, HttpContext httpContext)
        {
            var shapedEmployees = ShapeData(companiesDto, fields);
            if (ShouldGenerateLinks(httpContext))
                return ReturnLinkdedEmployees(companiesDto, fields, httpContext, shapedEmployees);

            return ReturnShapedEmployees(shapedEmployees);

        }

        private List<Entity> ShapeData(IEnumerable<CompanyDto> companiesDto, string fields)
        {
            return _dataShaper.ShapeData(companiesDto, fields)
                .Select(e => e.Entity)
                .ToList();
        }

        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
            var mediaType = (MediaTypeHeaderValue)httpContext.Items["AcceptHeaderMediaType"];
            return mediaType.SubTypeWithoutSuffix.EndsWith("hateoas",
           StringComparison.InvariantCultureIgnoreCase);
        }

        private LinkResponse ReturnShapedEmployees(List<Entity> shapedEmployees)
        {
            return new LinkResponse { ShapedEntities = shapedEmployees };
        }

        private LinkResponse ReturnLinkdedEmployees(IEnumerable<CompanyDto> companiesDto
            , string fields
            , HttpContext httpContext
            , List<Entity> shapedEmployees)
        {
            var companyDtoList = companiesDto.ToList();
            for (var index = 0; index < companyDtoList.Count(); index++)
            {
                var employeeLinks = CreateLinksForEmployee(httpContext, companyDtoList[index].Id, fields);
                shapedEmployees[index].Add("Links", employeeLinks);
            }
            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
            var linkedEmployees = CreateLinksForEmployees(httpContext, employeeCollection);
            return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
        }

        private List<Link> CreateLinksForEmployee(HttpContext httpContext
            , Guid id
            , string fields = "")
        {
            var links = new List<Link>
             {
                 new Link(_linkGenerator.GetUriByAction(httpContext
                 , "GetCompany", values: new { id, fields }),
                     "self",
                     "GET"),
                 new Link(_linkGenerator.GetUriByAction(httpContext,
                    "DeleteCompany", values: new { id }),
                     "delete_employee",
                     "DELETE"),
                 new Link(_linkGenerator.GetUriByAction(httpContext,
                    "UpdateCompany", values: new { id }),
                     "update_employee",
                     "PUT"),
                 new Link(_linkGenerator.GetUriByAction(httpContext,
                    "PartiallyUpdateCompany", values: new { id }),
                     "partially_update_employee",
                     "PATCH")
             };
            return links;

        }

        private LinkCollectionWrapper<Entity> CreateLinksForEmployees(HttpContext httpContext, LinkCollectionWrapper<Entity> employeesWrapper)
        {
            employeesWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext,
               "GetCompany", values: new { }),
                "self",
                "GET"));

            return employeesWrapper;
        }
    }
}
