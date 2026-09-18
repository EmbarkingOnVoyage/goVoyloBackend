# GoVoylo Backend — UAT Deployment Guide

This document explains, step by step, how the UAT environment is set up and how to deploy new code to it. It's written for someone with **no prior Azure or deployment experience** — every command is spelled out, and every section explains *why*, not just *how*.

---

## 1. What UAT actually is

UAT (User Acceptance Testing) is a small copy of the production environment, hosted entirely on Azure, that lets the team test real features against real (but non-production) data before anything goes live.

Everything for UAT lives in one **Azure Resource Group** — think of a resource group as a labeled folder that holds all the pieces belonging to one environment.

| Setting | Value |
|---|---|
| Subscription | `voylo-govo-subscription` (`02caba9b-ebc7-485f-84b9-94c3cc36ce74`) |
| Resource Group | `govo-u-centralindia-rg` |
| Region | Central India |

### The pieces inside it

| Resource name | What it is | What it's for |
|---|---|---|
| `govo-api-u-app` | Azure App Service (Linux, .NET 10) | Runs the actual backend API |
| `govo-api-u-asp` | App Service Plan | The compute plan/tier behind the App Service |
| `govo-db-u-psql` | PostgreSQL Flexible Server | The main relational database |
| `govo-db-u-mongo` | Azure Cosmos DB (MongoDB API), Free Tier | MongoDB-compatible database (used for activity logs) |
| `govo-secrets-u-kv` | Key Vault | Reserved for secrets (not yet wired up — see [Known Gaps](#9-known-gaps--future-improvements)) |
| `govo-api-u-appi` + `govo-monitor-u-law` | Application Insights + Log Analytics | Monitoring and logs |
| `govodataust` | Storage Account | Blob/file storage |

You do **not** need to create these from scratch for day-to-day deployment — they already exist. Section 8 covers what to do if you ever need to stand up a *new* environment from zero.

---

## 2. One-time setup (do this once, on any machine you'll deploy from)

### 2.1 Install the tools

**Azure CLI** — lets you manage Azure resources from the command line.
```powershell
winget install --id Microsoft.AzureCLI -e
```

**GitHub CLI** — lets you manage GitHub (PRs, secrets, workflow runs) from the command line.
```powershell
winget install --id GitHub.cli -e
```

After installing either tool, close and reopen your terminal so it picks up the new `PATH`.

> **PowerShell gotcha:** the Azure CLI is a `.cmd` wrapper, and PowerShell sometimes mis-parses arguments containing `|` or `&` (both appear in real Azure values, e.g. runtime stack strings and MongoDB connection strings). If a command with one of those characters fails with a strange "not recognized" error, either:
> - Prefix the command with `--%` (tells PowerShell to stop interpreting anything after it literally), or
> - Write the values to a JSON file and pass it with `--settings @file.json` instead of inline arguments (this is what we do for App Service settings in Section 5.4).

### 2.2 Log in to Azure

```powershell
az login --tenant 3c843d1a-c23f-4055-a90f-e73fd4630498
```

This opens a browser window for you to sign in. If that fails (e.g. it can't complete the redirect), use the more reliable device-code method instead:

```powershell
az login --tenant 3c843d1a-c23f-4055-a90f-e73fd4630498 --use-device-code
```

This prints a code and a URL (`https://login.microsoft.com/device`). Open that URL in any browser, enter the code, and sign in there.

Once logged in, set the correct subscription as active:
```powershell
az account set --subscription "02caba9b-ebc7-485f-84b9-94c3cc36ce74"
```

**You'll need an Azure account with at least *Contributor* access on `govo-u-centralindia-rg`.** If you get a "no subscriptions found" or authorization error, ask an existing team member (or whoever manages the `voylo-govo-subscription` subscription) to grant you access — see the exact request template in [Section 10](#10-request-templates).

### 2.3 Log in to GitHub

```powershell
gh auth login --hostname github.com --git-protocol https --web
```

This also gives you a device code and URL — same pattern as above. Sign in with whichever GitHub account has write access to `EmbarkingOnVoyage/goVoyloBackend`.

Verify it worked:
```powershell
gh auth status
```

---

## 3. The branch flow

Code moves through branches in this order before reaching UAT:

```
feature/xyz  →  dev  →  uat  (deployed)
```

- Individual feature branches get merged into `dev` as work is completed.
- When you're ready to test a batch of changes on UAT, open a Pull Request from `dev` into `uat`.
- Merging that PR is what triggers a deployment (once GitHub Actions is working — see [Section 7](#7-known-issue-github-actions-is-currently-blocked)).

**Always deploy via a PR into `uat`, never by pushing directly to it.** This keeps a visible history of what went to UAT and when, and gives someone a chance to sanity-check the diff before it goes live.

---

## 4. Deploying an update (the normal day-to-day process)

Assuming GitHub Actions is working (see Section 7 if it isn't):

1. Make sure your changes are merged into `dev`.
2. Open a PR from `dev` into `uat`:
   ```powershell
   gh pr create --repo EmbarkingOnVoyage/goVoyloBackend --base uat --head dev --title "Merge dev into uat" --body "Deploying latest dev changes to UAT"
   ```
3. Have someone review and merge it (via the GitHub website, or `gh pr merge <number> --merge`).
4. That merge automatically triggers the `Deploy Backend to UAT` GitHub Actions workflow (`.github/workflows/deploy-uat.yml`), which builds the app and pushes it to `govo-api-u-app`.
5. Watch it run under the repo's **Actions** tab, or:
   ```powershell
   gh run list --repo EmbarkingOnVoyage/goVoyloBackend --workflow deploy-uat.yml --limit 3
   ```

That's it — no manual Azure steps needed once this is working.

---

## 5. Manual deployment (fallback method)

Use this if GitHub Actions is unavailable for any reason (see Section 7). It deploys directly from your own machine instead of through CI.

### 5.1 Get the exact code you want deployed

```powershell
git fetch origin uat
git worktree add ../uat-deploy origin/uat --detach
cd ../uat-deploy
```

Using a separate [worktree](https://git-scm.com/docs/git-worktree) like this means you don't disturb whatever branch you currently have checked out in your main working folder.

### 5.2 Build and publish

```powershell
dotnet restore GoVoyloBackend.slnx
dotnet build GoVoyloBackend.slnx --configuration Release --no-restore
dotnet publish GoVoylo.Api/GoVoylo.Api.csproj --configuration Release --no-build --output ./publish
```

Confirm this finishes with **"0 Error(s)"** before continuing.

### 5.3 Package it

```powershell
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force
```

### 5.4 Deploy to Azure

```powershell
az webapp deploy --resource-group "govo-u-centralindia-rg" --name "govo-api-u-app" --src-path ./deploy.zip --type zip
```

This takes about a minute. A successful run ends with `"status": "RuntimeSuccessful"`.

### 5.5 Clean up

```powershell
cd ..
git worktree remove ./uat-deploy --force
```

---

## 6. Verifying a deployment worked

The app doesn't have a public root page, so a plain browser visit to `https://govo-api-u-app.azurewebsites.net/` will correctly show a 404 — that's normal, not a failure.

**Don't test `/health`** — it's deliberately restricted to internal Azure traffic only (load balancer probes), so it will always return `403 Forbidden` from your own machine. That's by design, not a bug.

The most reliable smoke test is hitting a real endpoint with a payload you know is invalid — a `400 Bad Request` with a proper validation-error JSON body proves the whole pipeline (routing, database connections, JWT config) started up correctly. A `500` error or the request hanging means something is actually broken.

```powershell
curl -X POST https://govo-api-u-app.azurewebsites.net/api/auth/send-otp -H "Content-Type: application/json" -d "{}"
```

Expected response: `400` with `"The Email field is required."` — this means the app is healthy.

For a full real-world test, point the mobile app at the UAT URL temporarily:

1. Open `packages/api/src/authConfig.ts` in the frontend repo.
2. Temporarily change the `AUTH_BASE_URL` export to `'https://govo-api-u-app.azurewebsites.net'`.
3. Reload the app and try a real flow (e.g. a flight search).
4. **Revert the file afterward** — don't commit this change; it's for testing only.

---

## 7. Known issue: GitHub Actions is currently blocked

As of this writing, every GitHub Actions run on this repo fails immediately with:

> *"The job was not started because your account is locked due to a billing issue."*

**This is misleading wording — it is not really about billing.** We confirmed:
- The repo is **public**, so Actions minutes are free and unlimited regardless of any billing plan.
- The organization's billing page (Settings → Billing and licensing → Overview, under the EmbarkingOnVoyage org) shows a clean $0 balance on the Free plan, no payment issues.
- The error persists no matter which account (including the org Owner's) triggers the run.

This points to an automatic **account/organization hold** (likely a fraud-prevention or new-account review flag) rather than an actual unpaid bill. This can only be lifted by **GitHub Support** — there is nothing to fix in the repo's or org's own settings.

**Action needed:** someone with access to the EmbarkingOnVoyage GitHub account should contact GitHub Support (https://support.github.com) and reference this exact error message, asking them to review/lift the account hold on Actions.

**Until it's resolved:** use the manual deployment method in Section 5 for every UAT deployment.

---

## 8. Setting up a brand-new environment from scratch

You'd only need this if standing up an entirely new environment (e.g. a second UAT, or a new SIT/staging environment). Skip this section for normal deployments.

### 8.1 Prerequisites
- An Azure resource group already created (ask your Azure admin, or create one with `az group create`).
- The `Microsoft.DocumentDB` resource provider registered on the subscription (only needed once per subscription, ever — see 8.4). If you get a `MissingSubscriptionRegistration` error, ask your Azure admin to run:
  ```powershell
  az provider register --namespace Microsoft.DocumentDB
  ```
  (This requires subscription-level access, not just resource-group access.)

### 8.2 Create the database
```powershell
az postgres flexible-server db create --resource-group "<rg-name>" --server-name "<postgres-server-name>" --name "<new-db-name>"
```
(This assumes the Postgres *server* itself already exists — creating a whole new server is a bigger, separate step.)

### 8.3 Create the MongoDB-compatible database (Cosmos DB)

We use Cosmos DB's MongoDB API instead of MongoDB Atlas, because it's a native Azure resource (same subscription, same bill, no third-party account) rather than a separate external service.

```powershell
az cosmosdb create --name "<new-cosmos-account-name>" --resource-group "<rg-name>" --locations regionName=<region> --capabilities EnableMongo --kind MongoDB --server-version 7.0 --default-consistency-level Session --enable-free-tier true
```

> `--enable-free-tier true` gives you 1000 RU/s + 25GB storage free forever — plenty for a UAT-sized workload, and only one such account is allowed per subscription. Use `false` if that free tier is already claimed by another account.

Get its connection string (via the Azure Portal: open the Cosmos DB account → **Settings → Connection Strings** → copy the Primary Connection String — this is treated as sensitive, so it's easiest to grab it yourself rather than scripting it):

Insert a database name into the connection string's path before using it, e.g.:
```
mongodb://<account>:<key>@<account>.mongo.cosmos.azure.com:10255/<your-db-name>?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@<account>@
```

### 8.4 Create the App Service

```powershell
az appservice plan create --name "<plan-name>" --resource-group "<rg-name>" --is-linux --sku B1
az webapp create --name "<app-name>" --resource-group "<rg-name>" --plan "<plan-name>" --runtime "DOTNETCORE:10.0"
```

If `DOTNETCORE:10.0` isn't accepted, check what's currently available:
```powershell
az webapp list-runtimes --os linux
```

### 8.5 Configure application settings

The backend needs these settings (see `GoVoylo.Api/appsettings.json` for the full/current list — it changes over time, so treat that file as the source of truth, not this document):

| Setting | Purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | Set to a name that is **not** `Development` (e.g. `UAT`), so the app doesn't try to load dev-only config |
| `ConnectionStrings__DefaultConnection` | Postgres connection string |
| `ConnectionStrings__MongoConnection` | Cosmos DB / MongoDB connection string |
| `JWT_SECRET` | A random secret — **generate a fresh one per environment, never reuse across environments** (see command below) |
| `FlyshopSettings__*` | Flyshop API credentials (ask a team lead — these can typically be reused across non-production environments since they point at Flyshop's own UAT server) |
| `SmtpSettings__*` | Email sending credentials |

Generate a JWT secret:
```powershell
$rng = New-Object System.Security.Cryptography.RNGCryptoServiceProvider
$bytes = New-Object byte[] 64
$rng.GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

Because these values contain special characters (`&`, `=`, `@`) that break when passed as command-line arguments, write them to a JSON file first and apply from there:

```json
[
  { "name": "ASPNETCORE_ENVIRONMENT", "value": "UAT", "slotSetting": false },
  { "name": "ConnectionStrings__DefaultConnection", "value": "Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require", "slotSetting": false },
  { "name": "ConnectionStrings__MongoConnection", "value": "mongodb://...", "slotSetting": false },
  { "name": "JWT_SECRET", "value": "...", "slotSetting": false }
]
```

```powershell
az webapp config appsettings set --resource-group "<rg-name>" --name "<app-name>" --settings "@settings.json"
```

**Delete that JSON file immediately afterward** — it contains real secrets and shouldn't be left lying around or committed anywhere.

### 8.6 Run database migrations

The Postgres firewall only allows connections from Azure services by default — you'll need to temporarily allow your own machine's IP to run migrations from your laptop:

```powershell
# Find your public IP
curl https://api.ipify.org

# Temporarily allow it
az postgres flexible-server firewall-rule create --resource-group "<rg-name>" --server-name "<postgres-server-name>" --name "TempDevMachine" --start-ip-address "<your-ip>" --end-ip-address "<your-ip>"
```

Then run the migration, pointing at the target database via environment variables (so you don't have to edit any config files):

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require"
$env:JWT_SECRET = "placeholder-not-used-for-migrations"
dotnet ef database update --project GoVoylo.Infrastructure --startup-project GoVoylo.Api
```

**Remove the temporary firewall rule once done:**
```powershell
az postgres flexible-server firewall-rule delete --resource-group "<rg-name>" --server-name "<postgres-server-name>" --name "TempDevMachine" --yes
```

### 8.7 Set up the GitHub Actions deployment credential

1. In the Azure Portal, open the App Service → **Overview** → **Download publish profile**.
2. Open the downloaded file in Notepad, copy its full contents.
3. In GitHub: repo → **Settings → Secrets and variables → Actions → New repository secret**.
4. Name it something like `AZURE_WEBAPP_PUBLISH_PROFILE_<ENV>`, paste the value, save.
5. Reference that secret name in a workflow file modeled on `.github/workflows/deploy-uat.yml`.

---

## 9. Known gaps / future improvements

- **Key Vault isn't wired up yet.** Secrets currently live as plain (though encrypted-at-rest) App Service Application Settings. `govo-secrets-u-kv` exists but isn't used — moving secrets there would need someone with Key Vault RBAC permissions (`Key Vault Secrets Officer` role), which is separate from general resource-group Contributor access.
- **No automatic migrations on deploy.** Schema changes currently require manually running `dotnet ef database update` (Section 8.6) — there's no `Database.Migrate()` call on startup. Adding one would remove this manual step for future deployments, at the cost of needing care around concurrent-instance migration races (a non-issue at UAT's current single-instance scale).
- **GitHub Actions is blocked** — see Section 7. This is the most impactful gap; once resolved, Section 4's fully-automated flow becomes the only method anyone needs.

---

## 10. Request templates

**Asking for Azure access:**
> Please grant `<your-email>` the **Contributor** role on the resource group `govo-u-centralindia-rg` (subscription `02caba9b-ebc7-485f-84b9-94c3cc36ce74`).
>
> Via Portal: Resource groups → govo-u-centralindia-rg → Access control (IAM) → + Add → Add role assignment → Role: Contributor → select the user → Review + assign.

**Asking for a resource provider to be registered (subscription-level action):**
> Please register the `Microsoft.DocumentDB` resource provider on the `voylo-govo-subscription` subscription (`02caba9b-ebc7-485f-84b9-94c3cc36ce74`). One-time, no-cost, no new resources created.
>
> Via Portal: Subscriptions → voylo-govo-subscription → Settings → Resource providers → search "DocumentDB" → select it → Register.
> Via CLI: `az provider register --namespace Microsoft.DocumentDB`
