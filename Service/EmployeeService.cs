﻿using AutoMapper;
using Contracts.Interfaces;
using Entities.Exceptions;
using Entities.LinkModels;
using Entities.Models;
using Service.Contracts.Interfaces;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        private readonly IEmployeeLinks _employeeLinks;
        public EmployeeService(IRepositoryManager repository
            , ILoggerManager logger
            , IMapper mapper
            , IDataShaper<EmployeeDto> dataShaper
            , IEmployeeLinks employeeLinks)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
            _employeeLinks = employeeLinks;
        }

        public async Task<(LinkResponse linkResponse, MetaData metaData)> GetEmployeesAsync(Guid companyId, LinkEmployeeParameters linkEmployeeParameters, bool trackChanges)
        {
            //if (!employeeParameters.ValidAgeRange())
            if (!linkEmployeeParameters.EmployeeParameters.ValidAgeRange())
                throw new MaxAgeRangeBadRequestException();

            await CheckIfCompanyExists(companyId, trackChanges);

            //var employeesWithMetaData = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges);
            var employeesWithMetaData = await _repository.Employee.GetEmployeesAsync(companyId, linkEmployeeParameters.EmployeeParameters, trackChanges);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesWithMetaData);
            //var shapedData = _dataShaper.ShapeData(employeesDto, employeeParameters.Fields);
            var linkResponse = _employeeLinks.TryGenerateLinks(employeesDto, linkEmployeeParameters.EmployeeParameters.Fields, companyId, linkEmployeeParameters.Context);


            return (linkResponse: linkResponse, metaData: employeesWithMetaData.MetaData);
        }

        public async Task<ShapedEntity> GetEmployeeAsync(Guid companyId, Guid employeeId, EmployeeParameters employeeParameters, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);

            var employeeFromDb = await GetEmployeeForCompanyAndCheckIfItExists(companyId, employeeId, trackChanges);
            var employeeDto = _mapper.Map<EmployeeDto>(employeeFromDb);
            var shapedEntity = _dataShaper.ShapeData(employeeDto, employeeParameters.Fields);

            return shapedEntity;
        }

        public async Task<EmployeeDto> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);

            var employeeEntity = _mapper.Map<Employee>(employeeForCreation);
            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repository.SaveAsync();

            var employeeDto = _mapper.Map<EmployeeDto>(employeeEntity);

            return employeeDto;
        }

        public async Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid employeeId, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);

            var employeeForCompany = await GetEmployeeForCompanyAndCheckIfItExists(companyId, employeeId, trackChanges);

            _repository.Employee.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();
        }

        public async Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid employeeId, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);

            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, employeeId, empTrackChanges);

            _mapper.Map(employeeForUpdate, employeeEntity);
            await _repository.SaveAsync();
        }

        public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync (
            Guid companyId, 
            Guid employeeId, 
            bool compTrackChanges, 
            bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);

            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, employeeId,empTrackChanges);
            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

            return (employeeToPatch, employeeEntity);
        }
        public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
        {
            _mapper.Map(employeeToPatch, employeeEntity);

            await _repository.SaveAsync();
        }

        private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);
        }

        private async Task<Employee> GetEmployeeForCompanyAndCheckIfItExists(Guid companyId, Guid id, bool trackChanges)
        {
            var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);
            if (employeeDb is null)
                throw new EmployeeNotFoundException(id);

            return employeeDb;
        }

    }
}
