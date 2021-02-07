This project contains submodules. To pull while updating all submodules, run
> git submodule update --recursive --remote

# Controls  

<b>HTC Vive</b>  
Engage/disengage drawing/erasing: Trigger  
Next brush (proejction method): Click on the right side of thumbpad  
Previous brush: Click on the left side of thumbpad  
Switch b/w draw and erase: Menu button  
Change direction of projection (for Spray brush): Choose by touching the thumbpad  
Move to next mesh: Grab button  

<b>Oculus Touch</b>  
Engage/disengage drawing/erasing: Index finger Trigger  
Next brush (proejction method): Hold Middle finger trigger and move D-pad to the right  
Previous brush: Hold Middle finger trigger down and move D-pad to the left  
Switch b/w draw and erase: A or X button  
Change direction of projection (for Spray brush): Move D-pad  
Move to next mesh: B or Y button


## ~~Fix for weird "Missing Script" error~~

~~1. On `NewCameraRig`>`Controller (left)` and `NewCameraRig`>`Controller (right)`, add the script "SteamVR_Behaviour_Pose". Set the "Input Source" variable in the script to `Left Hand` and `Right Hand`, resp.~~

~~2. The `Models`>`bunny` might report a missing prefab. Delete it and pull the prefab from `Assets/Models`. Add a `Mesh Collider` component to it, and set the mesh to be the `default` mesh in the prefab. Now, disable this gameobject in the Inspector.~~

~~3. For all the models (children of `Models`), add the script `ModelInfo` and set the name variable to be the same as the name of the object. e.g, the first one should be `bigbuckbunny`.~~

~~4. For `bigbuckbunny` and `head`, set the `Load Inside Offset Surface` variable to `True`.~~
