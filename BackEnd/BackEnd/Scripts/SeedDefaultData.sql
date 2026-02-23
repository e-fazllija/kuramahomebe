-- =============================================================================
-- Script dati di default per KuramaHome
-- Eseguire su PostgreSQL dopo aver applicato le migration (dotnet ef database update).
-- Uso: psql -h localhost -U postgres -d kuramahome_dev -f Scripts/SeedDefaultData.sql
--      oppure da pgAdmin / DBeaver collegati al DB.
-- =============================================================================

BEGIN;

-- -----------------------------------------------------------------------------
-- 1. RUOLI (AspNetRoles) - inseriti solo se non esistono
-- -----------------------------------------------------------------------------
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
SELECT * FROM (VALUES
  ('11111111-1111-1111-1111-111111111101', 'Admin',  'ADMIN',  'a1b2c3d4-e5f6-7890-abcd-ef1111111101'),
  ('11111111-1111-1111-1111-111111111102', 'Agency', 'AGENCY', 'a1b2c3d4-e5f6-7890-abcd-ef1111111102'),
  ('11111111-1111-1111-1111-111111111103', 'Agent',  'AGENT',  'a1b2c3d4-e5f6-7890-abcd-ef1111111103'),
  ('11111111-1111-1111-1111-111111111104', 'User',   'USER',   'a1b2c3d4-e5f6-7890-abcd-ef1111111104')
) AS v("Id", "Name", "NormalizedName", "ConcurrencyStamp")
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = v."NormalizedName");

