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
   
   **Nota**: Sostituisci `7267` con la porta del tuo backend se diversa.

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
- `payment_intent.succeeded` - Pagamento riuscito (one-time; per subscription il Payment lo crea `invoice.paid`)
- `payment_intent.payment_failed` - Pagamento fallito
- `customer.subscription.created` - Abbonamento creato
- `customer.subscription.updated` - Abbonamento aggiornato
- `customer.subscription.deleted` - Abbonamento eliminato
- `invoice.paid` - Fattura pagata (**crea il record in Payments per le subscription**)
- `invoice.payment_failed` - Fattura non pagata

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
     - `invoice.paid` (necessario per creare i Payment delle subscription)
     - `invoice.payment_failed`

2. **Aggiorna il WebhookSecret in appsettings.json** con quello di produzione

3. **Verifica** che l'endpoint sia pubblicamente accessibile

---

## Perché non vedo i soldi in Payments (bonifico)?

Con **bonifico** (o altri metodi asincroni) il flusso è:

1. L’utente avvia il pagamento → su Stripe la subscription/fattura resta in attesa.
2. Dopo qualche minuto il bonifico viene confermato su Stripe → l’abbonamento su Stripe diventa pagato.
3. Stripe invia i webhook al **tuo** server (es. `invoice.paid`, `payment_intent.succeeded`).

Nel nostro backend:

- **La tabella Payments** viene aggiornata **solo** quando arriva il webhook **`invoice.paid`** (per le subscription) o `payment_intent.succeeded` (solo per pagamenti one-time).  
  Se quel webhook **non arriva** al server (vedi sotto), il record di pagamento **non viene mai creato** → in Payments non vedi i soldi anche se su Stripe è tutto pagato.

- **La tabella WebHookEvents** contiene **solo** gli eventi che il server **ha effettivamente ricevuto**.  
  Se non vedi eventi, significa che le richieste di Stripe **non stanno raggiungendo** il tuo backend.

### Quando i webhook non arrivano

- **Locale (senza Stripe CLI)**  
  Stripe invia i webhook a un URL pubblico. `localhost` non è raggiungibile da Stripe, quindi **nessun evento** arriva e non vedi né WebHookEvents né i Payment creati da `invoice.paid`.

- **Produzione**  
  URL del webhook sbagliato, server spento o errore (es. 500) nel momento in cui Stripe invia l’evento: Stripe non recapita l’evento al tuo server, quindi niente record in WebHookEvents e niente Payment.

### Cosa fare

1. **In locale**  
   Usa **Stripe CLI** e inoltra i webhook al backend (vedi sopra):
   ```bash
   stripe listen --forward-to https://localhost:7267/api/StripeWebhookEvent/stripe
   ```
   Così gli eventi (incluso `invoice.paid`) arrivano e vengono salvati in WebHookEvents e creati i Payment.

2. **In produzione**  
   - Controlla in [Stripe Dashboard → Webhooks](https://dashboard.stripe.com/webhooks) che l’endpoint sia l’URL del tuo server (es. `https://tuodominio.com/api/StripeWebhookEvent/stripe`).  
   - Controlla i log delle richieste fallite (risposta non 2xx) e assicurati che il server sia raggiungibile e che non restituisca errore quando Stripe invia `invoice.paid`.

3. **Recupero Payment già pagato su Stripe ma non in DB**  
   Se l’abbonamento è già attivo su Stripe ma in Payments non compare nulla (es. webhook perso), apri la pagina **Gestione abbonamento**. Il backend prova a **sincronizzare** l’ultima fattura pagata da Stripe e, se trova una invoice pagata senza corrispondente Payment in DB, crea il record. Così i soldi compaiono in Payments senza rifare il pagamento.

---

## Note Importanti

1. L'endpoint `/api/StripeWebhookEvent/stripe` è decorato con `[AllowAnonymous]` per permettere a Stripe di chiamarlo
2. La sicurezza è garantita dalla verifica della firma Stripe
3. Gli eventi vengono salvati nel database per evitare duplicati
4. Ogni evento viene processato solo una volta grazie al controllo `IsEventProcessedAsync`
5. Per le **subscription**, il record in **Payments** viene creato **solo** dal webhook **`invoice.paid`** (non da `payment_intent.succeeded`). Se `invoice.paid` non arriva, il pagamento non appare in Payments anche se su Stripe è pagato.