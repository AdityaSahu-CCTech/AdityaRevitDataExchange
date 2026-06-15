# Aditya Revit Data Exchange Connector

A custom Autodesk Data Exchange Connector built for Autodesk Revit that extracts Revit model data, geometry, and parameters and publishes them to Autodesk Data Exchange (DXSDK).

This project acts as a bridge between Revit and Autodesk Data Exchange, allowing BIM data to be transformed into Exchange Elements and synchronized to Autodesk Construction Cloud (ACC).

---

# Overview

The connector performs the following operations:

- Connects Revit with Autodesk Data Exchange SDK
- Reads Revit elements from the active document
- Extracts geometry from Revit elements
- Extracts instance and type parameters
- Converts Revit objects into DXSDK ElementDataModel elements
- Attaches Revit parameters as Data Exchange parameters
- Creates and updates Data Exchanges
- Synchronizes Exchange data to Autodesk Construction Cloud

---

# Architecture

```text
┌─────────────────────────┐
│       Revit Model       │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ RevitElementCollector   │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ RevitParameterService   │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ RevitGeometryService    │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ RevitExchangeBuilder    │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ ElementDataModel        │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Autodesk Data Exchange  │
└─────────────────────────┘
```

---

# Project Structure

```text
AdityaRevitDataExchange
│
├── Application
│   └── App.cs
│
├── Commands
│   └── OpenDataExchangeCommand.cs
│
├── DXSDK
│   ├── ConnectorConfiguration.cs
│   ├── RevitConnectorHost.cs
│   ├── RevitInteropInvoker.cs
│   └── RevitReadWriteModel.cs
│
├── Models
│   ├── RevitElementInfo.cs
│   └── RevitParameterInfo.cs
│
├── Services
│   ├── Logger.cs
│   ├── RevitContext.cs
│   ├── RevitElementCollectorService.cs
│   ├── RevitExchangeBuilderService.cs
│   ├── RevitGeometryService.cs
│   └── RevitParameterService.cs
│
├── app.config
└── AdityaRevitDataExchange.addin
```

---

# Component Details

---

## Application

### App.cs

Revit Entry Point.

Responsibilities:

- Implements `IExternalApplication`
- Registers ribbon buttons
- Loads connector during Revit startup
- Initializes Data Exchange environment

---

## Commands

### OpenDataExchangeCommand.cs

Responsible for launching the connector UI.

Responsibilities:

- Opens Data Exchange Connector window
- Creates connector host
- Starts DXSDK workflow

---

## DXSDK Layer

This layer contains all Data Exchange SDK integration code.

---

### ConnectorConfiguration.cs

Stores connector configuration.

Responsibilities:

- Connector Name
- Connector Version
- DXSDK settings
- Authentication settings

---

### RevitConnectorHost.cs

Main connector bootstrapper.

Responsibilities:

- Creates DXSDK client
- Initializes connector services
- Creates Read/Write model
- Starts connector UI

---

### RevitInteropInvoker.cs

Acts as a bridge between:

```text
DXSDK UI
    ↕
Revit Connector
```

Responsibilities:

- Handles connector commands
- Passes data between UI and Revit
- Invokes Revit operations

---

### RevitReadWriteModel.cs

Core connector implementation.

Inherits from:

```csharp
BaseReadWriteExchangeModel
```

Responsibilities:

### Reading

- Fetch existing exchanges
- Download exchange data
- Read exchange revisions
- Load ElementDataModel

### Writing

- Create Exchange
- Update Exchange
- Build Exchange Data
- Sync data to Autodesk cloud

### Exchange Synchronization

```csharp
await Client.SyncExchangeDataAsync(
    identifier,
    elementDataModel
);
```

---

# Models

---

## RevitElementInfo.cs

Represents a Revit element.

Contains:

```csharp
ElementId
UniqueId
Name
Category
Parameters
Geometry
```

Used as intermediate data model before DXSDK conversion.

---

## RevitParameterInfo.cs

Represents a Revit parameter.

Contains:

```csharp
Name
Value
StorageType
IsReadOnly
```

Used for mapping Revit parameters to Data Exchange parameters.

