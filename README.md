# Master Duel Rush Frames

![C#](https://img.shields.io/badge/C%23-239120?logo=c-sharp&logoColor=white)
![Unity](https://img.shields.io/badge/Unity-100000?logo=unity&logoColor=white)
![.NET](https://img.shields.io/badge/.NET_6-5C2D91?logo=.net&logoColor=white)
![MelonLoader](https://img.shields.io/badge/MelonLoader-4CAF50)
![Harmony](https://img.shields.io/badge/Harmony-FF6B6B)

A [MelonLoader](https://github.com/LavaGang/MelonLoader) and [Harmony](https://github.com/pardeike/Harmony) mod for 
Yu-Gi-Oh! Master Duel that attempts to replace the standard card frames with Yu-Gi-Oh! Rush Duel card frames.

<p align="center">
<a href="https://ibb.co/DfZDSdT4"><img src="https://i.ibb.co/TMsBFydc/cards.jpg" alt="cards" border="0"></a>
</p>

## Table of Contents

- [Description](#description)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Known Issues](#known-issues)
- [Possible Future Improvements](#possible-future-improvements)
- [Development](#development)
- [License](#license)
- [Credits](#credits)

## Description

This mod transforms the visual appearance of cards in Yu-Gi-Oh! Master Duel by applying the card frame style used in 
Yu-Gi-Oh! Rush Duel. It repositions and rescales various card elements to match the Rush Duel aesthetic, including card artwork, level indicators, text areas, and attribute icons.

## Features

- **Card Frame Transformation**: Converts Master Duel card frames to Rush Duel style
- **Element Repositioning**: Automatically adjusts positions of:
  - Card artwork (moved and scaled)
  - Level stars (hidden and replaced with a numeric display)
  - Card name positioning
  - Effect text area
  - ATK/DEF values
  - Attribute icons
  - Spell/Trap type indicators
- **Level Display**: Replaces traditional level stars with numeric level display

## Requirements

- **Yu-Gi-Oh! Master Duel**
- **MelonLoader**
- **.NET Framework 6** support

## Installation

1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader) for Yu-Gi-Oh! Master Duel
2. Download the latest release in the [releases](https://github.com/Nauder/ygomd-rush-frames/releases) tab
3. Place the `MasterFaceEditor.dll` file in your MelonLoader `Mods` folder
4. Replace the card frames in the game with the ones provided in the `res` folder
    - The release includes a GUI app to automate this process, but it can be done manually as well
      - The GUI app source code and the card frames can be found in the [installer repository](https://github.com/Nauder/ygomd-rush-installer)
    - This can also be done manually with a GUI modding tool, such as [Floowandereeze & Modding](https://github.com/Nauder/floowandereeze-and-modding-qt)
    - Alternatively, you can manually replace the card frames in the game's `data.unity3d` file using a standard Unity Modding tool, such as [UABEA](https://github.com/nesrak1/UABEA)
5. Launch Yu-Gi-Oh! Master Duel

## Usage

The mod automatically applies the Rush Duel card frame style when viewing cards in-game. 
No additional configuration is required.

<p align="center">
<a href="https://ibb.co/TDT72HH0"><img src="https://i.ibb.co/gb7f9DDW/in-game.png" alt="in-game" border="0"></a>
</p>

## Known Issues

- Rarity effects are currently removed rather than adapted to the new frame style
- Some card types may require additional positioning adjustments
- The card frame replacement may need to be redone after major game updates
- There is no Ritual Pendulum support yet, a frame would need to be created for it
- The Pendulum Monster frame cuts a lot of the card artwork off, so some cards may look a little off

## Possible Future Improvements

As a disclaimer, I am not a C# developer, nor a game developer, so this project may not be the most efficient or
elegant way to achieve the task. If you have any suggestions or improvements, please feel free to open an issue
or submit a pull request.

- Add support for Rarity effects
- Add support for Pendulum Ritual monsters
- Adapt Link Monsters to use the proposed design from Phanthelia's Template
- Adapt Pendulum Monsters to use the proposed design from Phanthelia's Template
- Change Spell Cards so they have their type written instead of an icon, as they currently do in Rush Duel
- Make the mod automatically replace the card frames in the game's `data.unity3d` file

## Development

The .csproj file must use the references from your Master Duel MelonLoader folder.

- The project uses Melon Loader as the modding framework
- Harmony Patcher is used for patching some methods in the game code
- Currently, development is focused on Windows, as that is the main supported desktop OS for Master Duel

## License
This project is licensed under the terms specified in the LICENSE file

## Credits

- [Phanthelia](https://www.deviantart.com/phanthelia) for the original Rush Duel card frame template used in this mod