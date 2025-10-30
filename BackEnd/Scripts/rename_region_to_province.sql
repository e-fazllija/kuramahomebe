-- Script SQL per rinominare la colonna Region in Province nella tabella AspNetUsers
-- Questo script deve essere eseguito PRIMA della migration

-- 1. Rinominare la colonna Region in Province
ALTER TABLE "AspNetUsers" RENAME COLUMN "Region" TO "Province";

-- 2. Verificare che i dati siano stati spostati correttamente
-- SELECT "Province", COUNT(*) FROM "AspNetUsers" GROUP BY "Province";

-- NOTA: Se la colonna Province esiste già, decommentare le righe seguenti:
-- DROP COLUMN IF EXISTS "Province";
-- ALTER TABLE "AspNetUsers" RENAME COLUMN "Region" TO "Province";
