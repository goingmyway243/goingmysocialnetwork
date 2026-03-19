# SKILL: runbook-deploy-to-production

## Purpose
Provide step-by-step operational procedures for safely deploying GoingMy microservices to production environment with zero downtime and rollback capabilities.

## Use When
- Preparing release deployment
- Rolling out feature updates
- Emergency hotfixes
- Blue-green deployments

## Pre-Deployment Checklist

### Code Quality
- [ ] All unit tests pass: `dotnet test`
- [ ] Integration tests pass
- [ ] Code review approved
- [ ] No merge conflicts
- [ ] SonarQube quality gate passed

### Version Management
- [ ] Version bumped (semver)
- [ ] CHANGELOG.md updated
- [ ] Git tags created
- [ ] Release notes prepared

### Deployable
- [ ] Docker images built and tested
- [ ] Helm charts validated (if using K8s)
- [ ] Environmental configs prepared
- [ ] Database migrations tested

### Infrastructure
- [ ] Target environment checked
- [ ] Backup database taken
- [ ] SSL certificates valid
- [ ] DNS records correct

## Deployment Steps

### Phase 1: Pre-Deployment (1-2 hours before)

**Step 1.1: Notify Team**
```
Post in #deployments Slack channel:
"📢 Deployment scheduled for [TIME] UTC
Services: User Service v2.1, Post Service v1.5
Estimated duration: 15-20 minutes
Impact: None (rolling deployment)
Rollback procedure: [LINK]"
```

**Step 1.2: Verify Database Backups**
```bash
# List recent backups
aws rds describe-db-snapshots \
  --db-instance-identifier goingmy-prod

# Create fresh backup
aws rds create-db-snapshot \
  --db-instance-identifier goingmy-prod \
  --db-snapshot-identifier goingmy-prod-backup-$(date +%s)
```

**Step 1.3: Prepare Deployment Artifacts**
```bash
# Clone latest code
git clone --depth 1 --branch release/v2.1 \
  https://github.com/yourorg/goingmysocialnetwork.git

# Verify version
cat version.txt  # Should be: 2.1.0

# Build Docker images
docker build -t goingmy-user-service:2.1.0 \
  -f services/GoingMy.UserService/Dockerfile .

docker build -t goingmy-post-service:1.5.0 \
  -f services/GoingMy.PostService/Dockerfile .

# Tag for registry
docker tag goingmy-user-service:2.1.0 \
  registry.yourcompany.com/goingmy-user-service:2.1.0

# Push to registry
docker push registry.yourcompany.com/goingmy-user-service:2.1.0
```

### Phase 2: Database Migrations

**Step 2.1: Test Migrations on Staging**
```bash
# Run against staging database
ASPNETCORE_ENVIRONMENT=Staging \
dotnet ef database update \
  -s services/GoingMy.UserService/src/GoingMy.User.API

# Verify schema
aws rds-data execute-statement --sql "
  SELECT * FROM information_schema.tables 
  WHERE table_schema = 'public';
"
```

**Step 2.2: Run Migrations on Production**
```bash
# Create backup before migrations
aws rds create-db-snapshot \
  --db-instance-identifier goingmy-prod \
  --db-snapshot-identifier goingmy-prod-pre-migration-$(date +%s)

# Run migrations (with timeout)
timeout 300 dotnet ef database update \
  -s services/GoingMy.UserService/src/GoingMy.User.API \
  --connection "Server=prod-db.rds.amazonaws.com;..."

# Monitor progress
tail -f /var/log/migrations.log
```

### Phase 3: Service Deployment (Blue-Green Strategy)

**Step 3.1: Deploy to Green Environment**
```bash
# Update Kubernetes deployment
kubectl set image deployment/user-service-green \
  user-service=registry.yourcompany.com/goingmy-user-service:2.1.0 \
  --record

# Wait for rollout
kubectl rollout status deployment/user-service-green --timeout=5m

# Check pod status
kubectl get pods -l app=user-service-green

# Verify health
kubectl logs deployment/user-service-green -f --tail=50
```

**Step 3.2: Run Smoke Tests Against Green**
```bash
# Get green service endpoint
GREEN_URL=$(kubectl get service user-service-green -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')

# Run smoke tests
curl -X GET "$GREEN_URL/api/v1/health"
curl -X POST "$GREEN_URL/api/v1/users" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "smoketest@example.com",
    "name": "Smoke Test"
  }'

# If tests fail, STOP and investigate
if [ $? -ne 0 ]; then
  echo "❌ Smoke tests failed! Rolling back..."
  kubectl rollout undo deployment/user-service-green
  exit 1
fi
```

**Step 3.3: Switch Traffic to Green**
```bash
# Update service selector to point to green
kubectl patch service user-service -p \
  '{"spec":{"selector":{"deployment":"user-service-green"}}}'

# Verify traffic switch
kubectl get endpoints user-service

# Monitor for errors
kubectl logs -f deployment/user-service-green
```

