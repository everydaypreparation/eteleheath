using Abp.Application.Services;
using EMRO.Master.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Master
{
    public interface IMasterAppService : IApplicationService
    {
        //GetCountryOutput GetCountries();
        //GetStateOutput GetStates(long? countryId);
        //GetCityDtoOutput GetCities(long? stateId);
        //GetPostalCodeDtoOutput GetPostalcodes(long? CityId);
        GetReportTypeDtoOutput GetReportType();
        TimeZonesOutput GetTimeZone();

    }
}
