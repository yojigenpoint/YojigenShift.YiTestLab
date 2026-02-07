# YiTestLab 🔮
**​YiTestLab** (The YiXue Algorithm Laboratory) is a cross-platform visualization tool designed to test, demonstrate, and debug the **YiFramework**—a high-performance C# library for Chinese Metaphysics (YiXue) algorithms.

​Built with **Godot Engine 4 (.NET version)**, this project serves as both a testing ground for algorithms and a digital dictionary for metaphysical concepts.
> ​Note: This repository contains the UI and logic layer. It requires the core YiFramework library to function.
## ​✨ Features
​The laboratory includes 9 core modules for testing and visualization:
​1. **Five Elements (Wu Xing)**: Visualization of generating (Sheng) and overcoming (Ke) cycles.
1. **​Stems & Branches (GanZhi)**: Dictionary for Heavenly Stems and Earthly Branches with localized data.
1. **​Four Pillars (BaZi)**: Advanced plotting engine with solar time correction, Ten Gods (ShiShen), and Hidden Stems.
1. **​Chinese Naming**: (CN Only) Name generation based on Five Elements patterns and stroke analysis.
1. **​Eight Trigrams (BaGua)**: Visualization of Pre-heaven and Post-heaven Bagua arrangements.
​1. **Qi Men Dun Jia**: (WIP) Placeholder for upcoming advanced divination modules.
1. **​64 Hexagrams**: A complete digital index of the I Ching hexagrams.
​1. **Six Lines (Liu Yao)**: Full divination simulation including coin casting, hexagram mounting, and Six Beasts evaluation.
1. **​Plum Blossom Oracle**: Time-based and number-based divination systems.
## ​🛠️ Technical Stack
- **​Engine**: Godot Engine 4.x (.NET Version)
​- **Language**: C# 10+
- **​Core Dependency**: YojigenShift.YiFramework (Proprietary/Private NuGet)
- **​Target Platforms**: Windows Desktop, Android
## 🚀 Getting Started

### Prerequisites

* [Godot Engine 4.x (.NET version)](https://godotengine.org/download)
* .NET 9.0 SDK

### Installation

1.  Clone the repository:
    ```bash
    git clone [https://github.com/YourUsername/YojigenShift.YiTestLab.git](https://github.com/YourUsername/YojigenShift.YiTestLab.git)
    ```

2.  **Restore Dependencies:**
    This project relies on the core library `YojigenShift.YiFramework`, which is hosted on NuGet.
    * Simply **Build the solution** in Godot (or Visual Studio / VS Code).
    * The build system will automatically download and install the required library version.
    * *Troubleshooting:* If you encounter reference errors, try running `dotnet restore` in the project directory.

3.  Open the `project.godot` file in Godot Editor.
4.  Run the `Main.tscn` scene.
## 📱 Mobile Support
​This project supports exporting to **Android**.
- Responsive UI adapts to touch screens.
- ​"Screenshot & Report" feature works with native Android sharing intents (planned).
## ​🤝 Contributing
​This project is intended as a testbed. Issues and Pull Requests regarding UI/UX improvements are welcome. For algorithm-related bugs, please use the built-in "Feedback" tool within the app.
​## 📄 License
​The code in this repository is licensed under the MIT License.
> *Note: The referenced YiFramework library retains its own proprietary license.*

​Copyright © 2026 YojigenShift. All Rights Reserved.
