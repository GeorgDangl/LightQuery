pipeline {
    agent {
        node {
            label 'master'
            customWorkspace 'workspace/LightQuery'
        }
    }
    environment {
        KeyVaultBaseUrl = credentials('AzureCiKeyVaultBaseUrl')
        KeyVaultClientId = credentials('AzureCiKeyVaultClientId')
        KeyVaultClientSecret = credentials('AzureCiKeyVaultClientSecret')
    }
    stages {
        stage ('Test') {
            steps {
                powershell './build.ps1 Coverage -configuration Debug'
            }
            post {
                always {
                    warnings(
                        canComputeNew: false,
                        canResolveRelativePaths: false,
                        categoriesPattern: '',
                        consoleParsers: [[parserName: 'MSBuild']],
                        defaultEncoding: '',
                        excludePattern: '',
                        healthy: '',
                        includePattern: '',
                        messagesPattern: '',
                        unHealthy: '')
                    openTasks(
                        canComputeNew: false,
                        defaultEncoding: '',
                        excludePattern: '',
                        healthy: '',
                        high: 'HACK, FIXME',
                        ignoreCase: true,
                        low: '',
                        normal: 'TODO',
                        pattern: '**/*.cs, **/*.g4, **/*.ts, **/*.js',
                        unHealthy: '')
                    xunit(
                        testTimeMargin: '3000',
                        thresholdMode: 1,
                        thresholds: [
                            failed(failureNewThreshold: '0', failureThreshold: '0', unstableNewThreshold: '0', unstableThreshold: '0'),
                            skipped(failureNewThreshold: '0', failureThreshold: '0', unstableNewThreshold: '0', unstableThreshold: '0')
                        ],
                        tools: [
                            xUnitDotNet(deleteOutputFiles: true, failIfNotNew: true, pattern: '**/*_testresults*.xml', stopProcessingIfError: false)
                        ])
                    cobertura(
                        coberturaReportFile: 'output/cobertura_coverage.xml',
                        failUnhealthy: false,
                        failUnstable: false,
                        maxNumberOfBuilds: 0,
                        onlyStable: false,
                        zoomCoverageChart: false)
                    publishHTML([
                        allowMissing: false,
                        alwaysLinkToLastBuild: false,
                        keepAll: false,
                        reportDir: 'output/CoverageReport',
                        reportFiles: 'index.htm',
                        reportName: 'Coverage Report',
                        reportTitles: ''])
                }
            }
        }
        stage ('Deploy') {
            steps {
                powershell './build.ps1 UploadDocumentation+PublishGitHubRelease'
            }
        }
        stage ('Angular Library Test') {
            steps {
                powershell './build.ps1 NgLibraryTest'
            }
            post {
                always {
                    junit '**/*karma-results.xml'
                }
            }
        }
        stage ('Angular Library Publish') {
            steps {
                powershell './build.ps1 NgLibraryPublish'
            }
        }
    }
    post {
        always {
            step([$class: 'Mailer',
                notifyEveryUnstableBuild: true,
                recipients: "georg@dangl.me",
                sendToIndividuals: true])
        }
    }
}
