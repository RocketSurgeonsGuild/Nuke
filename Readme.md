# Rocket Surgery - Nuke

Every good Rocket Surgeon needs multiple choices of build systems to pick from them for their best work.  This is an integration for the `Nuke` build system with some defaults for all Rocket Surgeon Repositories (or if you follow the same structure, you can use it too!)

# Status
<!-- badges -->
[![github-release-badge]][github-release]
[![github-license-badge]][github-license]
[![codecov-badge]][codecov]
[![codacy-badge]][codacy]
<!-- badges -->

<!-- history badges -->
| Azure Pipelines | AppVeyor |
| --------------- | -------- |
| [![azurepipelines-badge]][azurepipelines] | [![appveyor-badge]][appveyor] |
| [![azurepipelines-history-badge]][azurepipelines-history] | [![appveyor-history-badge]][appveyor-history] |
<!-- history badges -->

<!-- nuget packages -->
| Package | NuGet | MyGet |
| ------- | ----- | ----- |
| Rocket.Surgery.Nuke | [![nuget-version-6plqb7nwtdoa-badge]![nuget-downloads-6plqb7nwtdoa-badge]][nuget-6plqb7nwtdoa] | [![myget-version-6plqb7nwtdoa-badge]![myget-downloads-6plqb7nwtdoa-badge]][myget-6plqb7nwtdoa] |
| Rocket.Surgery.Nuke.DotNetCore | [![nuget-version-75gp65y/nhyw-badge]![nuget-downloads-75gp65y/nhyw-badge]][nuget-75gp65y/nhyw] | [![myget-version-75gp65y/nhyw-badge]![myget-downloads-75gp65y/nhyw-badge]][myget-75gp65y/nhyw] |
<!-- nuget packages -->

<!-- generated references -->
[github-release]: https://github.com/RocketSurgeonsGuild/Nuke/releases/latest
[github-release-badge]: https://img.shields.io/github/release/RocketSurgeonsGuild/Nuke.svg?logo=github&style=flat "Latest Release"
[github-license]: https://github.com/RocketSurgeonsGuild/Nuke/blob/master/LICENSE
[github-license-badge]: https://img.shields.io/github/license/RocketSurgeonsGuild/Nuke.svg?style=flat "License"
[codecov]: https://codecov.io/gh/RocketSurgeonsGuild/Nuke
[codecov-badge]: https://img.shields.io/codecov/c/github/RocketSurgeonsGuild/Nuke.svg?color=E03997&label=codecov&logo=codecov&logoColor=E03997&style=flat "Code Coverage"
[codacy]: https://www.codacy.com/app/RocketSurgeonsGuild/Nuke
[codacy-badge]: https://api.codacy.com/project/badge/Grade/d31c561959b34f35ae2d99979bfb239a "Codacy"
[azurepipelines]: https://rocketsurgeonsguild.visualstudio.com/Libraries/_build/latest?definitionId=31&branchName=master
[azurepipelines-badge]: https://img.shields.io/azure-devops/build/rocketsurgeonsguild/Libraries/31.svg?color=98C6FF&label=azure%20pipelines&logo=azuredevops&logoColor=98C6FF&style=flat "Azure Pipelines Status"
[azurepipelines-history]: https://rocketsurgeonsguild.visualstudio.com/Libraries/_build?definitionId=31&branchName=master
[azurepipelines-history-badge]: https://buildstats.info/azurepipelines/chart/rocketsurgeonsguild/Libraries/31?includeBuildsFromPullRequest=false "Azure Pipelines History"
[appveyor]: https://ci.appveyor.com/project/RocketSurgeonsGuild/Nuke
[appveyor-badge]: https://img.shields.io/appveyor/ci/RocketSurgeonsGuild/Nuke.svg?color=00b3e0&label=appveyor&logo=appveyor&logoColor=00b3e0&style=flat "AppVeyor Status"
[appveyor-history]: https://ci.appveyor.com/project/RocketSurgeonsGuild/Nuke/history
[appveyor-history-badge]: https://buildstats.info/appveyor/chart/RocketSurgeonsGuild/Nuke?includeBuildsFromPullRequest=false "AppVeyor History"
[nuget-6plqb7nwtdoa]: https://www.nuget.org/packages/Rocket.Surgery.Nuke/
[nuget-version-6plqb7nwtdoa-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Nuke.svg?color=004880&logo=nuget&style=flat-square "NuGet Version"
[nuget-downloads-6plqb7nwtdoa-badge]: https://img.shields.io/nuget/dt/Rocket.Surgery.Nuke.svg?color=004880&logo=nuget&style=flat-square "NuGet Downloads"
[myget-6plqb7nwtdoa]: https://www.myget.org/feed/rocket-surgeons-guild/package/nuget/Rocket.Surgery.Nuke
[myget-version-6plqb7nwtdoa-badge]: https://img.shields.io/myget/rocket-surgeons-guild/vpre/Rocket.Surgery.Nuke.svg?label=myget&color=004880&logo=nuget&style=flat-square "MyGet Pre-Release Version"
[myget-downloads-6plqb7nwtdoa-badge]: https://img.shields.io/myget/rocket-surgeons-guild/dt/Rocket.Surgery.Nuke.svg?color=004880&logo=nuget&style=flat-square "MyGet Downloads"
[nuget-75gp65y/nhyw]: https://www.nuget.org/packages/Rocket.Surgery.Nuke.DotNetCore/
[nuget-version-75gp65y/nhyw-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Nuke.DotNetCore.svg?color=004880&logo=nuget&style=flat-square "NuGet Version"
[nuget-downloads-75gp65y/nhyw-badge]: https://img.shields.io/nuget/dt/Rocket.Surgery.Nuke.DotNetCore.svg?color=004880&logo=nuget&style=flat-square "NuGet Downloads"
[myget-75gp65y/nhyw]: https://www.myget.org/feed/rocket-surgeons-guild/package/nuget/Rocket.Surgery.Nuke.DotNetCore
[myget-version-75gp65y/nhyw-badge]: https://img.shields.io/myget/rocket-surgeons-guild/vpre/Rocket.Surgery.Nuke.DotNetCore.svg?label=myget&color=004880&logo=nuget&style=flat-square "MyGet Pre-Release Version"
[myget-downloads-75gp65y/nhyw-badge]: https://img.shields.io/myget/rocket-surgeons-guild/dt/Rocket.Surgery.Nuke.DotNetCore.svg?color=004880&logo=nuget&style=flat-square "MyGet Downloads"
<!-- generated references -->

<!-- nuke-data
github:
  owner: RocketSurgeonsGuild
  repository: Nuke
azurepipelines:
  account: rocketsurgeonsguild
  teamproject: Libraries
  builddefinition: 31
appveyor:
  account: RocketSurgeonsGuild
  build: Nuke
myget:
  account: rocket-surgeons-guild
codacy:
  project: d31c561959b34f35ae2d99979bfb239a
-->