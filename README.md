**Project Description**
This is a .NET-based example demonstrating a Publisher/Consumer pattern using RabbitMQ. 
It includes two services (publisher and consumer), a lightweight RabbitMQ wrapper project, and a test project.

**Contents**
- `PublisherService/` : publisher that sends messages.
- `ConsumerService/` : consumer that receives and processes messages.
- `RabbitMqService/` : shared transport and serializer interfaces and a RabbitMQ transport implementation.
- `Test/` : unit/integration test examples for the components.
- `docker-compose.yaml` : optional Docker Compose setup for RabbitMQ and services.

**Prerequisites**
- .NET SDK 8.0 or later installed
- (Optional) Docker & Docker Compose to run RabbitMQ and services in containers

**Clone and Run**
git clone https://github.com/Busisile-Zilwa2601/rabbit-mq-project.git
cd rabbit-mq-project
- Starting up locally
  1. Start RabbitMQ locally (or use Docker Compose below).
  2. In two consoles, run the publisher and consumer projects:

```powershell
cd PublisherService
dotnet run

cd ..\ConsumerService
dotnet run
```
The publisher will send messages defined by the `Message` contract and the consumer will receive them.

- Start up with Docker-Compose
   From the repo root:

  ```powershell
  docker compose up --build
  ```
  This will bring up RabbitMQ (if defined in `docker-compose.yaml`) and build/run services if configured. Use `docker compose down` to stop and remove containers.

  NOTE: Docker runs your container in non-interactive mode, so Console.ReadLine() will not wait for keyboard input.
  Containers are not interactive by default.

**Running tests**
Run tests from the repo root:

```powershell
dotnet test Test/Test.csproj
```
