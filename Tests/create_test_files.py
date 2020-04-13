import os

from maya import standalone
standalone.initialize(name='python')

from maya import cmds
from maya.OpenMaya import MGlobal

folder = R'C:\repositories\MayaLauncher\Tests\Files'

types = [
    ('.ma', 'mayaAscii'),
    ('.mb', 'mayaBinary')
] 

apiVersion = str(MGlobal.apiVersion())

for type in types:
    extension = type[0]
    format = type[1]
    filename = os.path.join(folder, apiVersion + extension)
    filename_ref = os.path.join(folder, apiVersion + '_ref' + extension)
    
    cmds.file(new=True, force=True)
    cmds.file(rename=filename_ref)
    
    cmds.polyCube(name='cube')  
    
    cmds.file(type=format, save=True, force=True)  
    
    cmds.file(new=True, force=True)
    cmds.file(rename=filename)
    cmds.file(filename_ref, r=True, ns='ref' )
    
    cmds.loadPlugin( "stereoCamera", qt=True )
    from maya.app.stereo import stereoCameraRig
    rig = stereoCameraRig.createStereoCameraRig('StereoCamera')

    cmds.file(type=format, save=True, force=True)
   