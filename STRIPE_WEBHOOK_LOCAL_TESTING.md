# Testing Stripe Webhooks in Ambiente Locale

## Problema
In ambiente locale, Stripe non può inviare webhook direttamente perché l'endpoint `/api/StripeWebhookEvent/stripe` non è pubblicamente accessibile.

## Soluzione 1: Stripe CLI (Consigliata)

### Installazione
1. Scarica Stripe CLI da: https://stripe.com/docs/stripe-cli
   - **Windows**: Scarica l'eseguibile o usa `scoop install stripe`
   - **macOS**: `brew install stripe/stripe-cli/stripe`
   - **Linux**: Scarica il pacchetto .deb o .rpm

### Configurazione

1. **Login a Stripe**:
   ```bash
   stripe login
   ```
   Questo aprirà il browser per autorizzare l'accesso.

2. **Inoltrare i webhook al backend locale**:
   ```bash
   stripe listen --forward-to https://localhost:7267/api/StripeWebhookEvent/stripe
   ```
   
   **Nota**: La porta predefinita del backend è `7267`. Verifica in `Properties/launchSettings.json` se diversa.

3. **Copia il webhook signing secret**:
   Stripe CLI mostrerà un segreto simile a: `whsec_xxxxx`
   Questo va inserito in `appsettings.json`:
   ```json
   "Stripe": {
     "SecretKey": "sk_test_xxxxx",
     "PublishableKey": "pk_test_xxxxx",
     "WebhookSecret": "whsec_xxxxx"  // <-- Inserisci qui
   }
   ```

### Testing

1. **Avvia il backend** (se non già avviato)

2. **In un terminale separato, avvia Stripe CLI**:
   ```bash
   stripe listen --forward-to https://localhost:7267/api/StripeWebhookEvent/stripe
   ```

3. **Trigger eventi di test**:
   ```bash
   # Simulare un pagamento riuscito
   stripe trigger payment_intent.succeeded

   # Simulare un pagamento fallito
   stripe trigger payment_intent.payment_failed

   # Simulare creazione abbonamento
   stripe trigger customer.subscription.created

   # Simulare aggiornamento abbonamento
   stripe trigger customer.subscription.updated

   # Simulare cancellazione abbonamento
   stripe trigger customer.subscription.deleted
   ```

### Vantaggi
- ✅ Soluzione ufficiale Stripe
- ✅ Facile da usare
- ✅ Simula eventi realistici
- ✅ Non richiede modifiche al codice

---

## Soluzione 2: Ngrok (Alternativa)

### Installazione
1. Scarica ngrok da: https://ngrok.com/download
2. Registrati per ottenere un authtoken

### Uso

1. **Avvia ngrok**:
   ```bash
   ngrok http https://localhost:7267
   ```

2. **Copia l'URL pubblico** fornito da ngrok (es: `https://xxxx-xx-xxx.ngrok.io`)

3. **Configura il webhook su Stripe Dashboard**:
   - Vai su: https://dashboard.stripe.com/test/webhooks
   - Clicca "Add endpoint"
   - URL: `https://xxxx-xx-xxx.ngrok.io/api/StripeWebhookEvent/stripe`
   - Eventi: seleziona gli eventi necessari

4. **Copia il webhook signing secret** e aggiornalo in `appsettings.json`

### Svantaggi
- ⚠️ L'URL cambia ad ogni riavvio di ngrok (versione gratuita)
- ⚠️ Bisogna configurare manualmente il webhook su Stripe Dashboard

---

## Soluzione 3: Testing Manuale tramite Endpoint

Se vuoi solo testare la logica interna senza Stripe CLI, puoi creare un endpoint di test temporaneo.

### Eventi supportati dal controller

Il controller gestisce i seguenti eventi:
- `payment_intent.succeeded` - Pagamento riuscito
- `payment_intent.payment_failed` - Pagamento fallito
- `customer.subscription.created` - Abbonamento creato
- `customer.subscription.updated` - Abbonamento aggiornato
- `customer.subscription.deleted` - Abbonamento eliminato

---

## Testing in Produzione

Quando deploy in produzione:

1. **Configura il webhook su Stripe Dashboard**:
   - URL: `https://tuodominio.com/api/StripeWebhookEvent/stripe`
   - Eventi da selezionare:
     - `payment_intent.succeeded`
     - `payment_intent.payment_failed`
     - `customer.subscription.created`
     - `customer.subscription.updated`
     - `customer.subscription.deleted`

2. **Aggiorna il WebhookSecret in appsettings.json** con quello di produzione

3. **Verifica** che l'endpoint sia pubblicamente accessibile

---

## Note Importanti

1. L'endpoint `/api/StripeWebhookEvent/stripe` è decorato con `[AllowAnonymous]` per permettere a Stripe di chiamarlo
2. La sicurezza è garantita dalla verifica della firma Stripe
3. Gli eventi vengono salvati nel database per evitare duplicati
4. Ogni evento viene processato solo una volta grazie al controllo `IsEventProcessedAsync`
