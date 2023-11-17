pipeline {
    agent {
        node {
            label 'master'
        }
    }
    environment {
        KeyVaultBaseUrl = credentials('AzureCiKeyVaultBaseUrl')
        KeyVaultClientId = credentials('AzureCiKeyVaultClientId')
        KeyVaultClientSecret = credentials('AzureCiKeyVaultClientSecret')
        KeyVaultTenantId = credentials('AzureKeyVaultTenantId')
    }
    stages {
        stage ('CI') {
          parallel {
            stage ('Backend') {
              stages {
                stage ('Test') {
                    steps {
                        powershell './build.ps1 Coverage -configuration Debug'
                    }
                    post {
                        always {
                            recordIssues(
                                tools: [
                                    msBuild(), 
                                    taskScanner(
                                        excludePattern: '**/*node_modules/**/*', 
                                        highTags: 'HACK, FIXME', 
                                        ignoreCase: true, 
                                        includePattern: '**/*.cs, **/*.g4, **/*.ts, **/*.js', 
                                        normalTags: 'TODO')
                                    ])
                            xunit(
                                testTimeMargin: '3000',
                                thresholdMode: 1,
                                thresholds: [
                                    failed(failureNewThreshold: '0', failureThreshold: '0', unstableNewThreshold: '0', unstableThreshold: '0'),
                                    skipped(failureNewThreshold: '0', failureThreshold: '0', unstableNewThreshold: '0', unstableThreshold: '0')
                                ],
                                tools: [
                                    xUnitDotNet(deleteOutputFiles: true, failIfNotNew: true, pattern: '**/*testresults.xml', stopProcessingIfError: false)
                                ])
                            cobertura(
                                coberturaReportFile: 'output/Cobertura.xml',
                                failUnhealthy: false,
                                failUnstable: false,
                                maxNumberOfBuilds: 0,
                                onlyStable: false,
                                zoomCoverageChart: false)
                        }
                    }
                }
                stage ('Deploy') {
                    steps {
                        powershell './build.ps1 UploadDocumentation+PublishGitHubRelease'
                    }
                }
              }
            }
            stage ('Frontend') {
              stages {
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
			}
          }
        }
    }
    post {
        always {
            step([$class: 'Mailer',
                notifyEveryUnstableBuild: true,
                recipients: [
                    emailextrecipients([
                        [$class: 'CulpritsRecipientProvider'],
                        [$class: 'RequesterRecipientProvider']
                        ])
                    ].join(' '),
                sendToIndividuals: true])
            cleanWs()
        }
    }
}
