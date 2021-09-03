# Changelog

## [2.2.8] - 2021-09-03

### Fixed
- Internals visible for a new MirrorSharp.IL library

## [2.2.7] - 2021-05-07

### Fixed
- Error when force-requesting signature help for F#

## [2.2.7-preview-2021-07-08-1] - 2021-07-08-1
## [2.2.6] - 2021-06-22
## [2.2.5] - 2021-06-21
## [2.2.4] - 2021-06-20

### Changed
- Internal refactoring (no API changes)
- Internal diagnostics to investigate specific issues

## [2.2.3] - 2021-03-04
## [2.2.2] - 2020-12-17

### Changed
- Internal prototypes of new extensions (not exposed)

## [2.2.1] - 2020-09-12

### Added
- Show XML documentation in signature help.

## [2.2.0] - 2020-09-05

### Added
- Ability to add custom Analyzers, e.g: `SetupCSharp(o => o.AnalyzerReferences = o.AnalyzerReferences.Add(...))`.