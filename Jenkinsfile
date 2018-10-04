pipeline {
    agent any
    environment {
        WORKSPACE = "${pwd()}"
        SOLUTION_ID = 'PS.OTT.Microservice.DeleteDevice.KAZ'
        SOLUTION_FILENAME = "${SOLUTION_ID}.sln"
        PUBLISH_PROJECT_TARGET_DIR = "${SOLUTION_ID}"
        PUBLISH_PROJECT_TARGET_FILENAME = "${SOLUTION_ID}.csproj"
        
        PUBLISH_TEMP_DIR = "D:\\PSOTT_DEV\\Releases\\${SOLUTION_ID}\\Temp_${VERSION_TAG}"
        PUBLISH_DIR_LOCAL = "D:\\PSOTT_DEV\\Releases\\${SOLUTION_ID}"
        PUBLISH_DIR_NETWORK = "\\\\34.248.201.147\\PSOTT_DEV\\Releases\\${SOLUTION_ID}"
        
        // These are consts do not change them
        VERSION_TAG = sh(returnStdout: true, script: "git describe --always --dirty --long --tags").trim().replaceFirst('-','.')
        MSBUILD = tool name: 'V4.6.1', type: 'hudson.plugins.msbuild.MsBuildInstallation'
        NUGET_REPO_GLOBAL = 'https://nuget.org/api/v2/'
        NUGET_REPO_PS = 'D:\\NugetRepo'
        NUGET = 'c:\\nuget.exe'

   }

    options {
        buildDiscarder(logRotator(numToKeepStr:'5'))
    }

    stages {
        
        stage('Nuget Restore') {
            steps { 
                bat "dotnet restore ${SOLUTION_FILENAME} --source ${NUGET_REPO_GLOBAL} --source ${NUGET_REPO_PS}" 
            }
        }

        stage('Assembly Version Patch') {
            steps { 
                sh "DllVersioning.Core.sh \"./${PUBLISH_PROJECT_TARGET_DIR}/\"" 
                echo "publish network path is: ${PUBLISH_DIR_NETWORK}"
            }
        }

        stage('Publish to Disk') {
            steps { 
                bat "dotnet publish --self-contained false"
            }
        }

        stage('Zip'){
            steps {
                bat "\"C:\\Program Files\\7-Zip\\7z.exe\" a \"${PUBLISH_DIR_LOCAL}\\${VERSION_TAG}.zip\" \"${WORKSPACE}\\${SOLUTION_ID}\\bin\\Debug\\netcoreapp2.0\\publish\\*\""
            }
        }

        stage('Cleanup'){
            steps {
                sh "rm -rf \"${SOLUTION_ID}\\bin\""
            }
        }

    }
}