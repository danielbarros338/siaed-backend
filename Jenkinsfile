pipeline {
    agent any

    environment {
        GIT_BRANCH                                      = "${env.BRANCH_NAME ?: 'main'}"
        PROD_HOST                                       = 'logosnext.com.br'
        PROD_USER                                       = 'root'
        SSH_CREDENTIALS                                 = 'ssh-prod-server'
        REMOTE_APP_DIR                                  = "${env.REMOTE_APP_DIR ?: '/root/siaed-backend'}"
        REMOTE_REPO_URL                                 = "${env.GIT_URL ?: 'https://github.com/danielbarros338/siaed-backend'}"
        MYSQL_PASSWORD_CREDENTIAL                       = 'siaed-mysql-password'
        MYSQL_ROOT_PASSWORD_CREDENTIAL                  = 'siaed-mysql-root-password'
        JWT__KEY_CREDENTIAL                             = 'siaed-jwt-key'
        JWT__ISSUER_CREDENTIAL                          = 'siaed-jwt-issuer'
        JWT__AUDIENCE_CREDENTIAL                        = 'siaed-jwt-audience'
        OPENAI__APIKEY_CREDENTIAL                       = 'siaed-openai-apikey'
        URL_API                                         = 'siaed_url_api'
        EMAIL__EMAILSETTINGS__HOST                      = 'siaed_email_host'
        EMAIL__EMAILSETTINGS__PORT                      = 'siaed_email_port'
        EMAIL__EMAILSETTINGS__USERNAME                  = 'siaed_email_username'
        EMAIL__EMAILSETTINGS__PASSWORD                  = 'siaed_email_password'
        EMAIL__EMAILSETTINGS__FROM                      = 'siaed_email_from'
        EMAIL__EMAILSETTINGS__DISPLAY_NAME              = 'siaed_email_display_name'
        EMAIL__CLIENTGMAIL__CLIENT_ID                   = 'siaed_client_gmail_client_id'
        EMAIL__CLIENTGMAIL__PROJECT_ID                  = 'siaed_client_gmail_project_id'
        EMAIL__CLIENTGMAIL__AUTH_URI                    = 'siaed_client_gmail_auth_uri'
        EMAIL__CLIENTGMAIL__TOKEN_URI                   = 'siaed_client_gmail_token_uri'
        EMAIL__CLIENTGMAIL__AUTH_PROVIDER_X509_CERT_URL = 'siaed_client_gmail_auth_provider_x509_cert_url'
        EMAIL__CLIENTGMAIL__CLIENT_SECRET               = 'siaed_client_gmail_client_secret'
        EMAIL__CLIENTGMAIL__JAVASCRIPT_ORIGINS          = 'siaed_client_gmail_javascript_origins'

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
                    string(credentialsId: env.OPENAI__APIKEY_CREDENTIAL, variable: 'OpenAI__ApiKey'),
                    string(credentialsId: env.URL_API, variable: 'UrlApi'),
                    string(credentialsId: env.EMAIL__EMAILSETTINGS__HOST, variable: 'Email__EmailSettings__Host'),
                    string(credentialsId: env.EMAIL__EMAILSETTINGS__PORT, variable: 'Email__EmailSettings__Port'),
                    string(credentialsId: env.EMAIL__EMAILSETTINGS__USERNAME, variable: 'Email__EmailSettings__Username'),
                    string(credentialsId: env.EMAIL__EMAILSETTINGS__PASSWORD, variable: 'Email__EmailSettings__Password'),
                    string(credentialsId: env.EMAIL__EMAILSETTINGS__FROM, variable: 'Email__EmailSettings__From'),
                    string(credentialsId: env.EMAIL__EMAILSETTINGS__DISPLAY_NAME, variable: 'Email__EmailSettings__DisplayName'),
                    string(credentialsId: env.EMAIL__CLIENTGMAIL__CLIENT_ID, variable: 'Email__ClientGmail__ClientId'),
                    string(credentialsId: env.EMAIL__CLIENTGMAIL__PROJECT_ID, variable: 'Email__ClientGmail__ProjectId'),
                    string(credentialsId: env.EMAIL__CLIENTGMAIL__AUTH_URI, variable: 'Email__ClientGmail__AuthUri'),
                    string(credentialsId: env.EMAIL__CLIENTGMAIL__TOKEN_URI, variable: 'Email__ClientGmail__TokenUri'),
                    string(credentialsId: env.EMAIL__CLIENTGMAIL__AUTH_PROVIDER_X509_CERT_URL, variable: 'Email__ClientGmail__AuthProviderX509CertUrl'),
                    string(credentialsId: env.EMAIL__CLIENTGMAIL__CLIENT_SECRET, variable: 'Email__ClientGmail__ClientSecret'),
                    string(credentialsId: env.EMAIL__CLIENTGMAIL__JAVASCRIPT_ORIGINS, variable: 'Email__ClientGmail__JavascriptOrigins')
                ]) {
                    sshagent(credentials: [env.SSH_CREDENTIALS]) {
                        sh '''
                            set -e

                            TEMP_ENV_FILE=$(mktemp)
                            trap 'rm -f "$TEMP_ENV_FILE"' EXIT

                            # Docker Compose interpreta $ em valores do .env.
                            # Escapamos para $$ para preservar segredos literais.
                            escape_for_compose_env() {
                                printf '%s' "$1" | sed 's/[$]/$$/g'
                            }

                            MYSQL_PASSWORD_ESCAPED=$(escape_for_compose_env "$MYSQL_PASSWORD")
                            MYSQL_ROOT_PASSWORD_ESCAPED=$(escape_for_compose_env "$MYSQL_ROOT_PASSWORD")
                            JWT_KEY_ESCAPED=$(escape_for_compose_env "$Jwt__Key")
                            JWT_ISSUER_ESCAPED=$(escape_for_compose_env "$Jwt__Issuer")
                            JWT_AUDIENCE_ESCAPED=$(escape_for_compose_env "$Jwt__Audience")
                            OPENAI_API_KEY_ESCAPED=$(escape_for_compose_env "$OpenAI__ApiKey")
                            URL_API_ESCAPED=$(escape_for_compose_env "$UrlApi")
                            EMAIL_SETTINGS_HOST_ESCAPED=$(escape_for_compose_env "$Email__EmailSettings__Host")
                            EMAIL_SETTINGS_PORT_ESCAPED=$(escape_for_compose_env "$Email__EmailSettings__Port")
                            EMAIL_SETTINGS_USERNAME_ESCAPED=$(escape_for_compose_env "$Email__EmailSettings__Username")
                            EMAIL_SETTINGS_PASSWORD_ESCAPED=$(escape_for_compose_env "$Email__EmailSettings__Password")
                            EMAIL_SETTINGS_FROM_ESCAPED=$(escape_for_compose_env "$Email__EmailSettings__From")
                            EMAIL_SETTINGS_DISPLAY_NAME_ESCAPED=$(escape_for_compose_env "$Email__EmailSettings__DisplayName")
                            EMAIL_CLIENT_GMAIL_CLIENT_ID_ESCAPED=$(escape_for_compose_env "$Email__ClientGmail__ClientId")
                            EMAIL_CLIENT_GMAIL_PROJECT_ID_ESCAPED=$(escape_for_compose_env "$Email__ClientGmail__ProjectId")
                            EMAIL_CLIENT_GMAIL_AUTH_URI_ESCAPED=$(escape_for_compose_env "$Email__ClientGmail__AuthUri")
                            EMAIL_CLIENT_GMAIL_TOKEN_URI_ESCAPED=$(escape_for_compose_env "$Email__ClientGmail__TokenUri")
                            EMAIL_CLIENT_GMAIL_AUTH_PROVIDER_X509_CERT_URL_ESCAPED=$(escape_for_compose_env "$Email__ClientGmail__AuthProviderX509CertUrl")
                            EMAIL_CLIENT_GMAIL_CLIENT_SECRET_ESCAPED=$(escape_for_compose_env "$Email__ClientGmail__ClientSecret")
                            EMAIL_CLIENT_GMAIL_JAVASCRIPT_ORIGINS_ESCAPED=$(escape_for_compose_env "$Email__ClientGmail__JavascriptOrigins")

                            require_non_empty() {
                                VAR_NAME="$1"
                                VAR_VALUE="$2"

                                if [ -z "$VAR_VALUE" ]; then
                                    echo "Credencial obrigatoria vazia: $VAR_NAME"
                                    exit 1
                                fi
                            }

                            require_non_empty "MYSQL_PASSWORD" "$MYSQL_PASSWORD"
                            require_non_empty "MYSQL_ROOT_PASSWORD" "$MYSQL_ROOT_PASSWORD"
                            require_non_empty "Jwt__Key" "$Jwt__Key"
                            require_non_empty "Jwt__Issuer" "$Jwt__Issuer"
                            require_non_empty "Jwt__Audience" "$Jwt__Audience"
                            require_non_empty "OpenAI__ApiKey" "$OpenAI__ApiKey"
                            require_non_empty "UrlApi" "$UrlApi"
                            require_non_empty "Email__EmailSettings__Host" "$Email__EmailSettings__Host"
                            require_non_empty "Email__EmailSettings__Port" "$Email__EmailSettings__Port"
                            require_non_empty "Email__EmailSettings__Username" "$Email__EmailSettings__Username"
                            require_non_empty "Email__EmailSettings__Password" "$Email__EmailSettings__Password"
                            require_non_empty "Email__EmailSettings__From" "$Email__EmailSettings__From"

                            case "$Email__EmailSettings__Port" in
                                ''|*[!0-9]*)
                                    echo "Credencial invalida: Email__EmailSettings__Port deve ser numerica"
                                    exit 1
                                    ;;
                            esac

                            cat > "$TEMP_ENV_FILE" <<EOF
                            MYSQL_PASSWORD=${MYSQL_PASSWORD_ESCAPED}
                            MYSQL_ROOT_PASSWORD=${MYSQL_ROOT_PASSWORD_ESCAPED}
                            Jwt__Key=${JWT_KEY_ESCAPED}
                            Jwt__Issuer=${JWT_ISSUER_ESCAPED}
                            Jwt__Audience=${JWT_AUDIENCE_ESCAPED}
                            OpenAI__ApiKey=${OPENAI_API_KEY_ESCAPED}
                            UrlApi=${URL_API_ESCAPED}
                            Email__EmailSettings__Host=${EMAIL_SETTINGS_HOST_ESCAPED}
                            Email__EmailSettings__Port=${EMAIL_SETTINGS_PORT_ESCAPED}
                            Email__EmailSettings__Username=${EMAIL_SETTINGS_USERNAME_ESCAPED}
                            Email__EmailSettings__Password=${EMAIL_SETTINGS_PASSWORD_ESCAPED}
                            Email__EmailSettings__From=${EMAIL_SETTINGS_FROM_ESCAPED}
                            Email__EmailSettings__DisplayName=${EMAIL_SETTINGS_DISPLAY_NAME_ESCAPED}
                            Email__ClientGmail__ClientId=${EMAIL_CLIENT_GMAIL_CLIENT_ID_ESCAPED}
                            Email__ClientGmail__ProjectId=${EMAIL_CLIENT_GMAIL_PROJECT_ID_ESCAPED}
                            Email__ClientGmail__AuthUri=${EMAIL_CLIENT_GMAIL_AUTH_URI_ESCAPED}
                            Email__ClientGmail__TokenUri=${EMAIL_CLIENT_GMAIL_TOKEN_URI_ESCAPED}
                            Email__ClientGmail__AuthProviderX509CertUrl=${EMAIL_CLIENT_GMAIL_AUTH_PROVIDER_X509_CERT_URL_ESCAPED}
                            Email__ClientGmail__ClientSecret=${EMAIL_CLIENT_GMAIL_CLIENT_SECRET_ESCAPED}
                            Email__ClientGmail__JavascriptOrigins=${EMAIL_CLIENT_GMAIL_JAVASCRIPT_ORIGINS_ESCAPED}
                            EOF

                            ssh -o StrictHostKeyChecking=no ${PROD_USER}@${PROD_HOST} "mkdir -p '${REMOTE_APP_DIR}'"
                            scp -o StrictHostKeyChecking=no "$TEMP_ENV_FILE" ${PROD_USER}@${PROD_HOST}:${REMOTE_APP_DIR}/.env

                            ssh -o StrictHostKeyChecking=no ${PROD_USER}@${PROD_HOST} "APP_DIR='${REMOTE_APP_DIR}' REPO_URL='${REMOTE_REPO_URL}' BRANCH_NAME='${GIT_BRANCH}' bash -se" <<'EOSSH'
                            set -e

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

                            echo "Host de deploy: $(hostname)"
                            echo "Diretorio de deploy: $APP_DIR"
                            echo "Branch de deploy: $BRANCH_NAME"

                            echo "Commit antes:"
                            git rev-parse --short HEAD

                            git fetch --prune origin
                            git checkout -B "$BRANCH_NAME" "origin/$BRANCH_NAME"
                            git reset --hard "origin/$BRANCH_NAME"

                            echo "Commit depois:"
                            git rev-parse --short HEAD

                            echo "Container atual (antes do restart):"
                            docker ps --filter name=siaed-api --format 'table {{.Names}}\t{{.ID}}\t{{.Image}}\t{{.RunningFor}}' || true

                            docker network inspect nginx_net >/dev/null 2>&1 || docker network create nginx_net

                            if ! docker compose --env-file .env build --pull --no-cache siaed-migrations siaed-api \
                                || ! docker compose --env-file .env down --remove-orphans \
                                || ! docker compose --env-file .env up -d --remove-orphans; then
                                echo "Falha ao subir stack. Estado dos containers:"
                                docker compose --env-file .env ps || true
                                echo "Logs das migrations:"
                                docker compose --env-file .env logs siaed-migrations || true
                                echo "Logs da API:"
                                docker compose --env-file .env logs siaed-api || true
                                exit 1
                            fi

                            echo "Estado final dos containers:"
                            docker compose --env-file .env ps
                            echo "Container atual (apos deploy):"
                            docker ps --filter name=siaed-api --format 'table {{.Names}}\t{{.ID}}\t{{.Image}}\t{{.RunningFor}}'
EOSSH
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
