# GoingMySocialNetwork

This project is a social networking application. Follow the instructions below to set up and run the application using Docker Compose.

⚠️ Notes:
Certain functions are not yet implemented.

## You can quickly access the demo at:
[https://goingmysocial.azurewebsites.net/](https://goingmysocial.azurewebsites.net/)

## Prerequisites

Ensure you have the following installed on your system:
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

## Running the Application

1. Clone the repository:
    ```bash
    git clone https://github.com/yourusername/goingmysocialnetwork.git
    cd goingmysocialnetwork
    ```

2. Build and start the containers:
    ```bash
    docker-compose up --build
    ```

3. Access the application:
    - Open your browser and navigate to `http://localhost:4200` (you can replace `4200` with the port specified in the `docker-compose.yml` file).

4. To stop the application:
    ```bash
    docker-compose down
    ```

## Troubleshooting

- If you encounter issues, check the logs using:
  ```bash
  docker-compose logs
  ```

- Ensure no other services are running on the same ports.
