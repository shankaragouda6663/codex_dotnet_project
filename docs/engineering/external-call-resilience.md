# External Call Resilience

## Required

- New external calls must define explicit timeout and retry policy.
- Avoid unbounded retries and retry storms.
- Distinguish retryable vs non-retryable failures.

## .NET Notes

- Prefer centrally configured `HttpClient` policies and explicit timeout settings.
- Keep retry counts and timeout values documented next to call registration.
