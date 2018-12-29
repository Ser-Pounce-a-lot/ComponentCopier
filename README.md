# Component Copier
The ComponentCopier is a simple script that copies the Components from one GameObject onto another. This script can save a lot of time when working with characters inside Unity.

For example, there is a character that has a lot components, but modifications to the underlying model need to be made. Normally after importing the new model into the scene, all the components would have to be added again manually. This script allows the copying of the components from the old version to the new version of the model, with the use of a simple interface.

## How to install
There are two ways to install this package.

##### Installation through the Unity Store
1. In Unity open the Asset Store by going to Window > Asset Store.
2. Enter Component Copier in the search box and select it.
3. Click on the Download/Import button.

##### Installation via UnityPackage
1. Download the UnityPackage from the [Releases page](https://github.com/Ser-Pounce-a-lot/ComponentCopier/releases).
2. Inside Unity go to Assets > Import Package > Custom Package.
3. Navigate to the UnityPackage that was downloaded earlier and select it.
4. A small dialog will pop up. Import the package by clicking Ok at the bottom.

##### Installation via GitHub/ZIP file
1. Download the ZIP file from the [Releases page](https://github.com/Ser-Pounce-a-lot/ComponentCopier/releases).
2. Unzip the contents into the Assets folder of the Unity Project.


## How to use
1. Inside Unity open the Component Copier by going to Tools > Component Copier > Open Component Copier.
2. Drag and drop the old and new models from the scene hierarchy into the fields.
3. Click on the Check components button.
4. A list with source and destination GameObjects will appear and also which components will be copied over. If everything is correct then skip to step 7.
5. Certain components or GameObjects can be excluded from the copying process by checking or unchecking them.
6. If a destination GameObject is incorrect or empty. It can be changed by dragging and dropping in the correct GameObject from the scene hierarchy.
7. Click the Copy components button to start the copy process.
8. After a few seconds, a message will appear and display how many components were copied. All the listed components have now been copied.

## Blacklisting components
In some occasions you want certain component types to be ignored. The blacklist, by default, already contains a few types of components that are ignored. 

1. To modify the blacklist go to Tools > Component Copier > Edit Blacklist.
2. It will now show the blacklist inside the Inspector panel. When adding new exclusions, keep in mind that the names are case-sensitive.
