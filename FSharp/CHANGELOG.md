# Changelog

## [Unreleased]

### Added
- Added IFSharpSession.CompileAsync

## Changed
- Changed required .NET Framework version to 4.6.2 as required by dependent packages

### Removed
- (Breaking) Removed MirrorSharpFSharpOptions.Debug (F# PDB generation is not currently supported by MirrorSharp)
- (Breaking) Removed FSharpProjectOptionsExtensions.WithOtherOptionDebug

## [2.0.1] - 2024-06-01

### Added
- F# 8 support by @psfinaki (additional thanks to previous F# 7 upgrade PRs by @vzarytovskii and @rstm-sf)

## [2.0.0] - 2022-08-13

### Changed
- (Breaking) Renamed FSharpVirtualFile.Name to Path

### Fixed
- Multiple edge cases in the file management and error reporting

### Removed
- (Breaking) Removed FSharpVirtualFile.Stream

## [1.0.0] - 2022-04-04
## [1.0.0-test-2021-04-02-1] - 2022-04-02
## [1.0.0-test-2021-04-01-1] - 2022-04-01

### Changed
- Updated to support MirrorSharp.Common 3.0.0

## [0.23.0] - 2021-10-22

### Fixed
- Fixed InvalidOperationException in F# completion with function types in scope

### Added
- Added proper completion icon for local values

## [0.22.0] - 2021-08-04

### Fixed
- Fixed WorkSessionExtensions.IsFSharp failing if session is a mock

## [0.21.0] - 2021-06-23

### Added
- Updated to support FSharp.Compiler.Service version 40 by @baronfel

## [0.20.0] - 2020-12-21

### Added
- F# 5 support by @baronfel

## [0.19.0] - 2020-12-17

### Changed
- Updated to support MirrorSharp.Common 2.2.2