pipeline {
    agent any

    environment {
        GIT_BRANCH      = "${env.BRANCH_NAME ?: 'main'}"
        PROD_HOST       = 'logosnext.com.br'
        PROD_USER       = 'root'
        SSH_CREDENTIALS = 'ssh-prod-server'
        REMOTE_APP_DIR  = "${env.REMOTE_APP_DIR ?: '/root/siaed-backend'}"
        REMOTE_REPO_URL = "${env.GIT_URL ?: 'https://github.com/danielbarros338/siaed-backend'}"
        MYSQL_PASSWORD_CREDENTIAL      = 'siaed-mysql-password'
        MYSQL_ROOT_PASSWORD_CREDENTIAL = 'siaed-mysql-root-password'
        JWT__KEY_CREDENTIAL            = 'siaed-jwt-key'
        JWT__ISSUER_CREDENTIAL         = 'siaed-jwt-issuer'
        JWT__AUDIENCE_CREDENTIAL       = 'siaed-jwt-audience'
        OPENAI__APIKEY_CREDENTIAL      = 'siaed-openai-apikey'

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
                withCredentials([
                    string(credentialsId: env.MYSQL_PASSWORD_CREDENTIAL, variable: 'MYSQL_PASSWORD'),
                    string(credentialsId: env.MYSQL_ROOT_PASSWORD_CREDENTIAL, variable: 'MYSQL_ROOT_PASSWORD'),
                    string(credentialsId: env.JWT__KEY_CREDENTIAL, variable: 'Jwt__Key'),
                    string(credentialsId: env.JWT__ISSUER_CREDENTIAL, variable: 'Jwt__Issuer'),
                    string(credentialsId: env.JWT__AUDIENCE_CREDENTIAL, variable: 'Jwt__Audience'),
                    string(credentialsId: env.OPENAI__APIKEY_CREDENTIAL, variable: 'OpenAI__ApiKey')
                ]) {
                    sshagent(credentials: [env.SSH_CREDENTIALS]) {
                        sh '''
                            set -e

                            TEMP_ENV_FILE=$(mktemp)
                            trap 'rm -f "$TEMP_ENV_FILE"' EXIT

                            cat > "$TEMP_ENV_FILE" <<EOF
MYSQL_PASSWORD=${MYSQL_PASSWORD}
MYSQL_ROOT_PASSWORD=${MYSQL_ROOT_PASSWORD}
Jwt__Key=${Jwt__Key}
Jwt__Issuer=${Jwt__Issuer}
Jwt__Audience=${Jwt__Audience}
OpenAI__ApiKey=${OpenAI__ApiKey}
EOF

                            ssh -o StrictHostKeyChecking=no ${PROD_USER}@${PROD_HOST} "mkdir -p '${REMOTE_APP_DIR}'"
                            scp -o StrictHostKeyChecking=no "$TEMP_ENV_FILE" ${PROD_USER}@${PROD_HOST}:${REMOTE_APP_DIR}/.env

                            ssh -o StrictHostKeyChecking=no ${PROD_USER}@${PROD_HOST} '
                            set -e
                            APP_DIR="${REMOTE_APP_DIR}"
                            REPO_URL="${REMOTE_REPO_URL}"
                            BRANCH_NAME="${GIT_BRANCH}"

                            command -v git >/dev/null 2>&1 || { echo "git nao instalado na VPS"; exit 1; }
                            command -v docker >/dev/null 2>&1 || { echo "docker nao instalado na VPS"; exit 1; }
                            docker compose version >/dev/null 2>&1 || { echo "docker compose plugin nao encontrado na VPS"; exit 1; }

                            if [ ! -d "$APP_DIR/.git" ]; then
                                echo "Diretorio de deploy inexistente ou sem checkout Git: $APP_DIR"

                                if [ -z "$(find "$APP_DIR" -mindepth 1 -maxdepth 1 ! -name .env -print -quit)" ]; then
                                    TEMP_REMOTE_ENV=$(mktemp)

                                    if [ -f "$APP_DIR/.env" ]; then
                                        mv "$APP_DIR/.env" "$TEMP_REMOTE_ENV"
                                    fi

                                    rm -rf "$APP_DIR"
                                    git clone --branch "$BRANCH_NAME" "$REPO_URL" "$APP_DIR"
                                    mv "$TEMP_REMOTE_ENV" "$APP_DIR/.env"
                                else
                                    echo "O diretorio $APP_DIR existe, mas nao contem um checkout Git valido."
                                    exit 1
                                fi
                            fi

                            chmod 600 "$APP_DIR/.env"
                            cd "$APP_DIR"

                            echo "Commit antes:"
                            git rev-parse --short HEAD

                            git fetch origin
                            git reset --hard origin/$BRANCH_NAME

                            echo "Commit depois:"
                            git rev-parse --short HEAD

                            docker network inspect nginx_net >/dev/null 2>&1 || docker network create nginx_net
                            docker compose up -d --build --force-recreate --remove-orphans
                            '
                        '''
                    }
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