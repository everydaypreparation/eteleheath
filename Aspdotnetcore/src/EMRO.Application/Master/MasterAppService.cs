using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EMRO.Master.Dto;
using EMRO.ReportTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneNames;

namespace EMRO.Master
{
    [AbpAuthorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class MasterAppService : ApplicationService, IMasterAppService
    {
        //IRepository<Country, long>, IRepository<State, long>, IRepository<City, long>, IRepository<PostalCode, long>
        //These members set in constructor using constructor injection.

        private readonly IRepository<ReportType, Guid> _reportTypeRepository;
        private readonly IRepository<TimeZones, Guid> _timezonesRepository;

        public MasterAppService(
            IRepository<ReportType, Guid> reportTypeRepository, IRepository<TimeZones, Guid> timezonesRepository)
        {


            _reportTypeRepository = reportTypeRepository;
            _timezonesRepository = timezonesRepository;
        }

        //public GetCityDtoOutput GetCities(long? stateId)
        //{
        //    //var citylist = _cityRepository.GetAllList().Where(c => c.StateId == stateId);

        //    //var cityDtoList = citylist.
        //    //          Select(c => new CityDto
        //    //          {
        //    //              CityId = c.Id,
        //    //              Name = c.Name,
        //    //              Abbr = c.Abbr
        //    //          }).ToList();

        //    //return new GetCityDtoOutput { CityList = cityDtoList };
        //}

        //public GetCountryOutput GetCountries()
        //{

        //    var countrylist = _countryRepository.GetAllList();

        //    var countryDtoList = countrylist.
        //              Select(country => new CountryDto
        //              {
        //                  CountryId = country.Id,
        //                  Name = country.Name,
        //                  Abbr = country.Abbr
        //              }).ToList();

        //    return new GetCountryOutput { Countries = countryDtoList };
        //}

        //public GetPostalCodeDtoOutput GetPostalcodes(long? CityId)
        //{
        //    var postalcodelist = _postalCodeRepository.GetAllList().Where(c => c.CityId == CityId);

        //    var postalcodeDtoList = postalcodelist.
        //              Select(p => new PostalCodeDto
        //              {
        //                  PostalCodeId = p.Id,
        //                  Name = p.Name,
        //                  Abbr = p.Abbr
        //              }).ToList();

        //    return new GetPostalCodeDtoOutput { postalCodes = postalcodeDtoList };
        //}

        public GetReportTypeDtoOutput GetReportType()
        {
            var reportlist = _reportTypeRepository.GetAll();

            var ReportDtoList = reportlist.
                      Select(p => new ReportTypeDto
                      {
                          Id = p.Id,
                          Name = p.Name,

                      }).ToList();

            return new GetReportTypeDtoOutput { ReportTypes = ReportDtoList };
        }

        //public GetStateOutput GetStates(long? countryId)
        //{
        //    var statelist = _stateRepository.GetAll().Where(s => s.CountryId == countryId);

        //    var stateDtoList = statelist.
        //              Select(country => new StateDto
        //              {
        //                  StateId = country.Id,
        //                  Name = country.Name,
        //                  Abbr = country.Abbr
        //              }).ToList();

        //    return new GetStateOutput { states = stateDtoList };
        //}

        public TimeZonesOutput GetTimeZone()
        {
            TimeZonesOutput output = new TimeZonesOutput();
            try
            {
                var timezones = TimeZoneInfo.GetSystemTimeZones();
                output.Items = timezones.Select(x => new TimeZonesListOutput
                {
                    TimeZoneId = x.Id,
                    Abbr = TZNames.GetAbbreviationsForTimeZone(x.Id, "en-US").Standard,
                    UTCOffset = x.GetUtcOffset(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, x)).ToString().Contains("-") ?
                    "(UTC" + x.GetUtcOffset(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, x)).ToString().Substring(0, 6) + ")" + x.DisplayName.Split(')')[1] :
                    "(UTC+" + x.GetUtcOffset(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, x)).ToString().Substring(0, 5) + ")" + x.DisplayName.Split(')')[1]
                }).OrderBy(x => x.Abbr).ToList();
                output.Message = "Get timezone list.";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Delete Documents Error:" + ex.StackTrace);
            }
            return output;
        }
    }
}