---

# Services

---

## RevitContext.cs

Centralized access to Revit API objects.

Provides:

```csharp
UIApplication
UIDocument
Document
```

Used throughout the application.

---

## Logger.cs

Logging utility.

Responsibilities:

- Info Logs
- Warning Logs
- Error Logs
- Debug Messages

---

## RevitElementCollectorService.cs

Collects Revit elements from active document.

Responsibilities:

### Element Collection

```csharp
FilteredElementCollector
```

### Supported Categories

- Walls
- Floors
- Doors
- Windows
- Columns
- Structural Members
- Generic Models

and other model elements.

Returns:

```csharp
List<RevitElementInfo>
```

---

## RevitParameterService.cs

Extracts Revit parameters.

Responsibilities:

### Instance Parameters

```csharp
element.Parameters
```

### Type Parameters

```csharp
elementType.Parameters
```

### Parameter Conversion

Converts:

```text
Revit Parameter
        ↓
RevitParameterInfo
        ↓
DXSDK Parameter
```

Example:

```csharp
new Parameter(
    parameterName,
    parameterValue
);
```

---

## RevitGeometryService.cs

Handles geometry extraction.

Responsibilities:

### Geometry Extraction

```csharp
GeometryElement
```

### Geometry Traversal

```csharp
Solid
Mesh
Face
Edge
Curve
```

### Geometry Conversion

Converts Revit geometry into DXSDK compatible geometry.

Output:

```csharp
ElementGeometry
```

---

## RevitExchangeBuilderService.cs

Builds Data Exchange payload.

Responsibilities:

### Create ElementDataModel

```csharp
var dataModel =
    ElementDataModel.Create(Client);
```

### Create Elements

```csharp
dataModel.AddElement(...)
```

### Attach Geometry

```csharp
dataModel.SetElementGeometry(...)
```

### Attach Parameters

```csharp
await element.CreateInstanceParameterAsync(
    parameter
);
```

Produces final Exchange Data ready for synchronization.

---

# Data Flow

```text
Revit Document
      │
      ▼
Element Collector Service
      │
      ▼
Parameter Service
      │
      ▼
Geometry Service
      │
      ▼
Exchange Builder Service
      │
      ▼
ElementDataModel
      │
      ▼
SyncExchangeDataAsync
      │
      ▼
Autodesk Data Exchange
```

---

# Parameter Mapping

The connector extracts parameters from Revit and creates DXSDK parameters.

Example:

```csharp
var parameter =
    new Parameter(
        revitParameter.Name,
        revitParameter.Value
    );

await element.CreateInstanceParameterAsync(
    parameter
);
```

Current implementation supports:

- String Parameters
- Integer Parameters
- Double Parameters
- Boolean Parameters

---

# Geometry Support

Current geometry support includes:

### Mesh Geometry

- Revit Mesh
- DXSDK Mesh

---

# Exchange Operations

Supported operations:

### Create Exchange

```csharp
Client.CreateExchangeAsync(...)
```

### Update Exchange

```csharp
Client.SyncExchangeDataAsync(...)
```

### Download Exchange

```csharp
Client.GetElementDataModelAsync(...)
```

### Generate Viewable

```csharp
Client.GenerateViewableAsync(...)
```

---

# Revit Add-In Registration

The add-in is loaded using:

```xml
AdityaRevitDataExchange.addin
```

This file registers:

- Add-In Name
- Assembly Location
- Full Class Name
- Vendor Information

with Autodesk Revit.

---

# Dependencies

### Autodesk

- Autodesk Revit API
- Autodesk Revit API UI
- Autodesk Data Exchange SDK

### .NET

- .NET Framework 4.8

# Build Requirements

- Visual Studio 2022
- .NET Framework 4.8
- Revit 2027
- Autodesk Data Exchange SDK
- Autodesk Construction Cloud Account

---

# Current Features

✅ Revit Element Collection

✅ Geometry Extraction

✅ Parameter Extraction

✅ DXSDK Parameter Mapping

✅ Exchange Creation

✅ Exchange Synchronization

✅ Viewable Generation


# Author

**Aditya Sahu**

