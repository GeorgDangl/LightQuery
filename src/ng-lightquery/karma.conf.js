// Karma configuration
// Generated on Thu Nov 16 2017 20:19:54 GMT+0100 (Mitteleuropäische Zeit)

module.exports = function(config) {
  config.set({

    // frameworks to use
    // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
    frameworks: ['jasmine', 'karma-typescript'],


    // list of files / patterns to load in the browser
    files: [
      { pattern: 'test.spec.ts' },
      { pattern: 'src/**/*.ts' }
    ],

    client: {
      clearContext: false // leave Jasmine Spec Runner output visible in browser
    },

    // preprocess matching files before serving them to the browser
    // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
    preprocessors: {
      '**/*.ts': ['karma-typescript']
    },

    karmaTypescriptConfig: {
      bundlerOptions: {
          entrypoints: /\.spec.ts$/
      },
      compilerOptions: {
          lib: ["ES2015", "DOM"]
      },
      reports: {
        "cobertura": {
          "filename": "typescript.coverageresults"
        }
      }
  },

    // test results reporter to use
    // possible values: 'dots', 'progress'
    // available reporters: https://npmjs.org/browse/keyword/karma-reporter
    reporters: ['progress', 'dots', 'karma-typescript', 'kjhtml', 'junit'],


    // web server port
    port: 9876,


    // enable / disable colors in the output (reporters and logs)
    colors: true,


    // level of logging
    // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
    logLevel: config.LOG_INFO,


    // enable / disable watching file and executing tests whenever any file changes
    autoWatch: true,


    // start these browsers
    // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
    browsers: ['PhantomJS'],


    // Continuous Integration mode
    // if true, Karma captures browsers, runs the tests and exits
    singleRun: true,
    
    junitReporter: {
        outputDir: '../../output',
        outputFile: 'karma-results.xml',
        useBrowserName: false
    },

    // Concurrency level
    // how many browser should be started simultaneous
    concurrency: Infinity
  })
}
