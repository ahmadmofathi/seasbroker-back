using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Constants;
using Seasbroker.Modules.Vessel.Application.DTOs;

namespace Seasbroker.Modules.Vessel.Application.Mapping;

public static class VesselMapper
{
    public static VesselRecordDto ToRecordDto(global::Seasbroker.Infrastructure.Persistence.Entities.Vessel vessel)
    {
        return new VesselRecordDto
        {
            Id = vessel.Id.ToString(),
            CollectionId = VesselConstants.VesselsCollectionName,
            CollectionName = VesselConstants.VesselsCollectionName,
            Created = vessel.Created,
            Updated = vessel.Updated,
            Name = vessel.Name,
            ImoNumber = vessel.ImoNumber,
            VesselType = vessel.VesselType,
            Dwt = vessel.Dwt,
            TeuCapacity = vessel.TeuCapacity,
            LengthOverall = vessel.LengthOverall,
            Beam = vessel.Beam,
            Draft = vessel.Draft,
            CurrentPort = vessel.CurrentPort,
            FlagCountry = vessel.FlagCountry,
            Status = vessel.Status,
            Customer = vessel.CustomerId?.ToString(),
            Notes = vessel.Notes,
        };
    }

    public static VesselAvailabilityRecordDto ToRecordDto(VesselAvailability availability)
    {
        return new VesselAvailabilityRecordDto
        {
            Id = availability.Id.ToString(),
            CollectionId = VesselConstants.VesselAvailabilitiesCollectionName,
            CollectionName = VesselConstants.VesselAvailabilitiesCollectionName,
            Created = availability.Created,
            Updated = availability.Updated,
            VesselId = availability.VesselId.ToString(),
            AvailableFrom = availability.AvailableFrom,
            AvailableTo = availability.AvailableTo,
            OpenPort = availability.OpenPort,
            DestinationPort = availability.DestinationPort,
            IsActive = availability.IsActive,
        };
    }
}