-- -----------------------------------------------------------------------------
-- 2. PIANI DI ABBONAMENTO (SubscriptionPlans) - solo se tabella vuota
-- -----------------------------------------------------------------------------
INSERT INTO "SubscriptionPlans" ("Id", "Name", "Description", "Price", "BillingPeriod", "Active", "CreationDate", "UpdateDate")
SELECT * FROM (VALUES
  (1,  'Free',   'Piano gratuito di benvenuto per nuovi utenti. Periodo di prova di 10 giorni con funzionalità base.', 0.00,   'monthly',   true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (2,  'Basic',  'Piano base per piccole agenzie immobiliari. Ideale per iniziare con funzionalità essenziali.', 19.00,   'monthly',   true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (3,  'Pro',    'Piano professionale per agenzie medie con più filiali. Include export dati e storage aumentato.', 39.00,   'monthly',   true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (4,  'Premium','Piano enterprise per grandi agenzie e gruppi immobiliari. Funzionalità illimitate e supporto prioritario.', 99.00,   'monthly',   true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (5,  'Basic 3 Months',  'Piano Basic prepagato valido 3 mesi.', 54.00,   'quarterly',  true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (6,  'Basic 6 Months',  'Piano Basic prepagato valido 6 mesi.', 102.00,  'semiannual', true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (7,  'Basic 12 Months', 'Piano Basic prepagato valido 12 mesi.', 182.00,  'annual',    true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (8,  'Pro 3 Months',    'Piano Pro prepagato valido 3 mesi.', 111.00,   'quarterly',  true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (9,  'Pro 6 Months',    'Piano Pro prepagato valido 6 mesi.', 210.00,   'semiannual', true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (10, 'Pro 12 Months',   'Piano Pro prepagato valido 12 mesi.', 374.00,   'annual',    true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (11, 'Premium 3 Months', 'Piano Premium prepagato valido 3 mesi.', 282.00, 'quarterly',  true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (12, 'Premium 6 Months', 'Piano Premium prepagato valido 6 mesi.', 535.00, 'semiannual', true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  (13, 'Premium 12 Months','Piano Premium prepagato valido 12 mesi.', 950.00, 'annual',    true, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC')
) AS v("Id", "Name", "Description", "Price", "BillingPeriod", "Active", "CreationDate", "UpdateDate")
WHERE NOT EXISTS (SELECT 1 FROM "SubscriptionPlans" LIMIT 1);

-- Aggiorna sequence se abbiamo inserito piani
SELECT setval(pg_get_serial_sequence('"SubscriptionPlans"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "SubscriptionPlans"));

-- -----------------------------------------------------------------------------
-- 3. FEATURES PER PIANO FREE (Id piano = 1)
-- -----------------------------------------------------------------------------
INSERT INTO "SubscriptionFeatures" ("SubscriptionPlanId", "FeatureName", "FeatureValue", "Description", "CreationDate", "UpdateDate")
SELECT 1, * FROM (VALUES
  ('max_agencies',   '1',   'Massimo 1 agenzia',           NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_agents',     '5',   'Massimo 5 agenti',             NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_properties', '20',  'Massimo 20 immobili',         NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_customers',  '50',  'Massimo 50 clienti',          NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_requests',  '100',  'Massimo 100 richieste',       NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('export_enabled', 'false', 'Export dati disabilitato', NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_exports',   '0',   'Nessun export disponibile',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('storage_limit',  '1',   'Storage limitato a 1 GB',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC')
) AS v("FeatureName", "FeatureValue", "Description", "CreationDate", "UpdateDate")
WHERE EXISTS (SELECT 1 FROM "SubscriptionPlans" WHERE "Id" = 1)
  AND NOT EXISTS (SELECT 1 FROM "SubscriptionFeatures" WHERE "SubscriptionPlanId" = 1 LIMIT 1);

-- -----------------------------------------------------------------------------
-- 4. FEATURES PER PIANO BASIC (Id piano = 2)
-- -----------------------------------------------------------------------------
INSERT INTO "SubscriptionFeatures" ("SubscriptionPlanId", "FeatureName", "FeatureValue", "Description", "CreationDate", "UpdateDate")
SELECT 2, * FROM (VALUES
  ('max_agencies',   '1',   'Massimo 1 agenzia',           NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_agents',     '5',   'Massimo 5 agenti',             NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_properties', '20',  'Massimo 20 immobili',         NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_customers',  '50',  'Massimo 50 clienti',          NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_requests',  '100',  'Massimo 100 richieste',       NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('export_enabled', 'false', 'Export dati disabilitato', NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_exports',   '0',   'Nessun export disponibile',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('storage_limit',  '1',   'Storage limitato a 1 GB',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC')
) AS v("FeatureName", "FeatureValue", "Description", "CreationDate", "UpdateDate")
WHERE EXISTS (SELECT 1 FROM "SubscriptionPlans" WHERE "Id" = 2)
  AND NOT EXISTS (SELECT 1 FROM "SubscriptionFeatures" WHERE "SubscriptionPlanId" = 2 LIMIT 1);

-- -----------------------------------------------------------------------------
-- 5. FEATURES PER PIANO PRO (Id piano = 3)
-- -----------------------------------------------------------------------------
INSERT INTO "SubscriptionFeatures" ("SubscriptionPlanId", "FeatureName", "FeatureValue", "Description", "CreationDate", "UpdateDate")
SELECT 3, * FROM (VALUES
  ('max_agencies',   '5',   'Massimo 5 agenzie',            NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_agents',     '25',  'Massimo 25 agenti',            NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_properties', '100', 'Massimo 100 immobili',        NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_customers',  '500', 'Massimo 500 clienti',         NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_requests',  '1000','Massimo 1000 richieste',       NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('export_enabled', 'true', 'Export dati abilitato',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_exports',   '10',  'Massimo 10 export al mese',    NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('storage_limit',  '10',  'Storage limitato a 10 GB',    NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC')
) AS v("FeatureName", "FeatureValue", "Description", "CreationDate", "UpdateDate")
WHERE EXISTS (SELECT 1 FROM "SubscriptionPlans" WHERE "Id" = 3)
  AND NOT EXISTS (SELECT 1 FROM "SubscriptionFeatures" WHERE "SubscriptionPlanId" = 3 LIMIT 1);

-- -----------------------------------------------------------------------------
-- 6. FEATURES PER PIANO PREMIUM (Id piano = 4)
-- -----------------------------------------------------------------------------
INSERT INTO "SubscriptionFeatures" ("SubscriptionPlanId", "FeatureName", "FeatureValue", "Description", "CreationDate", "UpdateDate")
SELECT 4, * FROM (VALUES
  ('max_agencies',   '10',       'Massimo 10 agenzie',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_agents',     '50',       'Massimo 50 agenti',      NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_properties', 'unlimited','Immobili illimitati',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_customers',  'unlimited','Clienti illimitati',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_requests',  'unlimited','Richieste illimitate',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('export_enabled', 'true',     'Export dati abilitato',  NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('max_exports',   'unlimited','Export illimitati',       NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'),
  ('storage_limit',  'unlimited','Storage illimitato',     NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC')
) AS v("FeatureName", "FeatureValue", "Description", "CreationDate", "UpdateDate")
WHERE EXISTS (SELECT 1 FROM "SubscriptionPlans" WHERE "Id" = 4)
  AND NOT EXISTS (SELECT 1 FROM "SubscriptionFeatures" WHERE "SubscriptionPlanId" = 4 LIMIT 1);

COMMIT;

-- Messaggio di conferma (opzionale, in psql vedrai il risultato)
DO $$
BEGIN
  RAISE NOTICE 'Seed completato: ruoli e piani di abbonamento inseriti (se mancanti).';
END $$;
