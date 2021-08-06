# TriSculpt

A tool made to automatically triangulate pictures and then export them. The process uses Delaunay Triangulation and Shannon Entropy to detect points of interest in an image and then create a triangulation mesh from them. .png and .jpg are supported and the original image file will not be changed. The export image will be a png file. The program is developed using Unity. 

---

## Installation

[Download](https://drive.google.com/drive/folders/15Caaugp3H4eM1VS8gGigU5vRhe8rtcdg) and run the TriSculpt.exe within the folder to start the program. Since the program export images antiviruses might interfer. If you are unable to export or run the program properly try whitelisting triSculpt.exe.

## How to use
1. Press **Load Image** and select image to work on. 
1. Manipulate parameters for desired triangulation. Check *Parameters* for more info
1. Choose export folder by pressing **Export Folder** or leave at default which is *Desktop*
1. Export Image by pressing **Export Image**

## Screenshots

![pic-1](https://github.com/Zmarfan/TriSculpt/blob/main/readmePictures/1.jpg?raw=true)
![pic-2](https://github.com/Zmarfan/TriSculpt/blob/main/readmePictures/2.jpg?raw=true)
![pic-3](https://github.com/Zmarfan/TriSculpt/blob/main/readmePictures/3.jpg?raw=true)
![pic-4](https://github.com/Zmarfan/TriSculpt/blob/main/readmePictures/4.jpg?raw=true)

## Parameters
* **Detail Accuracy** - How accurately the tool identifies points of interest. A low value will find high detail but compression might fool it. Too high values and the tool will have trouble spotting any detail. Recommendation is to keep it high.
* **Amount of Points** - How many points (disregarding border points) should the triangulation have.
* **Border Point Amount** - How many points along each border should there be?
* **Point Influence Length** - How spread apart triangles should be. For low point count have a high Influence Length but for higher point count; decrease Influence Length to group triangles to high detail areas.
* **Point Influence Strength** - How much influence the Influence Length should have. Can usually be left at default. It is unpredictable.
* **Corner Color Sample Point** - Where should the sample points for the coloring be for each triangle.
