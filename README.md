# MobileVRController

![gif](http://myamens.com/Uploads/MobileVRController/V1.gif)
![gif](http://myamens.com/Uploads/MobileVRController/V2.gif)
![gif](http://myamens.com/Uploads/MobileVRController/V3.gif)

This project is a simple and easy way to hook your smart phone into your Unity project and use it as a remote controller.

Usage:
------
First build the project to your mobile device (iOS/Android) using Sender scene file. This file has only a single object that handles mobile sensors data sending. 
In your project, add ServiceManager object, and set IsReceiver property to true, also configure the IP address property to points to mobile's IP address (Make sure both the mobile and PC are connected on the same WiFi network). You can now register to ServiceManager.OnValueChanged delegate to receive notificaitions about new data arrival.
Before running Unity application, make sure the mobile app is running (ready to get connections). 



Features:
---------
- Send Realtime sensors information to client device
- Easy to add new service provides for data mapping
- Event based system to notify when new data arrives

Current Service Providers:
--------------------------
- GyroServiceProvider: provides realtime gyroscope information
- AccelServiceProvider: provides realtime acceleration information
- TouchServiceProvider: provides touch points with the pressure information from the touch screen
- SwipeServiceProvider: provides swiping direction
- (new)FeedbackServiceProvider: provides feedback (vibration) to mobile side

Expanding Support:
------------------
You can expand the number of services provided by the mobile by implementing IServiceProvider class. Study the previous classes as examples of how to implement it.
