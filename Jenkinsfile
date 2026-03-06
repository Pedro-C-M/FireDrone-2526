pipeline {
    agent any
    
    // Environment variables that Jenkins will provide
    // These should be configured in Jenkins: Manage Jenkins > Credentials
    environment {
        // Reference Jenkins credentials here
        // Syntax: credentials('credential-id-in-jenkins')
        RABBITMQ_HOST = credentials('rabbitmq-host')
        RABBITMQ_USER = credentials('rabbitmq-user')
        RABBITMQ_PASSWORD = credentials('rabbitmq-password')
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Build') {
            steps {
                script {
                    // Set environment variables for Docker Compose
                    sh """
                        export RABBITMQ_HOST=${RABBITMQ_HOST}
                        export RABBITMQ_USER=${RABBITMQ_USER}
                        export RABBITMQ_PASSWORD=${RABBITMQ_PASSWORD}
                        
                        # Run Ant build
                        ./ant/bin/ant clean build
                    """
                }
            }
        }
        
        stage('Test') {
            steps {
                script {
                    sh """
                        export RABBITMQ_HOST=${RABBITMQ_HOST}
                        export RABBITMQ_USER=${RABBITMQ_USER}
                        export RABBITMQ_PASSWORD=${RABBITMQ_PASSWORD}
                        
                        # Run your tests here
                        ./ant/bin/ant test
                    """
                }
            }
        }
        
        stage('Deploy with Docker') {
            steps {
                script {
                    // Pass environment variables to docker-compose
                    sh """
                        export RABBITMQ_HOST=${RABBITMQ_HOST}
                        export RABBITMQ_USER=${RABBITMQ_USER}
                        export RABBITMQ_PASSWORD=${RABBITMQ_PASSWORD}
                        
                        docker-compose down
                        docker-compose up -d --build
                    """
                }
            }
        }
    }
    
    post {
        always {
            // Cleanup
            cleanWs()
        }
        failure {
            echo 'Build failed!'
        }
        success {
            echo 'Build succeeded!'
        }
    }
}
