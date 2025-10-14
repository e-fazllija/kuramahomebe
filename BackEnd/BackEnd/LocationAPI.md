# Location API Documentation

## Endpoints

### 1. Create Location
**POST** `/api/Location/Create`

Creates a new location.

**Request Body:**
```json
{
  "name": "Centro Storico",
  "city": "Roma",
  "isActive": true,
  "orderIndex": 1
}
```

**Response:**
```json
{
  "id": 1,
  "name": "Centro Storico",
  "city": "Roma",
  "isActive": true,
  "orderIndex": 1,
  "creationDate": "2024-01-01T00:00:00",
  "updateDate": "2024-01-01T00:00:00"
}
```

### 2. Update Location
**POST** `/api/Location/Update`

Updates an existing location.

**Request Body:**
```json
{
  "id": 1,
  "name": "Centro Storico Updated",
  "city": "Roma",
  "isActive": true,
  "orderIndex": 1
}
```

### 3. Get Locations (Paginated)
**GET** `/api/Location/Get?currentPage=1&filterRequest=centro&city=Roma`

Gets paginated list of locations with optional filtering.

**Query Parameters:**
- `currentPage` (int): Page number (default: 1)
- `filterRequest` (string, optional): Filter by name or city
- `city` (string, optional): Filter by specific city

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "Centro Storico",
      "city": "Roma",
      "isActive": true,
      "orderIndex": 1,
      "creationDate": "2024-01-01T00:00:00",
      "updateDate": "2024-01-01T00:00:00"
    }
  ],
  "total": 100
}
```

### 4. Get All Active Locations
**GET** `/api/Location/GetAll`

Gets all active locations.

**Response:**
```json
[
  {
    "id": 1,
    "name": "Centro Storico",
    "city": "Roma",
    "isActive": true,
    "orderIndex": 1,
    "creationDate": "2024-01-01T00:00:00",
    "updateDate": "2024-01-01T00:00:00"
  }
]
```

### 5. Get Locations Grouped by City
**GET** `/api/Location/GetGroupedByCity`

Gets locations grouped by city (similar to the original locations.ts format).

**Response:**
```json
[
  {
    "city": "Roma",
    "locations": [
      {
        "id": "Centro Storico",
        "name": "Centro Storico"
      },
      {
        "id": "Eur",
        "name": "Eur"
      }
    ]
  }
]
```

### 6. Get Location by ID
**GET** `/api/Location/GetById?id=1`

Gets a specific location by ID.

### 7. Delete Location
**DELETE** `/api/Location/Delete?id=1`

Deletes a location by ID.

### 8. Toggle Location Active Status
**POST** `/api/Location/ToggleActive?id=1`

Toggles the active status of a location.

### 9. Update Location Order
**POST** `/api/Location/UpdateOrder`

Updates the order of multiple locations.

**Request Body:**
```json
[
  {
    "id": 1,
    "name": "Centro Storico",
    "city": "Roma",
    "isActive": true,
    "orderIndex": 1
  },
  {
    "id": 2,
    "name": "Eur",
    "city": "Roma",
    "isActive": true,
    "orderIndex": 2
  }
]
```

## Error Responses

All endpoints return error responses in the following format:

```json
{
  "status": "Error",
  "message": "Error description"
}
```

## Notes

- The `GetGroupedByCity` endpoint returns data in the same format as the original `locations.ts` file
- All locations are automatically seeded with the existing data from `locations.ts` on application startup
- The `IsActive` field allows soft deletion of locations
- The `OrderIndex` field allows custom ordering of locations within each city
- The `Get` endpoint uses pagination internally but returns a simplified response with `data` and `total` properties 