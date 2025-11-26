# Location Seeding Service

## Overview
This service automatically seeds the database with provinces, cities, and locations data for the Italian regions specified.

## Automatic Seeding
The seeding process runs automatically when the application starts. The `LocationDataSeeder.SeedLocations()` method is called in `Program.cs` during application startup.

## Manual Seeding
You can also trigger the seeding process manually by calling the API endpoint:

```
POST /api/Location/SeedLocations
```

## Data Included

### Provinces (16 total)
- Arezzo
- Caserta
- Chieti
- Firenze
- Frosinone
- L'Aquila
- Latina
- Napoli
- Perugia
- Rieti
- Roma
- Sassari
- Terni
- Trento
- Viterbo

### Cities
Each province contains multiple cities. For example:
- **Roma**: 67 cities including Roma, Ardea, Fiumicino, Tivoli, etc.
- **Frosinone**: 12 cities including Frosinone, Fiuggi, Anagni, etc.
- **Firenze**: 3 cities including Firenze, Empoli, Tavarnelle Val di Pesa

### Locations
Locations are available for specific cities, particularly in the Roma province:
- **Roma**: 67 locations including Acilia, Eur, Ostia, Monte Sacro, etc.
- **Ardea**: 5 locations including Centro Storico, Nuova California, etc.
- **Fiumicino**: 6 locations including Fiumicino Paese, Isola Sacra, etc.
- **Tivoli**: 7 locations including Centro Storico, Tivoli Terme, etc.

## Database Structure
The seeding creates the following hierarchy:
```
Province (1) → City (N) → Location (N)
```

## Safety Features
- The seeder checks if data already exists before seeding
- If locations already exist, the seeding process is skipped
- All operations are wrapped in try-catch blocks for error handling

## Usage Examples

### Check if seeding is needed
```csharp
if (!context.Locations.Any())
{
    await LocationDataSeeder.SeedLocations(context);
}
```

### Manual seeding via API
```bash
curl -X POST https://your-api-url/api/Location/SeedLocations
```

### Response
```json
{
    "status": "Success",
    "message": "Locations seeded successfully",
    "result": true
}
```

## Notes
- The seeding process is idempotent - it can be run multiple times safely
- All timestamps (CreationDate, UpdateDate) are set to the current time during seeding
- The service maintains referential integrity between provinces, cities, and locations 