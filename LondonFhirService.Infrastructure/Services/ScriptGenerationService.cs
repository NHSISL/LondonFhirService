// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using ADotNet.Clients;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets.Tasks;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets.Tasks.SetupDotNetTaskV5s;

namespace LondonFhirService.Infrastructure.Services
{
    internal class ScriptGenerationService
    {
        private readonly ADotNetClient adotNetClient;

        public ScriptGenerationService() =>
            adotNetClient = new ADotNetClient();

        public void GenerateBuildScript(string branchName, string projectName, string dotNetVersion)
        {
            var githubPipeline = new GithubPipeline
            {
                Name = "Build",

                OnEvents = new Events
                {
                    Push = new PushEvent { Branches = [branchName] },

                    PullRequest = new PullRequestEvent
                    {
                        Types = ["opened", "synchronize", "reopened", "closed"]
                    }
                },

                Jobs = new Dictionary<string, Job>
                {
                    {
                        "build",
                        new Job
                        {
                            Name = "Build",
                            RunsOn = BuildMachines.UbuntuLatest,

                            EnvironmentVariables = new Dictionary<string, string>
                            {
                                {
                                    "NOTIFICATIONCONFIGURATIONS__APIKEY",
                                    "${{ secrets.NOTIFICATIONCONFIGURATIONS__APIKEY }}"
                                }
                            },

                            Steps = new List<GithubTask>
                            {
                                new CheckoutTaskV5
                                {
                                    Name = "Check out"
                                },

                                new SetupDotNetTaskV5
                                {
                                    Name = "Setup .Net",

                                    With = new TargetDotNetVersionV5
                                    {
                                        DotNetVersion = dotNetVersion
                                    }
                                },

                                new GithubTask
                                {
                                    Name = "Generate CI SQL Server password",
                                    Shell = "bash",
                                    Run =
                                        """
                                        password="$(openssl rand -base64 24)Aa1!"
                                        echo "::add-mask::$password"
                                        echo "CI_SQL_SERVER_PASSWORD=$password" >> "$GITHUB_ENV"
                                        """
                                },

                                new GithubTask
                                {
                                    Name = "Point CI database connections at Dockerized SQL Server",
                                    Shell = "pwsh",
                                    Run =
                                        """
                                        # Parsed as JSON rather than text-replaced: the LocalDB connection
                                        # strings are backslash-escaped on disk, and a raw-text match on the
                                        # escaped form is fragile. Parsing keeps each file's own Database=
                                        # name and only swaps in the container's server/credentials. This
                                        # runs before Restore/Build so the build's own content-copy carries
                                        # the patched values into bin/ - no need to patch build output too.
                                        $files = Get-ChildItem -Path . -Filter "appsettings.json" -Recurse
                                        foreach ($file in $files) {
                                          $json = Get-Content $file.FullName -Raw | ConvertFrom-Json
                                          $current = $json.ConnectionStrings.LondonFhirServiceConnectionString
                                          if ($current -and ($current -match "Database=([^;]+)")) {
                                            $databaseName = $matches[1]
                                            $password = $env:CI_SQL_SERVER_PASSWORD
                                            $json.ConnectionStrings.LondonFhirServiceConnectionString = (
                                              "Server=localhost,1433;Database=$databaseName;User Id=sa;" +
                                              "Password=$password;TrustServerCertificate=True;" +
                                              "MultipleActiveResultSets=true")
                                            $json | ConvertTo-Json -Depth 10 | Set-Content -Path $file.FullName
                                            Write-Host "Patched connection string in $($file.FullName)"
                                          }
                                        }
                                        """
                                },

                                new GithubTask
                                {
                                    Name = "Start SQL Server (Docker)",
                                    Run = "docker run -d --name ci-sql-server -e \"ACCEPT_EULA=Y\" " +
                                        "-e \"MSSQL_SA_PASSWORD=$CI_SQL_SERVER_PASSWORD\" -p 1433:1433 " +
                                        "mcr.microsoft.com/mssql/server:2022-latest"
                                },

                                new GithubTask
                                {
                                    Name = "Wait for SQL Server to be ready",
                                    Shell = "bash",
                                    Run =
                                        """
                                        sqlcmd_path=/opt/mssql-tools18/bin/sqlcmd
                                        for i in {1..30}; do
                                          if docker exec ci-sql-server "$sqlcmd_path" -C -S localhost -U sa \
                                              -P "$CI_SQL_SERVER_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; then
                                            echo "SQL Server is ready"
                                            exit 0
                                          fi
                                          echo "Waiting for SQL Server... ($i)"
                                          sleep 2
                                        done
                                        echo "::error::SQL Server did not become ready in time"
                                        exit 1
                                        """
                                },

                                new RestoreTask
                                {
                                    Name = "Restore",

                                    // LondonFhirService.Manage.Client.esproj (Microsoft.VisualStudio.JavaScript.Sdk)
                                    // only defines a TargetFrameworkVersion when '$(OS)'=='WINDOWS_NT' - by the SDK's
                                    // own admission, "nuget restore will not work on Mac/Linux by default" for it. The
                                    // CI solution filter below restores everything except that JS/TS project; local
                                    // Visual Studio/Rider users keep opening the full LondonFhirService.slnx unchanged.
                                    Run = "dotnet restore LondonFhirService.CI.slnf"
                                },

                                new DotNetBuildTask
                                {
                                    Name = "Build",
                                    Run = "dotnet build LondonFhirService.CI.slnf --no-restore"
                                },

                                new GithubTask
                                {
                                    Name = "Install EF Tools",
                                    Shell = "bash",
                                    Run =
                                        """
                                        dotnet tool install --global dotnet-ef
                                        echo "$HOME/.dotnet/tools" >> "$GITHUB_PATH"
                                        """
                                },

                                new GithubTask
                                {
                                    Name = "Deploy Database",
                                    Run = $"dotnet ef database update --project {projectName}/{projectName}.csproj " +
                                        $"--startup-project {projectName}/{projectName}.csproj",

                                    EnvironmentVariables = new Dictionary<string, string>
                                    {
                                        {
                                            "ConnectionStrings__LondonFhirServiceConnectionString",
                                            "Server=localhost,1433;Database=LondonFhirService;User Id=sa;" +
                                                "Password=${{ env.CI_SQL_SERVER_PASSWORD }};" +
                                                "TrustServerCertificate=True;MultipleActiveResultSets=true"
                                        }
                                    }
                                },

                                new TestTask
                                {
                                    Name = "Run Unit Tests",
                                    Shell = "pwsh",
                                    Run =
                                        """
                                        $projects = Get-ChildItem -Path . -Filter "*Tests.Unit*.csproj" -Recurse
                                        $failed = @()
                                        foreach ($project in $projects) {
                                          Write-Host "Running tests for: $($project.FullName)"
                                          dotnet test $project.FullName --no-build --verbosity normal
                                          if ($LASTEXITCODE -ne 0) { $failed += $project.Name }
                                        }
                                        if ($failed.Count -gt 0) {
                                          Write-Host "::error::Test projects failed: $($failed -join ', ')"
                                          exit 1
                                        }
                                        """
                                },

                                new TestTask
                                {
                                    Name = "Run Acceptance Tests",
                                    Shell = "pwsh",
                                    Run =
                                        """
                                        $projects = Get-ChildItem -Path . -Filter "*Tests.Acceptance*.csproj" -Recurse
                                        $failed = @()
                                        foreach ($project in $projects) {
                                          Write-Host "Running tests for: $($project.FullName)"
                                          dotnet test $project.FullName --no-build --verbosity normal
                                          if ($LASTEXITCODE -ne 0) { $failed += $project.Name }
                                        }
                                        if ($failed.Count -gt 0) {
                                          Write-Host "::error::Test projects failed: $($failed -join ', ')"
                                          exit 1
                                        }
                                        """
                                }
                            }
                        }
                    },
                    {
                        "add_tag",
                        new TagJobV2(
                            runsOn: BuildMachines.UbuntuLatest,
                            dependsOn: "build",
                            projectRelativePath: $"{projectName}/{projectName}.csproj",
                            githubToken: "${{ secrets.PAT_FOR_TAGGING }}",
                            branchName: branchName)
                        {
                            Name = "Tag and Release"
                        }
                    },
                }
            };

            string buildScriptPath = "../../../../.github/workflows/build.yml";
            string directoryPath = Path.GetDirectoryName(buildScriptPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            adotNetClient.SerializeAndWriteToFile(
                adoPipeline: githubPipeline,
                path: buildScriptPath);
        }

        public void GeneratePrLintScript(string branchName)
        {
            var githubPipeline = new GithubPipeline
            {
                Name = "PR Linter",

                OnEvents = new Events
                {
                    PullRequest = new PullRequestEvent
                    {
                        Types = ["opened", "edited", "synchronize", "reopened", "closed"]
                    }
                },

                Jobs = new Dictionary<string, Job>
                {
                    {
                        "label",
                        new LabelJobV3(runsOn: BuildMachines.UbuntuLatest)
                        {
                            Name = "Add Label(s)"
                        }
                    },
                    {
                        "requireIssueOrTask",
                        new RequireIssueOrTaskJobV2(excludedAuthors: "dependabot[bot]")
                        {
                            Name = "Require Issue Or Task Association",
                        }
                    },
                    {
                        "setAuthorAsPrAssignee",
                        new SetAuthorAsPrAssigneeJobV2(runsOn: BuildMachines.UbuntuLatest)
                        {
                            Name = "Set Author As PR Assignee",
                        }
                    },
                }
            };

            string buildScriptPath = "../../../../.github/workflows/prLinter.yml";
            string directoryPath = Path.GetDirectoryName(buildScriptPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            adotNetClient.SerializeAndWriteToFile(
                adoPipeline: githubPipeline,
                path: buildScriptPath);
        }
    }
}
