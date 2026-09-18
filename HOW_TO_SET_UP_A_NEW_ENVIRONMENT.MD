# How to Set Up a New Environment (Step by Step)

This is a simple walkthrough of the exact process used to set up the UAT environment — written so you can follow the same steps yourself next time, for example when setting up Production.

Wherever you see **UAT**, that's just the name of the environment we did first. For Production, you'd do the same steps but with Production's own resource names and its own real values (never reuse UAT's database, passwords, or secrets for Production).

---

## Before you start

You'll need two things handed to you first:

1. **Azure access** — someone who already manages the Azure subscription needs to give your account permission on the resource group. Ask them this:
   > *"Please give my account Contributor access to the [resource group name] resource group."*

2. **GitHub access** — you need an account with write access to the backend's GitHub repository, so you can push code and merge PRs.

You don't need to know anything about Azure or GitHub beforehand — just get access, then follow along.

---

## Step 1: Install the tools on your computer

Two small programs let you control everything from the command line instead of clicking around websites.

Open PowerShell and run:

```powershell
winget install --id Microsoft.AzureCLI -e
winget install --id GitHub.cli -e
```

Close and reopen PowerShell after this finishes.

---

## Step 2: Log in to Azure

```powershell
az login
```

This opens a browser window — sign in with the account that was given access. If a browser window doesn't work properly, use this instead, which gives you a code to type into a website:

```powershell
az login --use-device-code
```

Once logged in, tell it which subscription to use:

```powershell
az account set --subscription "<the subscription ID you were given>"
```

---

## Step 3: See what's already there (or create a new resource group)

If you're setting up a brand new environment, first create a resource group — think of this as a labeled folder that will hold everything:

```powershell
az group create --name "<your-new-resource-group-name>" --location "<region, e.g. centralindia>"
```

If you're just adding to an existing one, skip this — just look at what's inside it:

```powershell
az resource list --resource-group "<resource-group-name>" --output table
```

---

## Step 4: Set up the database (PostgreSQL)

This is where most of the app's data lives.

If a Postgres *server* doesn't exist yet, someone needs to create one (this is a bigger, one-time step — ask for help here if you're not sure). Once a server exists, creating a new database inside it is simple:

```powershell
az postgres flexible-server db create --resource-group "<resource-group-name>" --server-name "<postgres-server-name>" --name "<new-database-name>"
```

---

## Step 5: Set up MongoDB (using Azure Cosmos DB)

The app also needs a MongoDB-style database. Instead of signing up for MongoDB separately (which means a separate account and a separate bill), we use **Azure Cosmos DB**, which speaks the same language as MongoDB but lives inside your Azure subscription like everything else.

**One-time step, only needed the very first time your subscription ever uses Cosmos DB:**
```powershell
az provider register --namespace Microsoft.DocumentDB
```
If you get a permission error here, this specific step needs someone with higher-level Azure access — ask them to run it for you.

**Create the database:**
```powershell
az cosmosdb create --name "<new-cosmos-name>" --resource-group "<resource-group-name>" --locations regionName=<region> --capabilities EnableMongo --kind MongoDB --server-version 7.0 --default-consistency-level Session --enable-free-tier true
```

Then get its connection details from the Azure Portal (the website): open the resource you just created → **Settings → Connection Strings** → copy the value shown there. Keep it somewhere safe — you'll need it in Step 7.

---

## Step 6: Set up the App Service (this is what actually runs the app)

Create the plan (the compute power behind it) and the App Service itself:

```powershell
az appservice plan create --name "<plan-name>" --resource-group "<resource-group-name>" --is-linux --sku B1
az webapp create --name "<app-name>" --resource-group "<resource-group-name>" --plan "<plan-name>" --runtime "DOTNETCORE:10.0"
```

---

## Step 7: Tell the app its settings and secrets

The app needs to know things like: which database to use, and a secret key for logins. These are set as "Application Settings" on the App Service.

Because some of these values contain special characters that don't work well typed directly into a command, write them into a small file first:

Create a file called `settings.json` with content like this (fill in your real values):

```json
[
  { "name": "ASPNETCORE_ENVIRONMENT", "value": "Production", "slotSetting": false },
  { "name": "ConnectionStrings__DefaultConnection", "value": "Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require", "slotSetting": false },
  { "name": "ConnectionStrings__MongoConnection", "value": "<the mongo connection string from Step 5>", "slotSetting": false },
  { "name": "JWT_SECRET", "value": "<a new random secret — see below>", "slotSetting": false }
]
```

