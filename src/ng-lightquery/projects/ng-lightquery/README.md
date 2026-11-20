# NgLightquery

This library is built with Angular CLI 22.3 and supports Angular 22 or later.
Its RxJS peer dependency supports `^6.5.3 || ^7.4.0`.

## Code scaffolding

Run `ng generate component component-name --project ng-lightquery` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module --project ng-lightquery`.
> Note: Don't forget to add `--project ng-lightquery` or else it will be added to the default project in your `angular.json` file. 

## Build

From the workspace directory, run `npm run buildLibrary` to build the publishable package.
The build uses `@angular/build:library` and Angular partial compilation.
The build artifacts will be stored in `dist/ng-lightquery/`.

## Publishing

After building your library with `npm run buildLibrary`, go to the dist folder `cd dist/ng-lightquery` and run `npm publish`.

## Running unit tests

Run `ng test ng-lightquery` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI README](https://github.com/angular/angular-cli/blob/master/README.md).
