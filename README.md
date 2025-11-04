# Inventory System & Robot Control & ISv4 - Database
  
The assignment was to implement an **inventory and order management system** using an object-oriented approach and a **GUI with DataGrid controls**.

The program allows the user to:
- Add new orders by selecting an item and quantity  
- Queue multiple orders for processing  
- Process the next order one at a time  
- Automatically update the **processed orders list** and **total revenue**  

The left DataGrid shows **queued orders**, and the right DataGrid shows **processed orders**.  
A button at the top allows the user to process the next order, and the total revenue is displayed beside it.

# Robot Control

In the extended version, the program is connected to a Universal Robots simulator (URSim) or a real UR robot through a network interface. How it works:
- The Robot.cs class establishes a TCP connection to the robot using two ports:
- 29999 for dashboard commands
- 30002 for URScript control commands
- When the user presses "Process Next Order", the system processes the next item in the order queue and automatically triggers the robot sequence.

The sequence is executed by calling _robot.RunSequence(), which sends a complete URScript program to the robot.
The robot performs a pick-and-place routine — picking an item from its source position and placing it in the shipment area.
All network communication is handled asynchronously, so the GUI remains responsive during robot motion.

# ISv4 - Database

This version of the Inventory System adds persistent storage using MariaDB/MySQL.
Orders and items are now stored in a real database instead of only memory.

Features:
- View available inventory items
- Add orders with selected items & quantity
- Queue orders for processing
- Process next order (moves it from Queued → Processed)
- Automatic revenue calculation
- All orders and order lines saved in the database
