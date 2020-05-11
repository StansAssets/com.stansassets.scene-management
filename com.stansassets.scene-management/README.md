# Scene Management
Provides a set of tools to organize scene loading and management in the Unity project.

[![NPM Package](https://img.shields.io/npm/v/com.stansassets.scene-management)](https://www.npmjs.com/package/com.stansassets.scene-management)
[![openupm](https://img.shields.io/npm/v/com.stansassets.scene-management?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.stansassets.scene-management/)
[![Licence](https://img.shields.io/npm/l/com.stansassets.scene-management)](https://github.com/StansAssets/com.stansassets.scene-management/blob/master/LICENSE)
[![Issues](https://img.shields.io/github/issues/StansAssets/com.stansassets.scene-management)](https://github.com/StansAssets/com.stansassets.scene-management/issues)

<!-- Add some useful links here -->

[API Reference](https://myapi) | [Forum](https://myforum) | [Wiki](https://github.com/StansAssets/com.stansassets.scene-management/wiki)

### Install from NPM
* Navigate to the `Packages` directory of your project.
* Adjust the [project manifest file](https://docs.unity3d.com/Manual/upm-manifestPrj.html) `manifest.json` in a text editor.
* Ensure `https://registry.npmjs.org/` is part of `scopedRegistries`.
  * Ensure `com.stansassets` is part of `scopes`.
  * Add `com.stansassets.scene-management` to the `dependencies`, stating the latest version.

A minimal example ends up looking like this. Please note that the version `X.Y.Z` stated here is to be replaced with [the latest released version](https://www.npmjs.com/package/com.stansassets.foundation) which is currently [![NPM Package](https://img.shields.io/npm/v/com.stansassets.scene-management)](https://www.npmjs.com/package/com.stansassets.scene-management).
  ```json
  {
    "scopedRegistries": [
      {
        "name": "npmjs",
        "url": "https://registry.npmjs.org/",
        "scopes": [
          "com.stansassets"
        ]
      }
    ],
    "dependencies": {
      "com.stansassets.scene-management": "X.Y.Z",
      ...
    }
  }
  ```
* Switch back to the Unity software and wait for it to finish importing the added package.

### Install from OpenUPM
* Install openupm-cli `npm install -g openupm-cli` or `yarn global add openupm-cli`
* Enter your unity project folder `cd <YOUR_UNITY_PROJECT_FOLDER>`
* Install package `openupm add com.stansassets.scene-management`
