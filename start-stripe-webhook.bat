@echo off
echo ============================================
echo   STRIPE WEBHOOK LISTENER - Kurama Home
echo ============================================
echo.
echo Porta Backend: https://localhost:7267
echo Endpoint: /api/StripeWebhookEvent/stripe
echo.
echo Avvio Stripe CLI...
echo.

stripe listen --forward-to https://localhost:7267/api/StripeWebhookEvent/stripe

pause



