# Changelog

## Unreleased

### Added
- Prototype implementation for starting in disconnected mode

### Fixed
- Edge cases of option handling on reconnects

### Changed
- Changed IL mode to "text/x-cil" (mode itself is not included yet)
- Changed approach to handling of user actions in disconnected mode

## [2.0.5] - 2021-09-05

### Added
- Added IL language to the language list

## [2.0.5-preview-2021-06-26-1] - 2021-06-26

### Changed
- Not user-facing — updated completion flow to fail earlier, before attempting to send incorrect data

## [2.0.4] - 2021-02-28

### Fixed
- Fixed incorrect imports that caused failures in Webpack 5 ([#142](https://github.com/ashmind/mirrorsharp/issues/142))

## [2.0.3] - 2021-01-09

### Fixed
- Included missing TypeScript *.d.ts files (types) in the package

## [2.0.2] - 2020-09-12

### Added
- Show XML documentation in signature help.

## [2.0.1] - 2020-08-28

### Fixed
- Fixed position of autocomplete description tooltips ([#133](https://github.com/ashmind/mirrorsharp/issues/133)).