To generate a random secret for `JWT_SECRET`, run this in PowerShell and copy the result:
```powershell
$rng = New-Object System.Security.Cryptography.RNGCryptoServiceProvider
$bytes = New-Object byte[] 64
$rng.GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```
**Important: always generate a brand new one for each environment. Never reuse UAT's secret for Production.**

Then apply the file:
```powershell
az webapp config appsettings set --resource-group "<resource-group-name>" --name "<app-name>" --settings "@settings.json"
```

**Delete `settings.json` right after this** — it has real passwords in it, so don't leave it lying around.

---

## Step 8: Create the database tables

The database starts empty — you need to run a command that creates all the tables the app needs.

First, temporarily allow your own computer to talk to the database (it's normally locked down):
```powershell
curl https://api.ipify.org
```
This shows your computer's IP address. Then:
```powershell
az postgres flexible-server firewall-rule create --resource-group "<resource-group-name>" --server-name "<postgres-server-name>" --name "TempMyMachine" --start-ip-address "<your ip>" --end-ip-address "<your ip>"
```

Now run the actual command that sets up the tables:
```powershell
$env:ConnectionStrings__DefaultConnection = "<your Postgres connection string from Step 7>"
$env:JWT_SECRET = "placeholder"
dotnet ef database update --project GoVoylo.Infrastructure --startup-project GoVoylo.Api
```

When it's done, remove the temporary access you just gave yourself:
```powershell
az postgres flexible-server firewall-rule delete --resource-group "<resource-group-name>" --server-name "<postgres-server-name>" --name "TempMyMachine" --yes
```

---

## Step 9: Connect GitHub so code can be deployed automatically

Log in to GitHub from your computer:
```powershell
gh auth login --hostname github.com --git-protocol https --web
```
Follow the code/link it gives you, same idea as the Azure login.

Then get a deployment key from Azure and give it to GitHub:

1. In the Azure Portal, open your App Service → **Overview** → click **Download publish profile**.
2. Open the downloaded file in Notepad, select everything, copy it.
3. Go to your GitHub repo → **Settings → Secrets and variables → Actions → New repository secret**.
4. Give it a name like `AZURE_WEBAPP_PUBLISH_PROFILE_PROD` and paste the value in.

This lets GitHub deploy code to your App Service without you having to do it by hand every time.

---

## Step 10: Set up automatic deployment

Copy the existing UAT deployment file (`.github/workflows/deploy-uat.yml`) and adjust it for the new environment — mainly just change the App Service name and the secret name to match what you set up in Step 9, and change which branch it watches (e.g. `main` for Production instead of `uat`).

Commit this file and push it through a Pull Request — same as any other code change.

---

## Step 11: Deploy and test

Once the workflow file above is merged into the right branch, pushing any new code to that branch will automatically build and deploy it.

To check it worked, test with a request you know should fail cleanly:
```powershell
curl -X POST https://<your-app-name>.azurewebsites.net/api/auth/send-otp -H "Content-Type: application/json" -d "{}"
```
If you get back an error message saying something like *"The Email field is required"*, that's actually good news — it means the app started up correctly and is responding properly.

---

## If something doesn't work

- **"Not recognized as a command" errors in PowerShell** — this usually means a value has a special character (`|` or `&`) in it that PowerShell is misreading. Put the value in a file instead of typing it directly (like we did in Step 7).
- **"No subscriptions found" when logging into Azure** — your account doesn't have access yet. Go back to "Before you start" and ask for access.
- **"MissingSubscriptionRegistration" error** — someone with higher-level Azure access needs to run the one-time `az provider register` command from Step 5 for you.
- **GitHub Actions fails with a "billing issue" message even though nothing is actually wrong with billing** — this happened to us on UAT. It's a rare GitHub account hold, not a real billing problem. If you hit this, deploy manually instead for now (ask whoever set up UAT how they did the manual deploy), and separately contact GitHub Support to get the hold removed.

---

## The golden rule

**Never reuse a password, secret, or connection string from one environment in another.** Every environment (Dev, UAT, Production) should have its own separate database, its own separate secret key, and its own separate credentials. If Production ever needs setting up, treat every value in this guide as something to generate fresh — don't copy anything from UAT's actual settings.
