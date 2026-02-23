# Setup ambiente di sviluppo locale

In sviluppo (`ASPNETCORE_ENVIRONMENT=Development`) l'app **non usa** KeyVault né risorse di produzione: tutto è configurato tramite `appsettings.Development.json`.

## Requisiti

### 1. Database PostgreSQL locale

- Crea un database locale, ad es. `kuramahome_dev`.
- In `appsettings.Development.json` aggiorna **ConnectionStrings:DefaultConnection** con host, porta, nome DB, utente e password del tuo PostgreSQL locale.
- Applica le migration: dalla cartella del progetto BackEnd esegui  
  `dotnet ef database update` (se usi EF Core Tools).

### 2. Storage locale (Azurite)

Lo storage dei blob (documenti e immagini proprietà) in sviluppo usa **Azurite** (emulatore Azure Storage).

- Installa Azurite (es. `npm install -g azurite` oppure tramite Docker).
- Avvia Azurite (es. `azurite --silent --location c:\azurite --debug c:\azurite\debug.log`).
- In `appsettings.Development.json` è già impostato  
  `"Storage:LocalConnectionString": "UseDevelopmentStorage=true"`  
  (default: porta blob 10000).

Se usi Azurite su porte diverse, imposta la connection string completa, ad es.:  
`DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=...;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;`

### 3. JWT

La chiave per i token in sviluppo è in **Authentication:DevelopmentKey** in `appsettings.Development.json`. Non serve KeyVault.

### 4. Stripe (opzionale)

Per testare pagamenti in locale, in `appsettings.Development.json` imposta:

- **Stripe:SecretKey**: chiave segreta test (es. `sk_test_...`).
- **Stripe:WebhookSecret**: secret del webhook di test (es. `whsec_...`) se usi Stripe CLI per inoltrare eventi in locale.

### 5. Mail (opzionale)

**MailOptions:Password** può restare vuoto in sviluppo; le mail non verranno inviate correttamente se non configuri un relay di test.

### 6. Frontend

**AppSettings:FrontendBaseUrl** è impostato a `http://localhost:5173` (Vue dev server). Modificalo se usi una porta diversa.

### 7. Dati di default (ruoli e piani subscription)

L’app ha bisogno di **ruoli** (Admin, Agency, Agent, User) e **piani di abbonamento** (Free, Basic, Pro, Premium e prepagati). Puoi inserirli in due modi:

#### Opzione A – Seed all’avvio (consigliato in sviluppo)

Con **SeedDefaultDataOnStartup: true** in `appsettings.Development.json` (default in Development), all’avvio l’applicazione:

- crea i ruoli se mancanti;
- verifica/crea il piano Free (trial);
- in Development, con **SeedSubscriptionPlansOnStartup: true**, inserisce tutti i piani e le relative feature.

Su un altro PC basta avviare l’API in Development dopo aver applicato le migration: i dati default vengono inseriti al primo avvio.

#### Opzione B – Script SQL manuale

Dopo `dotnet ef database update`, puoi eseguire lo script:

- **Scripts/SeedDefaultData.sql**

Esempio da riga di comando (sostituisci DB/user/password):

```bash
psql -h localhost -U postgres -d kuramahome_dev -f Scripts/SeedDefaultData.sql
```

Lo script è idempotente: inserisce ruoli e piani solo se mancanti, quindi puoi ripeterlo senza duplicati.

---

Verifica che in `launchSettings.json` i profili abbiano  
`"ASPNETCORE_ENVIRONMENT": "Development"`  
(già presente). Avviando l’API in Development verranno usati solo DB, storage e impostazioni locali definiti in `appsettings.Development.json`.
