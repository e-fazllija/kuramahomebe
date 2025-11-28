using BackEnd.Entities;

namespace BackEnd.Services.BusinessServices
{
    public static class IdealistaMappingHelper
    {
        public static object MapPropertyToIdealistaFormat(RealEstateProperty property, int? contactId = null)
        {
            // Mappa la tipologia
            var type = MapTypology(property.Typology);
            
            // Mappa lo stato dell'immobile
            var conservation = MapStateOfProperty(property.StateOfTheProperty);
            
            // Mappa l'equipaggiamento
            var equipment = MapFurniture(property.Furniture);
            
            // Mappa il riscaldamento
            var heatingType = MapHeating(property.Heating);
            
            // Mappa l'orientamento
            var (orientationNorth, orientationSouth, orientationEast, orientationWest) = MapExposure(property.Exposure);
            
            // Mappa la posizione delle finestre
            var windowsLocation = MapWindowsLocation(property.Exposure);
            
            // Mappa l'operazione (vendita/affitto)
            var operationType = property.Status == "Vendita" ? "sale" : "rent";
            
            // Mappa la classe energetica
            var energyRating = MapEnergyClass(property.EnergyClass);
            
            // Costruisce l'oggetto per Idealista
            var idealistaProperty = new
            {
                propertyId = property.IdealistaPropertyId ?? 0,
                type = type,
                address = new
                {
                    street = property.AddressLine,
                    city = property.City,
                    zipcode = property.PostCode,
                    province = property.State
                },
                code = property.Id.ToString(),
                reference = property.Id.ToString(),
                casaCode = property.Id.ToString(),
                contactId = contactId ?? 1,
                features = new
                {
                    areaConstructed = property.CommercialSurfaceate,
                    areaUsable = property.CommercialSurfaceate,
                    balcony = property.OtherFeatures?.ToLower().Contains("balcone") == true,
                    bathroomNumber = property.Bathrooms,
                    builtYear = property.YearOfConstruction > 0 ? property.YearOfConstruction : 2000,
                    conditionedAir = property.OtherFeatures?.ToLower().Contains("aria condizionata") == true || 
                                    property.OtherFeatures?.ToLower().Contains("climatizzato") == true,
                    conservation = conservation,
                    doorman = property.OtherFeatures?.ToLower().Contains("portiere") == true,
                    duplex = property.Typology?.ToLower().Contains("duplex") == true,
                    equipment = equipment,
                    energyCertificatePerformance = 0.01,
                    energyCertificateLaw = "dl-192_2005",
                    energyCertificateRating = energyRating,
                    energyCertificateEmissionsRating = energyRating,
                    energyCertificateEmissionsValue = 0.01,
                    isInTopFloor = property.Floor?.ToLower().Contains("ultimo") == true || 
                                  property.Floor?.ToLower().Contains("attico") == true,
                    garden = property.MQGarden > 0,
                    handicappedAdaptedAccess = false,
                    handicappedAdaptedUse = false,
                    liftAvailable = property.Elevators > 0,
                    orientationNorth = orientationNorth,
                    orientationSouth = orientationSouth,
                    orientationWest = orientationWest,
                    orientationEast = orientationEast,
                    parkingAvailable = property.ParkingSpaces > 0,
                    parkingIncludedInPrice = property.ParkingSpaces > 0,
                    parkingPrice = 0,
                    penthouse = property.Typology?.ToLower().Contains("attico") == true,
                    petsAllowed = false,
                    tenantNumber = 0,
                    recommendedForChildren = false,
                    pool = property.OtherFeatures?.ToLower().Contains("piscina") == true,
                    rooms = property.Bedrooms,
                    storage = property.WarehouseRooms > 0,
                    studio = property.Typology?.ToLower().Contains("monolocale") == true,
                    terrace = property.OtherFeatures?.ToLower().Contains("terrazzo") == true,
                    wardrobes = property.OtherFeatures?.ToLower().Contains("armadi") == true,
                    windowsLocation = windowsLocation,
                    heatingType = heatingType,
                    priceCommunity = (int)property.CondominiumExpenses,
                    depositMonths = 0,
                    priceReferenceIndex = 1,
                    isAuction = property.Auction,
                    minAuctionBidIncrement = 0,
                    auctionDeposit = 0,
                    auctionDate = property.Auction ? property.AssignmentEnd.ToString("yyyy-MM-dd") : null,
                    auctionTribunal = "tribunale_di_alessandria_ex_tribunale_di_acqui_terme",
                    currentOccupation = property.Availability?.ToLower().Contains("libero") == true ? "free" : "not_free",
                    gardenType = property.MQGarden > 0 ? "private" : null,
                    parkingSpaceCapacity = property.ParkingSpaces > 0 ? "single" : null,
                    parkingSpaceArea = property.ParkingSpaces > 0 ? 1 : 0,
                    outdoorParkingSpaceAvailable = property.ParkingSpaces > 0,
                    outdoorParkingSpaceType = property.ParkingSpaces > 0 ? "covered" : null,
                    outdoorParkingSpaceNumber = property.ParkingSpaces,
                    hiddenPrice = false,
                    residential = property.Category?.ToLower().Contains("residenziale") == true,
                    seasonalRental = false,
                    shortTerm = false,
                    shortTermLicense = (string?)null,
                    shortTermLicenseNational = (string?)null,
                    lowSeasonPrice = 0,
                    highSeasonPrice = 0,
                    residentOnly = false
                },
                operation = new
                {
                    price = (int)property.Price,
                    type = operationType
                },
                descriptions = new[]
                {
                    new
                    {
                        language = "it",
                        text = property.Description
                    }
                },
                additionalLink = property.VideoUrl ?? (string?)null,
                scope = "idealista"
            };

