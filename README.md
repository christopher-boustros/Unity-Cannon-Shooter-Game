# Unity Cannon Shooter Game
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/christopher-boustros/Unity-Cannon-Shooter-Game)

A 2D cannon shooter game made with Unity in which the player must destroy balloons with cannonballs. This game was made as part of a course assignment for COMP 521 Modern Computer Games in fall 2020 at McGill University. The goal of the assignment was to implement Perlin noise, Verlet integration, physics, and collision detection manually, without the use of libraries.

No third-party assets are used in the game.

You can play the game on GitHub Pages [**HERE**](https://christopher-boustros.github.io/Unity-Cannon-Shooter-Game/)!

![Alt text](/Game_Screenshot.png?raw=true "Game Screenshot")

## How to run the game

#### Requirements

You must have Unity version 2019.4.9f1 installed on your computer. Other versions of Unity may have compatibility issues.

#### Running the game in Unity

Clone the master branch of this repository with `git clone --single-branch https://github.com/christopher-boustros/Unity-Cannon-Shooter-Game.git`, or alternatively, download and extract the ZIP archive of the master branch. 

Open the Unity Hub, click on the Projects tab, click on the ADD button, and select the root directory of this repository.

Click on the project to open it in Unity.

In the Project window, double click on the `MainScene.unity` file from the `Assets/Scenes` folder to replace the sample scene.

Click on the play button to play the game.

## Game features

#### Movement
Up arrow = move cannon barrel upwards

Down arrow = move cannon barrel downwards

Left arrow = decrease muzzle velocity

Right arrow = increase muzzle velocity

Space bar = shoot a cannonball

Tab key = switch cannons

#### Perlin noise
The detail on the surface of the ground and water is randomly generated one-dimensional Perlin noise. The benefit of generating height values using Perlin noise instead of pseudo or true-random numbers is that each Perlin noise value is close to its previous value, making the terrain look smooth and continuous. 

#### Verlet integration
The motion of a balloon's body and string is modelled using Verlet integration and constraints between points. With Verlet integration, the previous position of a point on the balloon is used to determine its current position, allowing for a realistic-looking 2D simulation of movement.

#### Cannonball collisions
Cannonballs bounce upon collision with the ground and disappear upon collision with the water. When a cannonball hits a balloon body, the balloon disappears, and when a cannonball hits a balloon string, the string moves such that it does not intersect with the cannonball.

#### Wind
There is wind which changes velocity every 2 seconds. The balloons move in the direction of the wind and are destroyed when they move off screen.

## References

- The implementation of the `UpdateOrthographicCameraSize` method in `CameraScaler.cs` is inspired by [this source](https://pressstart.vip/tutorials/2018/06/14/37/understanding-orthographic-size.html).

- The implementation of the `ApplyVerletIntegration` and `ApplyStringSegmentLengthConstraint` methods in `VerletBalloon.cs` is inspired by [this source](https://www.youtube.com/watch?v=FcnvwtyxLds&ab_channel=YangHoDoo).

- The method of implementing a Perlin noise function in `PerlinNoise.cs` is based on [this source](https://www.cs.umd.edu/class/spring2018/cmsc425/Lects/lect12-1d-perlin.pdf).

- The method of implementing procedural terrain generation in `ProceduralTerrainGeneration.cs` is based on [this source](https://www.youtube.com/watch?v=-5OS1s-NWRw&ab_channel=ChronoABI).

No code in this repository is copied from the above sources.

## License

This repository is released under the [MIT License](https://opensource.org/licenses/MIT) (see LICENSE).
