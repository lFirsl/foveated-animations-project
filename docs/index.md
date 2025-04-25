---
layout: default
title: Home
---
<ul>
  <li><a href="#">Florin-Vladimir Stancu</a>, University of Leeds, United Kingdom</li>
  <li><a href="#">Tomer Weiss</a>, New Jersey Institute of Technology, United States of America</li>
  <li><a href="#">Rafael Kuffner Dos Anjos</a>, University of Leeds, United Kingdom</li>
</ul>


<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="/assets/videos/Foveated Animations - Dynamic Foveation Mean Values Presentation.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

# Abstract

Foveated rendering techniques have seen recent development with the advent of commercial head-mounted
displays with eye-tracking capabilities. 

The main drive is to exploit particular features of our peripheral vision
that allow optimizing rendering pipelines, which allows using less computational effort where the human
visual system may be unaware of differences. 

Most efforts have been focused on simplifying spatial visual detail on areas not being focused on by adjusting acuity of shading models, sharpness of images, and pixel
density. 

However, other perception pipeline areas are also influential, particularly in certain purpose-specific
applications. In this paper, we demonstrate it is possible to reduce animation rates in crowd simulations up
to a complete stop for agents in our peripheral vision without users noticing the effect. 

We implemented a prototype Unity3D application with typical crowd simulation scenarios and carried out user experiments to
study subjects’ perception to changes in animation rates. 

We find that in the best case we were able to reduce the number of operations by 99.3% compared to an unfoveated scenario, with opportunities for developments
combined with other acceleration techniques. 

This paper also includes an in-depth discussion about human
perception of movement in peripheral vision with novel ideas that will have applications beyond crowd
simulation.

# Brief Introduction

In this paper we focus on exploring the perception of adaptive animation rates on crowd simulations. We propose the concept of foveated animations; adaptive rate of on-screen animations in the
viewer’s periphery to save system resources without the user experience
being impacted. 

We found that the way our peripheral vision reacts to [visual crowding](https://en.wikipedia.org/wiki/Visual_crowding)
where stimuli are grouped and perceived statistically can be heavily exploited when rendering
animated crowds. When thousands of characters need bone transformations, blending, and other
factors calculated per frame, one can skip these operations without any impact to the user experience if using the 
appropriate level of foveation. 

While work has been done to optimize animation
costs, there is yet to be any research on utilizing gaze information to optimize this process
without perceived loss of quality.



## Approach

Under normal circumstances, the **Animation Update Frequency** (AUF) of an agent - that is, the amount of times per second an
agent on screen has its animations updated, measured in hertz - is equal to the frames-per-second (FPS) that the scene operates at. 
In other words, all animations on screen are updated every frame.

Our approach to foveating these animations then is to directly control the AUF of agents on screen depending on their
distance from the user's focus point.

<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="/assets/videos/Foveation Example.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

## Unity Prototype

To explore this approach we implemented a Unity3D prototype of a crowd simulation using
typical scenarios.


# Original User Test Video

<iframe width="640" height="360" src="https://www.youtube-nocookie.com/embed/0TlGL1wiBAY?si=Bn0RymPKVei7tO0_" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>


