# Document Lifecycle

1. **MANUAL_SOURCE**: Hand-written documentation representing active guidelines, architecture, or business rules.
2. **GENERATED_ARTIFACT**: Artifacts created by scripts, toolings, or build processes (e.g. CSV reports, build logs). They should reside in `docs/artifacts`.
3. **EXPORTED_COPY**: Read-only copies (PDF/DOCX) exported from source. Must reference the source.
4. **ARCHIVED**: Documents that are no longer the active source of truth. Move to `docs/90-archive`. Provide a reason and the path to the current source of truth.
5. **UNVERIFIED**: Documents lacking proper classification. Avoid using this state for new documentation.
