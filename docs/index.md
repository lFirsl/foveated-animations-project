---
layout: default
title: Home
---

<script src="https://polyfill.io/v3/polyfill.min.js?features=es6"></script>
<script id="MathJax-script" async
        src="https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js"></script>

<ul>
  <li><a href="#">Florin-Vladimir Stancu</a>, University of Leeds, United Kingdom</li>
  <li><a href="#">Tomer Weiss</a>, New Jersey Institute of Technology, United States of America</li>
  <li><a href="#">Rafael Kuffner Dos Anjos</a>, University of Leeds, United Kingdom</li>
</ul>

<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/Foveated Animations - Dynamic - No UI.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

# Presentation Video
<div style="text-align: center;">
    <iframe width="640" height="360" src="https://www.youtube-nocookie.com/embed/Z1_eHE3xyP0?si=lKhk9SZQvVcnwJlU" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>
</div>
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

Under normal circumstances, the **Animation Update Frequency** (AUF) of an agent - that is, the amount of times per second an
agent on screen has its animations updated, measured in hertz - is equal to the frames-per-second (FPS) that the scene operates at. 
In other words, all animations on screen are updated every frame.

Our approach to foveating these animations then is to directly control the AUF of agents on screen depending on their
distance from the user's focus point.

<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/Foveation Example.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

# Unity Prototype

To explore this approach we implemented a Unity3D prototype of a crowd simulation using
typical scenarios.

### Wander Crowd
<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/WanderCrowd_NoFovea_Compressed.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

### Square Marathon
<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/SquareMarathon_NoFovea.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

### Parallel Columns
<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/ParallelColumns_NoFovea.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

### Intercepting Crowds
<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/InterceptingCrowds_NoFovea.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

### T-Intercept
<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/TIntercept_NoFovea.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

# Approach

We've experimented with two approaches using our unity prototype. For both, everything within a threshold 
around the focus point, dubbed the "foveal area", is animated as normal. The difference between them lies 
in how agents outside the area are handled:

- **Full Stop:** everything outside the foveal area has it's animations fully halted.

- **Dynamic Foveation:** agents outside the foveal area have their AUF updated based on their distance from the focus point.

Details for both can be seen below.

## Full Stop Method
The Full Stop method works with a single threshold. Everything within it is animated as normal, and everything outside of it
has its animations fully halted.

The hypothesis for this method is that, due to the crowding effect, the agents whose animations are halted will not be observed
by the user.

Example video can be seen below. Mind that:
- Agent coloured red acts as stand-in focus point.
- Agents highlighted green are those within the assigned foveal area, and are fully animated.
- Agents without any highlight have their animations fully halted.

<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/Foveated Animations - Full Stop Mean Values Presentation.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

## Dynamic Foveation Method
The Dynamic Foveation Method works as follows:
1. Everything within the foveal area is animated as normal.
2. Everything outside the foveal area has its AUF changes based on this formula:

$$
R_i = \frac{R_t}{\max(1, \alpha \cdot \|p - p_0\|^2 - a_f)}
$$

Where:
- $$R_i$$ — Individual agent's AUF/animation rate.
- $$R_t$$ — Target framerate (e.g. maximum monitor refresh rate or scene FPS cap)
- $$𝛼$$ — Foveation Factor
- $$p$$ — Agent's Position
- $$p_0$$ — Focus Point's Position


Note that **agents with a calculated AUF of 5hz or lower have their animations halted instead.** This is
to avoid creating excessive flicker in the user's periphery.

Example video can be seen below. Mind that:
- Agent coloured red acts as stand-in focus point.
- Agents highlighted green are those within the assigned foveal area, and are fully animated.
- Agents highlighted red-to-grey have their AUF calculated using the formula above
- Agents without any highlight have their animations fully halted.

<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/Foveated Animations - Dynamic Foveation Mean Values Presentation.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

# Original User Test Video

<iframe width="640" height="360" src="https://www.youtube-nocookie.com/embed/0TlGL1wiBAY?si=Bn0RymPKVei7tO0_" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>


