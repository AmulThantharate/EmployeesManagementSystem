pipeline {
    agent any 

    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
        timestamps()
        timeout(time: 1, unit: 'HOURS')
        disableConcurrentBuilds()
    }
    
    enviorment {
        SCANNER_HOME = tool 'sonar'
    }

    stages {
        stage("Checkout") {
            steps {
                git branch: 'main', url: 'https://github.com/AmulThantharate/EmployeesManagementSystem.git'
            }
        }
    

    stage("Check Dotnet version"){
        steps {
            sh 'dotnet --version'
        }
    }
    }
}