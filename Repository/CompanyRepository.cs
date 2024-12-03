using Contracts.Interfaces.Repositories;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    internal sealed class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
        {
        }

        public void CreateCompany(Company company)
        {
            Create(company);
        }

        public void DeleteCompany(Company company)
        {
            Delete(company);
        }

        public async Task<PagedList<Company>> GetAllCompaniesAsync(CompanyParameters companyParameters, bool trackChanges)
        {
            var companies = await FindAll(trackChanges)
                .OrderBy(c => c.Name)
                .Skip((companyParameters.PageNumber - 1) * companyParameters.PageSize)
                .Take(companyParameters.PageSize)
                .ToListAsync();

            var count = await FindAll(trackChanges).CountAsync();

            return new PagedList<Company>(companies,
               count,
               companyParameters.PageNumber,
               companyParameters.PageSize);
        }

        public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
        {
            return await FindByCondition(x => ids.Contains(x.Id), trackChanges)
                .ToListAsync();
        }

        public async Task<Company?> GetCompanyAsync(Guid companyId, bool trackChanges)
        {
            return await FindByCondition(c => c.Id.Equals(companyId), trackChanges)
                .SingleOrDefaultAsync();
        }
    }

}
