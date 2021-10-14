# Getting Started

The Unity Resource Generator is a Unity Editor tool which can generate a helper class which will create a helper class that contains paths to loadable Unity resources. Additionally it will also generate helper methods to load scenes.

The generation of the class is customizable by default and the library also lets developers create custom code to run during generation.

## Installation

Use [OpenUPM](https://openupm.com/) to install the package.

```
openupm add com.autsoft.unityresourcegenerator
```

## Running the tool

The tool will create new button in the Editor at `Tools / Generate Resource Paths`

![Generate Button](~/images/intro/GenerateButton.png)

If your click the button the helper class will be generated in the root of the `Assets` folder
