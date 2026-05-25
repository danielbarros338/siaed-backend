pipeline {
    agent any

    environment {
        GIT_BRANCH      = "${env.BRANCH_NAME ?: 'main'}"
        PROD_HOST       = 'logosnext.com.br'
        PROD_USER       = 'root'
        SSH_CREDENTIALS = 'ssh-prod-server'
        REMOTE_APP_DIR  = '/root/SiaedBackend'

        HOME            = '/tmp'
        DOTNET_CLI_HOME = '/tmp'
        NUGET_PACKAGES  = '/tmp/.nuget/packages'
    }

    stages {
        stage('Restore, Build & Verify') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:10.0'
                    reuseNode true
                }
            }
            steps {
                sh '''
                    mkdir -p /tmp/.nuget/packages
                    dotnet restore SiaedBackend.slnx
                    dotnet build SiaedBackend.slnx -c Release --no-restore
                '''
            }
        }

        stage('Deploy to Production') {
            steps {
                sshagent(credentials: [env.SSH_CREDENTIALS]) {
                    sh """
                        ssh -o StrictHostKeyChecking=no ${PROD_USER}@${PROD_HOST} '
                        set -e
                        cd ${REMOTE_APP_DIR}

                        echo "Commit antes:"
                        git rev-parse --short HEAD

                        git fetch origin
                        git reset --hard origin/${GIT_BRANCH}

                        echo "Commit depois:"
                        git rev-parse --short HEAD

                        docker compose up -d --build --force-recreate --remove-orphans
                        '
                    """
                }
            }
        }
    }

    post {
        always {
            cleanWs()
        }
    }
}