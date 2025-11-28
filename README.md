This is Temi's version of Ricochet, the GDW game made by Bounce Back

External resources:

The color correction shader is the one provided in the canvas files.
The script for the color correction camera is the one provided in the lecture 5 slides.


Shaders:

Afterimage Clone:

Uses a transparency shader variation of a diffuse shader. The after image effect also gives the object a little bit of a flicker

This is done by multiplying the final color by a value power, which is a random value between minPulse and maxPulse, with Time used as a seed for consistent random numbers.

This is done to create a sense of instability in the afterimage's appearance, showing the player that the afterimage is temporary. 

https://youtu.be/2PpNJB_RwSk

Foresight Circle :

Uses a rim shader but with a transparent center.

This is done by supplying the alpha of the final product using the result from the saturated rim shader.

This is done to make the circle look better, as the mesh clipping through your model was not visual appealing.

https://github.com/Loluwa0006/GDW-III/releases/edit/itcg-course-project 

Afterimage Decal :

First I get the world space position using the scene depth, the screen position and an inverse view projection matrix

I then apply the world position to object position so i can track which pixels are overlapping with another mesh

THis is done by using a step node: returns 0 if pixel falls outside of mesh, otherwise returns 1

I also use a swizzle to project onto the xz axis, adding 0.5 to both X and Y to move the UV to a desired position

Lastly I sample the decal texture, and I use the step node's result as a scaler to determine which parts of the decal should or shouldn't be drawn

www.youtube.com/watch?v=-XXl2o-oQAU

Improvements:

2 new skills, Pivot and Takeback: increase variety of available abilities
New lighting: makes the world feel a bit more dynamic
New grass textures, model: also helps in making world mroe dynamic
Annoucement system: Helps to allow players to get ready before match officially begins

Sources:

Decal Tutorial: https://www.youtube.com/watch?v=f7iO9ernEmM



Link to presentation video:

https://youtu.be/9tJeTAuxapU 
