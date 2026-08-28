# Pricing Instructions

This scope maps to `pricing`.

- Payment-path changes require idempotency and duplicate-submission tests per [RULE-1].
- Never emit sensitive pricing/patient payloads to logs per [RULE-5].
