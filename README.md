# BadPaint
While experimenting with the Windows Api I created a paint app that lets you draw right onto your screen which is controllable from the command-line.
# Use
Use the `paint` command with one or more of the following parameters

  `-help, -h` This shows a parameter list like this one.
  
  `-brush, -b` You can use this to customize the brush. Any word works as a brush or you can use the presets: `square`, `circle`, `error`, `warning`, `info` or use `-file, -f` to    choose a image as a brush. The default brush is square.
  
  `-size, -s` You can use this to control the radius of the brush in px. The default value is 20px.
  
  `-colour -c` You can use this to change the colour of the brush. Most colours will work with this.
  
  `-time, -t` You can use this to control the amount of time you want to paint for in seconds. The maximum value is 30.
  
 Here is an exmaple of using the command:
 
 Using `paint -b warning -s 55 -t 30` will give you this:
 
 ![warningBrush](https://user-images.githubusercontent.com/94676987/155886699-256e134c-ae94-40ec-9820-8debf26186d1.png)
 
 
