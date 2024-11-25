# 🚀 MultiplexingServiceClient
The MultiplexingServiceClient library is designed to handle multiple connections to a Dataverse environment efficiently. It provides functionalities to execute various types of requests using multiple threads and batches, optimizing the connections for better performance.


## ✨ Key Features

**🔧 Connection Optimization**

Automatically optimizes connection settings such as DefaultConnectionLimit, ThreadPool, and ServicePointManager properties.

**⚡ Parallel Request Execution**

Executes multiple requests in parallel using Parallel.ForEach, supporting ExecuteMultipleRequest, UpdateMultipleRequest, and CreateMultipleRequest.

**🔄 Connection Management**

Manages ServiceClient instances and provides methods to clone and retrieve service clients.

## 📦 Installation
To install the **MultiplexingServiceClient** library, add the following NuGet package to your project:

```sh
dotnet add package MultiplexingServiceClient
```

or

Use the NuGet Package Manager in Visual Studio:

1. Right-click on your project in Solution Explorer and select "Manage NuGet Packages".
2. Search for `MultiplexingServiceClient`.
3. Click "Install" to add the package to your project.

## 🚀 How to Use

### Initialize the Client

To initialize the `MultiplexingServiceClient`, provide a list of connection strings for your Dataverse environments:

```csharp
var connectionStrings = new List<string>
{
    "your-connection-string-1",
    "your-connection-string-2"
};

var client = new MultiplexingServiceClient(connectionStrings);
```

### Create Multiple Request

To execute a `CreateMultipleRequest` using the `MultiplexingServiceClient`, follow these steps:

```csharp
var connectionStrings = new List<string>
{
    "your-connection-string-1",
    "your-connection-string-2"
};

var client = new MultiplexingServiceClient(connectionStrings);

var createRequest = new CreateMultipleRequest
{
    Targets = new EntityCollection(new List<Entity>
    {
        new Entity("account") { ["name"] = "Account 1" },
        new Entity("account") { ["name"] = "Account 2" }
    })
};

client.Execute(createRequest, numberOfThreads: 10, batchSize: 2);
```

## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Donations

If you find this tool useful and would like to support its development, you can buy me a coffee!

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/dynamicsninja)

Your support is greatly appreciated!