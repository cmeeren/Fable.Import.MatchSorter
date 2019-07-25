# Fable.Import.MatchSorter

This package provides Fable bindings for [match-sorter](https://github.com/kentcdodds/match-sorter/).

## Installation

* Install the `match-sorter` npm package:
  * using npm: `npm install match-sorter`
  * using yarn: `yarn add match-sorter`

* Install the bindings:
  * using dotnet: `dotnet add package Fable.Import.MatchSorter`
  * using paket: `paket add Fable.Import.MatchSorter`

## Example usage

```f#
open Fable.Import.MatchSorter

// Without configuration

let sortedMatches =
  ["apple"; "banana"; "kiwi"]
  |> matchSort "a"

// More complex configuration using fluent pipe syntax

type Country =
  { Id: Guid
    Name: string
    Description: string }

let countryMatchSortOpts =
  MSOpts.empty
  |> MSOpts.addKey (fun c -> c.Name)
  |> MSOpts.addKeySpec (
      KeySpec.create (fun c -> c.Description)
      |> KeySpec.withThreshold Ranking.Contains
      |> KeySpec.withMaxRanking Ranking.Equal)
  |> MSOpts.withThreshold Ranking.Contains
  |> MSOpts.keepDiacritics
  
let matchSortCountries query (cs: Country list) : Country list =
  matchSortWith countryMatchSortOpts query cs
  
// You can also ensure the options are converted to native match-sort
// options only once by partially applying them to matchSortWith:

let matchSortCountries : string -> Country list -> Country list =
  matchSortWith countryMatchSortOpts
```

You can also access the “native” match-sorter bindings in the `Native` submodule.

Changelog
---------

#### 1.1.1 (2019-07-25)

- Added [femto](https://github.com/Zaid-Ajaj/Femto/) support
- Updated for match-sorter 4 (no API changes)

#### 1.1.0 (2019-05-02)

- Updated for Fable.Core 3 and match-sorter 3. No API changes.

#### 1.0.0 (2019-01-12)

* Initial release

## Deployment checklist

1. Make necessary changes to the code
2. Update the changelog
3. Update the version and release notes in the package info
4. Update the supported npm dependency versions for femto in the fsproj
5. Commit and tag the commit (this is what triggers deployment from  AppVeyor). For consistency, the tag should ideally be in the format `v1.2.3`.
6. Push the changes and the tag to the repo. If AppVeyor build succeeds, the package is automatically published to NuGet.
