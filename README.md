# Roombacs
Roomba control library for C# / .NET Framework

# Getting Started

## 1. Add namespace
```cs
using Roombacs;
```

## 2. Create instance
```cs
// Default (Default baud rate => 115200)
RoombaControl rc = new RoombaControl("COM3"); // specify the serial port roomba connected

// If you specify the baud rate, you can use constructor below instead of above
RoombaControl rs = new RoombaControl("COM3", 9600);
```

## 3. Initialize the Roomba
```cs
rc.Power(); // turn on the power of Roomba
rc.Start(); // prepare the Roomba to receive commands
rc.SetOIMode(OIMode.SAFE); // Set OI Mode of the Roomba
```

## 4. Send Actuator Commands
```cs
// Go ahead for five seconds and stop
rc.GoAhead(300); // you must specify the speed of Roomba in mm/s unit
System.Threading.Thread.Sleep(5000);
rc.Stop(); // stop the Roomba
```


