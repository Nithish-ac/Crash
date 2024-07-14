# Multiplayer 2D App with Photon and Agora

## Table of Contents
- [Project Overview](#project-overview)
- [Features](#features)
- [Setup and Installation](#setup-and-installation)
- [Usage](#usage)
- [Code Structure](#code-structure)
- [Functionality](#functionality)
- [Evaluation Criteria](#evaluation-criteria)
- [Additional Features](#additional-features)
- [Known Issues](#known-issues)
- [Future Improvements](#future-improvements)
- [Acknowledgments](#acknowledgments)

## Project Overview
This project is a multiplayer 2D application using Photon for networking and Agora for audio/video communication. Players are represented as circles with square triggers around them. When players collide, they can see and hear each other through Agora, and the communication disconnects when they stop colliding.

## Features
- **Multiplayer Interaction**: Smooth synchronization of player movements using Photon Fusion.
- **Collision-Based Communication**: Integration with Agora for real-time audio and video communication upon collision.
- **Dynamic Channel Management**: Randomly generated Agora channel names for each collision.
- **Multiple Player Interaction**: Support for more than two players in a single audio/video channel when they collide.

## Setup and Installation
1. Clone the repository:
    ```sh
    git clone https://github.com/yourusername/multiplayer-2d-app.git
    cd multiplayer-2d-app
    ```
2. Open the project in Unity.
3. Import the necessary packages:
    - Photon Fusion
    - Agora Unity SDK
4. Set up your Agora App ID in the appropriate script.
5. Build and run the project.

## Usage
- **Host a Game**: Click the "Host" button to start a new game session.
- **Move the Player**: Use the WASD keys or arrow keys to move your player.
- **Collision Communication**: Move your player to collide with another player to start audio/video communication.

## Code Structure
- **PlayerSpawner.cs**: Handles player spawning and game session management.
- **PlayerController.cs**: Manages player movement and collision-based communication.
- **InputController.cs**: Collects player input and manages network input callbacks.

## Functionality
### Player Spawning and Movement
- Players spawn at random positions within the game area.
- Smooth player movement using input controls with Photon Fusion for synchronization.

### Collision-Based Communication
- Integration with Agora for real-time audio and video communication upon player collision.
- Communication disconnects when players stop colliding.

## Evaluation Criteria
### Functionality
- The app replicates the features demonstrated in the provided video, including player movement and collision-based communication.

### Code Quality
- Clean, well-organized, and documented code.
- Proper use of Photon Fusion and Agora SDKs.

### Performance
- Smooth gameplay experience without significant lag or bugs.
- Efficient network operations handling.

### Creativity
- Dynamic Agora channel management.
- Support for multiple players in a single channel during collisions.

## Additional Features
- **UI Enhancements**: Visual feedback for player interactions and connection status.
- **Error Handling**: Proper error handling for network connections and Agora integration.

## Known Issues
- Minor synchronization delay in high-latency network conditions.
- Occasional disconnection issues in unstable network environments.

## Future Improvements
- Implement more robust error handling and reconnection logic.
- Enhance UI for better user experience.
- Add more interactive elements and gameplay features.

## Acknowledgments
- Special thanks to the Photon and Agora teams for their excellent SDKs and support.
- Gratitude to my peers and mentors for their guidance and feedback throughout the project.

