# ChatWebServer
Chatweb is a simple chat application that allows users to communicate in real-time through a web-based interface. This project is developed by masluse and QueSchoenenberger.

## Getting Started

### Prerequisites
Before running Chatweb, make sure you have the following dependencies installed on your system:
- Docker
- docker-compose

### Installation
Clone the Chatweb repository from GitHub:
```
git clone https://github.com/masluse/ChatWebServer
cd ChatWebServer
```

### Usage
To run the Chatweb application locally, execute the following command within the project directory:

```
docker-compose up
```

Once the application is up and running, you can access the chat interface by navigating to http://localhost in your web browser.

## Docker Deployment

Chatweb can also be deployed using Docker and docker-compose. The official Docker image is available at ghcr.io/masluse/chatweb:latest.

To deploy the application using Docker, create a docker-compose.yml file with the following contents:

```
version: "3"
services:
  chat-server:
    image: ghcr.io/masluse/chatweb:latest
    container_name: chatweb-application
    ports:
      - 80:80
    environment:
      POSTGRES_USER: chat
      POSTGRES_PASSWORD: chatpassword
      POSTGRES_HOST: chatweb-application-db
      POSTGRES_PORT: 5432
      POSTGRES_DATABASE: chat
    tty: true
    stdin_open: true
    depends_on:
      - db

  db:
    image: postgres:13
    container_name: chatweb-application-db
    environment:
      POSTGRES_USER: chat
      POSTGRES_PASSWORD: chatpassword
      POSTGRES_DB: chat
      PGDATA: /var/lib/postgresql/data/pgdata
    restart: always
    volumes:
      - /docker/chatweb-postgres:/var/lib/postgresql/data
```

Save the docker-compose.yml file in the project directory and run the following command:
```
docker-compose up
```

The Chatweb application will be deployed and accessible at http://localhost.

## Contributing

If you would like to contribute to Chatweb, please follow these steps:

1. Fork the repository on GitHub.
2. Create a new branch with a descriptive name (git checkout -b my-new-feature).
3. Make your changes and commit them (git commit -am 'Add some feature').
4. Push the branch to your fork (git push origin my-new-feature).
5. Create a pull request and describe your changes in detail.
We welcome any contributions, including bug fixes, feature enhancements, and documentation improvements.

## License
This project is licensed under the MIT License. Feel free to use, modify, and distribute it as per the terms of the license.