### Phase 4: Post-Deployment Verification

**Step 4.1: Health Checks**
```bash
# Check service health
curl https://api.goingmy.com/api/v1/health

# Check database connectivity
curl https://api.goingmy.com/api/v1/users/1

# Check cache connectivity
curl https://api.goingmy.com/api/v1/cache-status

# Monitor error rates
curl https://monitoring.goingmy.com/metrics | grep http_requests_total
```

**Step 4.2: Real User Monitoring**
```bash
# Monitor APM
# Check Datadog/New Relic for:
# - Error rate
# - Request latency (p95, p99)
# - Database query times
# - Memory usage

# Wait 5-10 minutes for data collection
sleep 300

# Alert if anomalies detected
curl -X POST https://alerts.goingmy.com/api/check-deployment
```

**Step 4.3: User Impact Verification**
```bash
# Check login success rate
# Check post creation success rate
# Check no significant error spikes

# Query your monitoring tool:
# Errors in last 10 min should be < 0.5%
# P95 latency should be < 500ms
```

### Phase 5: Cleanup

**Step 5.1: Mark Deployment Complete**
```bash
# Tag in git
git tag -a "deployed/prod/v2.1" -m "Deployed to production"
git push origin "deployed/prod/v2.1"

# Update deployment log
echo "✅ User Service v2.1.0 deployed at $(date)" >> DEPLOYMENT.log
```

**Step 5.2: Decommission Old Environment**
```bash
# After 24 hours, delete blue environment
kubectl delete deployment user-service-blue
kubectl delete service user-service-blue

# Archive logs
aws s3 cp /var/log/deployment.log \
  s3://goingmy-logs/deployments/$(date +%Y-%m-%d)/
```

## Rollback Procedures

### Quick Rollback (< 2 minutes)

```bash
# Immediately switch traffic back to blue
kubectl patch service user-service -p \
  '{"spec":{"selector":{"deployment":"user-service-blue"}}}'

# Verify
curl https://api.goingmy.com/api/v1/health

# Alert team
curl -X POST https://alerts.goingmy.com/api/send-slack \
  -d '{"channel":"#deployments","message":"⚠️ Rollback to User Service v2.0.5"}'
```

### Full Rollback (Database Migration)

```bash
# 1. Switch traffic to previous version
kubectl patch service user-service -p \
  '{"spec":{"selector":{"deployment":"user-service-blue"}}}'

# 2. Rollback database migration
dotnet ef database update PreviousMigrationName \
  -s services/GoingMy.UserService/src/GoingMy.User.API

# 3. Verify data integrity
dotnet ef dbcontext validate

# 4. Confirm with team
# Manual verification that data is correct
```

## Monitoring During Deployment

```bash
# Window 1: Watch pod status
kubectl get pods -w

# Window 2: Watch service logs
kubectl logs -f deployment/user-service-green

# Window 3: Watch metrics
watch 'kubectl top pods -l app=user-service'

# Window 4: Monitor errors
tail -f /var/log/app-errors.log
```

## Communication Plan

**T-0 (Scheduled Time):**
- "Deployment starting in 5 minutes"

**T+0:**
- "🚀 Deployment in progress"
- "Migrating database..."
- "Rolling out new containers..."

**T+10:**
- "✅ Green environment ready"
- "Running smoke tests..."

**T+12:**
- "↔️ Switching traffic"
- "Monitoring for issues..."

**T+15:**
- "✅ Deployment complete!"
- "All services healthy"
- "No user impact detected"

## Critical Metrics to Monitor

- **Error Rate**: < 0.5%
- **P95 Latency**: < 500ms
- **CPU Usage**: < 70%
- **Memory Usage**: < 80%
- **Database Connections**: < 80% pool
- **Queue Depth**: < 100 messages

## Emergency Contacts

- **On-Call Engineer**: [Phone number]
- **Tech Lead**: [Slack @mention]
- **Database Admin**: [Email]
- **Infrastructure Team**: #infrastructure Slack

## Quality Criteria
- Zero downtime deployment
- Automatic rollback on failure
- Database migration tested
- All health checks passing
- No increase in error rate

## Verification Checklist
- [ ] Pre-deployment checklist complete
- [ ] Database backup taken
- [ ] Migrations tested
- [ ] Docker images pushed
- [ ] Green environment healthy
- [ ] Traffic switched successfully
- [ ] Post-deployment tests pass
- [ ] Monitoring shows no issues
- [ ] Team notified

## References
- Infrastructure Documentation
- Kubernetes Deployment Guide
- Database Migration Guide

## Changelog
- v1.0: Production deployment runbook
- v1.1: Added blue-green strategy
