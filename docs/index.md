---
layout: default
title: Foveated Animations
---

<script src="https://polyfill.io/v3/polyfill.min.js?features=es6"></script>
<script id="MathJax-script" async
        src="https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js"></script>

<script>
document.addEventListener("DOMContentLoaded", function () {
  const videos = document.querySelectorAll('video[data-autoplay]');

  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      const video = entry.target;
      if (entry.isIntersecting) {
        video.play();
      } else {
        video.pause();
      }
    });
  }, {
    threshold: 0.3
  });

  videos.forEach(video => observer.observe(video));
});
</script>

<ul>
  <li><a href="https://github.com/lFirsl">Florin-Vladimir Stancu</a>, University of Leeds, United Kingdom</li>
  <li><a href="#">Tomer Weiss</a>, New Jersey Institute of Technology, United States of America</li>
  <li><a href="https://github.com/rafaelkuffner">Rafael Kuffner Dos Anjos</a>, University of Leeds, United Kingdom</li>
</ul>
<p style="text-align: center;">
  <a href="">Preprint</a> |
  <a href="#">BibTeX</a> |
  <a href="https://github.com/lFirsl/foveated-animations-project">Unity Prototype</a>
</p>

<video autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/Foveated Animations - Dynamic - No UI.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

# Note - WIP
This website is still a **_Work In Progress_**. A finalized version is expected to be done by the **6th of May, 2025.**


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


# Unity Prototype

To explore this approach we implemented a Unity3D prototype of a crowd simulation using
typical scenarios.

### Wander Crowd
<video preload="metadata" data-autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/WanderCrowd_NoFovea_Compressed.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

### Square Marathon
<video preload="metadata" data-autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/SquareMarathon_NoFovea.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

### Parallel Columns
<video preload="metadata" data-autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/ParallelColumns_NoFovea.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

### Intercepting Crowds
<video preload="metadata" data-autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/InterceptingCrowds_NoFovea.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

### T-Intercept
<video preload="metadata" data-autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/TIntercept_NoFovea.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

# Approach


Under normal circumstances, the **Animation Update Frequency** (AUF) of an agent - that is, the amount of times per second an
agent on screen has its animations updated, measured in hertz - is equal to the frames-per-second (FPS) that the scene operates at.
In other words, all animations on screen are updated every frame.

Our approach to foveating these animations then is to directly control the AUF of agents on screen depending on their
distance from the user's focus point.

<video preload="metadata" data-autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/Foveation Example.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

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

<video preload="metadata" data-autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/Foveated Animations - Full Stop Mean Values Presentation.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

## Dynamic Foveation Method
The Dynamic Foveation Method works as follows:
1. Everything within the foveal area is animated as normal.
2. Everything outside the foveal area has its AUF changed based on this formula:

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

<video preload="metadata" data-autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/Foveated Animations - Dynamic Foveation Mean Values Presentation.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

# Applications
## VR Scenarios
Recent consumer-level Head Mounted Displays (HMDs) introduce eye-tracking capabilities. One example of such an HMD is the
[VIVE Pro Eye](https://www.vive.com/sea/product/vive-pro-eye/overview/), which we have used during our user testing.

We believe foveated animations can be used in this scenario to achieve higher performance rendering within the video games and entertainment industries.

As part of our evaluation, we have performed VR tests on our participants and have found little to no detriment to their
experience navigating through a modified wander crowd scene with foveated animations enabled.


Below you can see a video of foveated animations in action in a VR scene. The thresholds have been made higher than necessary
to allow for observation of the foveation and eye-tracking taking place, but in practice these thresholds can be made much lower.

<video preload="metadata" data-autoplay muted loop playsinline style="max-width: 100%; height: auto;">
  <source src="{{ site.baseurl }}/assets/videos/VR_Video.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

