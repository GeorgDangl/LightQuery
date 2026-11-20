# NgLightqueryWorkspace

This workspace builds the `ng-lightquery` library with Angular CLI 22.3 and TypeScript 6.0.

## Requirements

Use Node.js 22.22.3 or later in the 22.x line, 24.15.0 or later in the 24.x line, or 26.0.0 or later.
Run `npm ci` in this directory to install dependencies.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `npm run buildLibrary` for a production build using the `@angular/build:library` builder.
The publishable package will be stored in `dist/ng-lightquery/`.

## Running unit tests

Run `npm test` to execute the unit tests via [Vitest](https://vitest.dev) using the `@angular/build:unit-test` builder.
Tests run in Node.js with jsdom and without Zone.js testing polyfills; no browser installation is required.
Run `npm run test:ci` for a single test run that also produces `vitest-results.xml` for Jenkins and coverage reports in `coverage/ng-lightquery/`.

To generate HTML, LCOV and text-summary coverage reports locally, run `npm test -- --coverage`.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI README](https://github.com/angular/angular-cli/blob/master/README.md).