            return idealistaProperty;
        }

        public static string MapTypology(string? typology)
        {
            if (string.IsNullOrEmpty(typology))
                return "flat";

            var lower = typology.ToLower();
            if (lower.Contains("villa") || lower.Contains("casa"))
                return "house";
            if (lower.Contains("ufficio") || lower.Contains("locale"))
                return "office";
            if (lower.Contains("negozio") || lower.Contains("commerciale"))
                return "shop";
            if (lower.Contains("terreno"))
                return "land";
            
            return "flat";
        }

        private static string MapStateOfProperty(string? state)
        {
            if (string.IsNullOrEmpty(state))
                return "good";

            var lower = state.ToLower();
            if (lower.Contains("nuovo") || lower.Contains("ristrutturato"))
                return "new";
            if (lower.Contains("ottimo") || lower.Contains("buono"))
                return "good";
            if (lower.Contains("da ristrutturare") || lower.Contains("da sistemare"))
                return "to_renovate";
            
            return "good";
        }

        private static string MapFurniture(string? furniture)
        {
            if (string.IsNullOrEmpty(furniture))
                return "unfurnished";

            var lower = furniture.ToLower();
            if (lower.Contains("arredato") || lower.Contains("completo"))
                return "equipped_kitchen_and_furnished";
            if (lower.Contains("parzialmente"))
                return "equipped_kitchen";
            
            return "unfurnished";
        }

        private static string MapHeating(string? heating)
        {
            if (string.IsNullOrEmpty(heating))
                return "none";

            var lower = heating.ToLower();
            if (lower.Contains("centralizzato") || lower.Contains("centrale"))
            {
                if (lower.Contains("gas"))
                    return "central_gas";
                if (lower.Contains("metano"))
                    return "central_gas";
                return "central";
            }
            if (lower.Contains("autonomo"))
                return "autonomous";
            if (lower.Contains("elettrico"))
                return "electric";
            
            return "none";
        }

        private static (bool north, bool south, bool east, bool west) MapExposure(string? exposure)
        {
            if (string.IsNullOrEmpty(exposure))
                return (false, false, false, false);

            var lower = exposure.ToLower();
            return (
                north: lower.Contains("nord"),
                south: lower.Contains("sud"),
                east: lower.Contains("est"),
                west: lower.Contains("ovest")
            );
        }

        private static string MapWindowsLocation(string? exposure)
        {
            if (string.IsNullOrEmpty(exposure))
                return "external";

            var lower = exposure.ToLower();
            if (lower.Contains("interno") || lower.Contains("cortile"))
                return "internal";
            
            return "external";
        }

        private static string MapEnergyClass(string? energyClass)
        {
            if (string.IsNullOrEmpty(energyClass))
                return "G";

            var upper = energyClass.ToUpper();
            if (upper.Contains("A4") || upper.Contains("A+"))
                return "A";
            if (upper.Contains("A3"))
                return "A";
            if (upper.Contains("A2"))
                return "A";
            if (upper.Contains("A1"))
                return "A";
            if (upper.Contains("B"))
                return "B";
            if (upper.Contains("C"))
                return "C";
            if (upper.Contains("D"))
                return "D";
            if (upper.Contains("E"))
                return "E";
            if (upper.Contains("F"))
                return "F";
            
            return "G";
        }
    }
}

