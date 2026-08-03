# Changelog

Notable changes to OrpheusORM, generated from the commits on `master` by
[`.github/workflows/changelogWorkFlow.yml`](.github/workflows/changelogWorkFlow.yml)
when a release is tagged.

Releases before 2.1.0 predate this file — see the
[tags](https://github.com/mobilotronic/OrpheusORM/tags) for their history.

<!-- new releases are inserted below this line -->

## [2.1.0] - 2026-08-03

### Breaking Changes

- ServiceManager and ConfigurationManager are removed. Replace ServiceManager.ServiceProvider with the provider you already build, ServiceManager.Resolve<T>() with IOrpheusDatabase or your own container, and InitializeOrpheusConfiguration() with services.AddOrpheusConfiguration(). IOrpheusDatabase gains ServiceProvider, LoggerFactory and KeyGenerator, which breaks third-party implementations of the interface. OrpheusDemoApi - New ASP.NET Core Minimal API on PostgreSQL, in the solution so CI compiles it. Shows schema-from-models, the Module system persisting a master-detail aggregate in one Save(), async CRUD, BatchSize, and raw SQL projections. Creates its own database on first run. * feat: change log generation
- ServiceManager and ConfigurationManager are removed. Replace ServiceManager.ServiceProvider with the provider you already build, ServiceManager.Resolve<T>() with IOrpheusDatabase or your own container, and InitializeOrpheusConfiguration() with services.AddOrpheusConfiguration(). IOrpheusDatabase gains ServiceProvider, LoggerFactory and KeyGenerator, which breaks third-party implementations of the interface. OrpheusDemoApi - New ASP.NET Core Minimal API on PostgreSQL, in the solution so CI compiles it. Shows schema-from-models, the Module system persisting a master-detail aggregate in one Save(), async CRUD, BatchSize, and raw SQL projections. Creates its own database on first run.

### Features

- (#7)

### Other Changes

- Feature/orpheus changelog generation (#8)

**Full Changelog**: https://github.com/mobilotronic/OrpheusORM/compare/v2.0.0...v2.1.0
