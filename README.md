# Foveated Animations in the Context of Crowd Simulations.
This repository holds the Unity Prototype used in the "Foveated Animations for Efficient Crowd Simulation" paper, authored by Florin-Vladimir Stancu, Rafael Kuffner Dos Anjos and Tomer Weiss on 05/2025.

## What are Foveated Animations?
Similar to [Foveated Rendering](https://en.wikipedia.org/wiki/Foveated_rendering), Foveated Animations aim to reduce the quality of animations in the periphery of a user's field of view in such a way that the reduction in quality goes unnoticed by the user. The aim of this process is to save on computing power and allow a machine to run the same scene with an increased framerate without sacricifing user experience.

This project achieves this by using eye tracking to determine the user's gaze and then reduce the animation update frequency of agents running around the scene based on their distance from the user's gaze, until eventually animations are fully stopped for
agents that find themselves very far from the user's gaze.

The same effect can be replicated on a generic monitor without an eye tracker as well, however in this case a "focus point" needs to be allocated (that is, a point where we assume the user is consistently looking). This is done in this project by asigning a "red agent" that is easy to follow in every scene.

## How to run this repository
This project contains a fully fledged out Unity Project, developed on Unity Editor version 2022.3.19f1.

To run this project: 
1. Clone this repository
2. Open up the project from disk by using the Unity Hub
3. Run the project using Unity Editor Version 2022.3.19f1. Alternatively, you may be able to use any 2022.3.* version without facing compatability issues.

Upon loading this project with Unity, you can navigate to the `Assets > Scenes` to inspect all the scenes outlined within the project paper, with all necessary scripts attached and pre-set. You may run the scenes as they are, or change the settings of the "Focus Point Sphere" script attached to the "Protagonist" agents in each scene to experience different levels of foveation.

## User Test Video

If you only wish to see the foveation in effect, rather than run the project locally, you can instead check the user test video. This video includes all 4 scenes used during the original user study ran in March 2024.

You may find the video on:
- Youtube (Compressed): https://youtu.be/0TlGL1wiBAY
- OneDrive (Uncompressed - Requires download for highest quality): https://1drv.ms/v/s!AocnOYwxBpbGhdwbHGvkhCyv--w0fw?e=WmXz6Z
