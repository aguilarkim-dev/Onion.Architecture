using AutoMapper;
using Contracts.Interfaces;
using Entities.Exceptions;
using Service.Contracts.Interfaces;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class CompanyService : ICompanyService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public CompanyService(IRepositoryManager repository
            , ILoggerManager logger
            , IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public IEnumerable<CompanyDto> GetAllCompanies(bool trackChanges)
        {
            var companies = _repository.Company.GetAllCompanies(trackChanges);
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return companiesDto;
        }

        public CompanyDto GetCompany(Guid companyId, bool trachChanges)
        {
            var company = _repository.Company.GetCompany(companyId, trachChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);


            var companyDto = _mapper.Map<CompanyDto>(company);
            return companyDto;
        }
    }
}
