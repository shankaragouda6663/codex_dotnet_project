# Workers Instructions

This scope maps to `workers`.

- Worker changes that alter payment-path behavior require idempotency and duplicate-submission tests per [RULE-1].
- New worker external calls must define explicit timeout and retry policy per [RULE-4].
- Do not log sensitive data per [RULE-5].
