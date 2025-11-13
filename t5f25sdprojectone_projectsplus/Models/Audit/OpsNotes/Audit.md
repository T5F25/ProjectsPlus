### Audit Operational Notes

Brief summary
- Purpose: operational guidance for the audit_events table and supporting audit infrastructure — retention, partitioning, backups, archival, purge policies, access controls, and emergency procedures.  
- Principles: append-only primary store; immutable rows preferred; instrumented, auditable changes to schema and retention; minimal blockers for forensic queries; privacy and legal compliance.

---

### Retention and archival policy
- Default retention: keep hot rows in the primary DB for 90 days (query/analysis window) unless regulatory limits require longer.  
- Cold archival: monthly export of older rows (OccurredAt older than 90 days) to an immutable object store (e.g., S3/Blob) in compressed Parquet/NDJSON format with manifest files containing CorrelationId ranges and row counts.  
- Retention tiers: Hot (0–90 days), Warm (90–365 days), Cold (>365 days). Configure tier thresholds per legal requirements.  
- Verification: each archival job writes a manifest and checksum; verify row counts against source before deleting from primary.

---

### Partitioning and scaling guidance
- When to partition: implement time-range partitioning once table size and query patterns show sustained growth (e.g., >100M rows or >50k writes/sec).  
- Partition key recommendation: RANGE partition on OccurredAt (monthly partitions) for time-based retention and fast range scans.  
- Index strategy: keep narrow single-column indexes for common filters (correlation_id, actor_id, event_type, occurred_at) and add composite indexes only for proven query patterns; avoid wide indexes including large JSON columns.  
- Rollout approach: add partitioning in a staged migration: create new partitioned table, backfill recent window, switch writes, then backfill older data in background to minimize downtime.

---

### Purge, deletion, and GDPR
- Deletion policy: avoid ad-hoc deletes. Implement deletion as a controlled, auditable operation requiring dual-approval and an audit row documenting the purge action and reason.  
- GDPR/Subject-Access: for required removals, implement a targeted export-and-redact workflow:
  1. Locate all audit rows tied to subject (by ActorId, TargetId, correlation ids).  
  2. Export matching rows to a secure staging area with manifest.  
  3. Create redacted replacement rows if audit intent must be preserved (e.g., record that an event existed but redact personal payload).  
  4. Delete original rows from primary only after dual-approval and entry in an operations log.  
- Soft-delete alternative: tag rows as redacted (metadata_json.redacted = true) and store full payloads in encrypted archival store; prefer this when regulatory obligations allow.

---

### Backups, recovery, and migrations
- Backups: rely on database-provider native backups (point-in-time recovery enabled). For archival exports, keep at least two replicated copies (different storage classes/regions).  
- Recovery: maintain tested runbooks for restoring a point-in-time backup, running schema migrations in transactional stages, and re-applying seed/system types.  
- Migrations: treat audit schema changes conservatively — add columns with nullable defaults first, backfill in background, then flip constraints; avoid destructive migrations without an operational rollback plan and migration-run manifests.

---

### Access control, monitoring, and alerting
- Access: restrict write capability to service accounts only; read access for analysts via a separate analytics role and read-only replicas. Enforce role-based access and audit all schema or permission changes.  
- Monitoring: instrument metrics for write throughput, queue length (if writer uses buffer), write error rate, and archival job success/failure. Surface CorrelationId and SystemTypeId distribution charts for operational debugging.  
- Alerts: high-severity alerts on sustained write failures, archival job failures, or unexpected retention-table growth. Alert on dropped events from fire-and-forget queue.

---

### Emergency procedures and forensic exports
- Emergency export steps:
  1. Identify scope: correlation id(s), actor id(s), time range.  
  2. Snapshot relevant partitions or run targeted export query with streaming to compressed NDJSON/Parquet.  
  3. Produce signed manifest and checksum; copy to secure location; notify stakeholders.  
- Read-only emergency: if primary DB stressed, mount read-replica snapshot and run exports there to avoid impacting production.  
- Evidence preservation: for legal holds, mark affected partitions as preserved and suspend cleanup for tied retention windows.

---

### Operational governance and change control
- All retention/purge/migration changes require documented RFC, test plan, rollback plan, and at least two approvers.  
- Maintain an Ops runbook (versioned in repo) that includes scripts for archival, backfill, verification, and purge; require automatic dry-run mode for any deletion script.  
- Record every operation touching audit_events in an operations-audit log (separate store) that includes operator, rationale, manifest checksums, and dual-approval records.

---

### Quick checklist for engineers
- Do: write audit rows inside the same DB transaction for domain changes that require atomicity.  
- Do: avoid selecting payload_json/metadata_json in list queries; use projections.  
- Do: test archival restore and verify manifests as part of release acceptance.  
- Don’t: perform unilateral deletes of audit rows; require documented dual-approval and manifest verification.