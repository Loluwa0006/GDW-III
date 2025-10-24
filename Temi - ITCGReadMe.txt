This is Temi's version of Ricochet, the GDW game made by Bounce Back

External resources:

The color correction shader is the one provided in the canvas files.
The script for the color correction camera is the one provided in the lecture 5 slides.


Shaders:

Afterimage :

Uses a transparency shader variation of a diffuse shader. The after image effect also gives the object a little bit of a flicker

This is done by multiplying the final color by a value power, which is a random value between minPulse and maxPulse, with Time used as a seed for consistent random numbers.

This is done to create a sense of instability in the afterimage's appearance, showing the player that the afterimage is temporary. 

Foresight Circle :

Uses a rim shader but with a transparent center.

This is done by supplying the alpha of the final product using the result from the saturated rim shader.

This is done to make the circle look better, as the mesh clipping through your model was not visual appealing.


Link to presentation video:

https://youtu.be/7vRzi2pYGcA